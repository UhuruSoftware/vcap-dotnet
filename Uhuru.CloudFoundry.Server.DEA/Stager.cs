// -----------------------------------------------------------------------
// <copyright file="Stager.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using Uhuru.Utilities;

    class Stager
    {
        /// <summary>
        /// The stager lock.
        /// </summary>
        private object stagerLock = new object();

        /// <summary>
        /// Gets or sets the supported runtimes.
        /// </summary>
        public Dictionary<string, DeaRuntime> runtimes { get; set; }

        /// <summary>
        /// Gets or sets the droplet directory. 
        /// </summary>
        public string dropletDir { get; set; }

        /// <summary>
        /// Gets or sets the staged directory.
        /// </summary>
        public string stagedDir { get; set; }

        /// <summary>
        /// Gets or sets the apps directory.
        /// </summary>
        public string appsDir { get; set; }

        /// <summary>
        /// Gets or sets the database directory. It is where the snapshot app state saves the instances.
        /// </summary>
        public string dbDir { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only http should be used to get the application file.
        /// </summary>
        public bool forceHttpFileSharing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether not to clean up the directory cleanup.
        /// </summary>
        public bool disableDirCleanup
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stager"/> class.
        /// </summary>
        public Stager()
        {
            this.runtimes = new Dictionary<string, DeaRuntime>();
            if (!this.disableDirCleanup)
            {
                TimerHelper.RecurringCall(CleanCacheIntervalMs, delegate { this.CleanCacheDirectory(); });
            }
        }

        /// <summary>
        /// The timer to cleanup the application files cached.
        /// </summary>
        public const int CleanCacheIntervalMs = 20000;

        /// <summary>
        /// Cleans the cache directory.
        /// </summary>
        public void CleanCacheDirectory()
        {
            if (this.disableDirCleanup)
            {
                return;
            }

            lock (this.stagerLock)
            {
                try
                {
                    DirectoryInfo directory = new DirectoryInfo(this.stagedDir);
                    foreach (System.IO.FileInfo file in directory.GetFiles())
                    {
                        file.Delete();
                    }

                    foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories())
                    {
                        subDirectory.Delete(true);
                    }
                }
                catch (Exception e)
                {
                    Logger.Warning(Strings.CloudNotCleanCacheDirectory, e.ToString());
                }
            }
        }

        /// <summary>
        /// Checks weather the runtime is supported.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <returns>True if the runtime is supported.</returns>
        public bool RuntimeSupported(string runtime)
        {
            if (string.IsNullOrEmpty(runtime) || !this.runtimes.ContainsKey(runtime))
            {
                Logger.Debug(Strings.IgnoringRequestNoSuitableRuntimes, runtime);
                return false;
            }

            if (!this.runtimes[runtime].Enabled)
            {
                Logger.Debug(Strings.IgnoringRequestRuntimeNot, runtime);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Setups the runtimes.
        /// </summary>
        public void SetupRuntimes()
        {
            if (this.runtimes == null || this.runtimes.Count == 0)
            {
                Logger.Fatal(Strings.CannotDetermineApplicationRuntimes);
                throw new InvalidOperationException(Strings.CannotDetermineApplicationRuntimes);
            }

            Logger.Info(Strings.Checkingruntimes);

            foreach (KeyValuePair<string, DeaRuntime> kvp in this.runtimes)
            {
                string name = kvp.Key;
                DeaRuntime runtime = kvp.Value;
                
                // Only enable when we succeed
                runtime.Enabled = false;

                // Check that we can get a version from the executable
                string version_flag = string.IsNullOrEmpty(runtime.VersionFlag) ? "-v" : runtime.VersionFlag;

                string expanded_exec = Utils.RunCommandAndGetOutputAndErrors("where", runtime.Executable).Trim();

                expanded_exec = expanded_exec.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];

                if (!File.Exists(expanded_exec))
                {
                    Logger.Info(Strings.FailedExecutableNot, name, runtime.Executable, Directory.GetCurrentDirectory(), expanded_exec);
                    continue;
                }

                // java prints to stderr, so munch them both..
                string version_check = Utils.RunCommandAndGetOutputAndErrors(
                    expanded_exec, 
                    string.Format(CultureInfo.InvariantCulture, "{0} {1}", expanded_exec, version_flag)).Trim();

                runtime.Executable = expanded_exec;

                if (string.IsNullOrEmpty(runtime.Version))
                {
                    continue;
                }

                // Check the version for a match
                if (new Regex(runtime.Version).IsMatch(version_check))
                {
                    // Additional checks should return true
                    if (!string.IsNullOrEmpty(runtime.AdditionalChecks))
                    {
                        string additional_check = Utils.RunCommandAndGetOutputAndErrors(runtime.Executable, 
                            string.Format(CultureInfo.InvariantCulture, "{0}", runtime.AdditionalChecks));
                        if (!new Regex("true").IsMatch(additional_check))
                        {
                            Logger.Info(Strings.FailedAdditionalChecks, name);
                        }
                    }

                    runtime.Enabled = true;
                    Logger.Info(Strings.RuntimeOk, name);
                }
                else
                {
                    Logger.Info(Strings.FailedVersionMismatch, name, version_check);
                }
            }
        }

        /// <summary>
        /// Stages the app directory.
        /// </summary>
        /// <param name="bitsFile">The bits file.</param>
        /// <param name="bitsUri">The bits URI.</param>
        /// <param name="sha1">The sha1.</param>
        /// <param name="tgzFile">The TGZ file.</param>
        /// <param name="instance">The instance.</param>
        public void StageAppDirectory(string bitsFile, string bitsUri, string sha1, string tgzFile, DropletInstance instance)
        {
            // What we do here, in order of preference..
            // 1. Check our own staged directory.
            // 2. Check shared directory from CloudController that could be mounted (bits_file)
            // 3. Pull from http if needed.
            string InstanceDir = instance.Properties.Directory;

            lock (this.stagerLock)
            {
                // check before dowloading
                if (instance.Properties.StopProcessed)
                    return;

                if (File.Exists(tgzFile))
                {
                    Logger.Debug(Strings.FoundStagedBitsInLocalCache);
                }
                else
                {
                    // If we have a shared volume from the CloudController we can see the bits directly, just link into our staged version.
                    DateTime start = DateTime.Now;
                    if (!this.forceHttpFileSharing && File.Exists(bitsFile))
                    {
                        Logger.Debug(Strings.SharingCloudControllerStagingDirectory);
                        File.Copy(bitsFile, tgzFile);
                        Logger.Debug(Strings.TookXSecondsToCopyFromShared, DateTime.Now - start);
                    }
                    else
                    {
                        Logger.Debug(Strings.Needtodownloadappbitsfrom, bitsUri);

                        this.DownloadAppBits(bitsUri, sha1, tgzFile);

                        Logger.Debug(Strings.TookXSecondsToDownloadAndWrite, DateTime.Now - start);
                    }
                }

                // check before extracting
                if (instance.Properties.StopProcessed)
                {
                    return;
                }

                DateTime startStageing = DateTime.Now;

                // Explode the app into its directory and optionally bind its local runtime.
                Directory.CreateDirectory(InstanceDir);

                string tarFileName = Path.GetFileName(tgzFile);
                tarFileName = Path.ChangeExtension(tarFileName, ".tar");

                Utils.UnzipFile(InstanceDir, tgzFile); // Unzip
                Utils.UnzipFile(InstanceDir, Path.Combine(InstanceDir, tarFileName)); // Untar
                File.Delete(Path.Combine(InstanceDir, tarFileName));

                Logger.Debug(Strings.TookXSecondsToStageTheApp, DateTime.Now - startStageing);
            }
        }

        /// <summary>
        /// Downloads the app bits.
        /// </summary>
        /// <param name="BitsUri">The bits URI.</param>
        /// <param name="Sha1">The sha1 of the download.</param>
        /// <param name="TgzFile">The TGZ file.</param>
        private void DownloadAppBits(string BitsUri, string Sha1, string TgzFile)
        {
            WebClient client = new WebClient();

            try
            {
                string PendingTgzFile = Path.Combine(this.stagedDir, string.Format(CultureInfo.InvariantCulture, Strings.Pending, Sha1));
                client.DownloadFile(BitsUri, PendingTgzFile);
                File.Move(PendingTgzFile, TgzFile);
            }
            finally
            { 
                client.Dispose();
                }

            string FileSha1;
            using (Stream stream = File.OpenRead(TgzFile))
            {
                using (SHA1 sha = SHA1.Create())
                {
                    FileSha1 = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
            
            if (FileSha1.ToUpperInvariant() != Sha1.ToUpperInvariant())
            {
                Logger.Warning(Strings.DonlodedFileFromIs, BitsUri, FileSha1, Sha1);
                throw new InvalidOperationException(Strings.Downlodedfileiscorrupt);
            }
        }

        /// <summary>
        /// Create the necessary directories for the DEA process
        /// </summary>
        public void CreateDirectories()
        {
            try
            {
                Directory.CreateDirectory(this.dropletDir);
                Directory.CreateDirectory(this.stagedDir);
                Directory.CreateDirectory(this.appsDir);
                Directory.CreateDirectory(this.dbDir);
            }
            catch (Exception e)
            {
                Logger.Fatal(Strings.CannotCreateSupported, e.ToString());
                throw;
            }
        }
    }
}
