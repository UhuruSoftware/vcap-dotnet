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

    /// <summary>
    /// This class contains all the functionality required to download/unzip application bits and manages runtimes.
    /// </summary>
    public class Stager
    {
        /// <summary>
        /// The timer to cleanup the application files cached.
        /// </summary>
        public const int CleanCacheIntervalMilliseconds = 20000;

        /// <summary>
        /// The stager lock.
        /// </summary>
        private object stagerLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Stager"/> class.
        /// </summary>
        public Stager()
        {
            this.Runtimes = new Dictionary<string, DeaRuntime>();
            if (!this.DisableDirCleanup)
            {
                TimerHelper.RecurringCall(CleanCacheIntervalMilliseconds, delegate { this.CleanCacheDirectory(); });
            }
        }

        /// <summary>
        /// Gets or sets the supported runtimes.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "It is used for JSON (de)serialization.")]
        public Dictionary<string, DeaRuntime> Runtimes { get; set; }

        /// <summary>
        /// Gets or sets the droplet directory. 
        /// </summary>
        public string DropletDir { get; set; }

        /// <summary>
        /// Gets or sets the staged directory.
        /// </summary>
        public string StagedDir { get; set; }

        /// <summary>
        /// Gets or sets the apps directory.
        /// </summary>
        public string AppsDir { get; set; }

        /// <summary>
        /// Gets or sets the database directory. It is where the snapshot app state saves the instances.
        /// </summary>
        public string DBDir { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only http should be used to get the application file.
        /// </summary>
        public bool ForceHttpFileSharing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether not to clean up the directory cleanup.
        /// </summary>
        public bool DisableDirCleanup
        {
            get;
            set;
        }

        /// <summary>
        /// Create the necessary directories for the DEA process
        /// </summary>
        public void CreateDirectories()
        {
            try
            {
                Directory.CreateDirectory(this.DropletDir);
                Directory.CreateDirectory(this.StagedDir);
                Directory.CreateDirectory(this.AppsDir);
                Directory.CreateDirectory(this.DBDir);
            }
            catch (Exception e)
            {
                Logger.Fatal(Strings.CannotCreateSupported, e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Cleans the cache directory.
        /// </summary>
        public void CleanCacheDirectory()
        {
            if (this.DisableDirCleanup)
            {
                return;
            }

            lock (this.stagerLock)
            {
                try
                {
                    DirectoryInfo directory = new DirectoryInfo(this.StagedDir);
                    foreach (System.IO.FileInfo file in directory.GetFiles())
                    {
                        file.Delete();
                    }

                    foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories())
                    {
                        subDirectory.Delete(true);
                    }
                }
                catch (UnauthorizedAccessException e)
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
            if (string.IsNullOrEmpty(runtime) || !this.Runtimes.ContainsKey(runtime))
            {
                Logger.Debug(Strings.IgnoringRequestNoSuitableRuntimes, runtime);
                return false;
            }

            if (!this.Runtimes[runtime].Enabled)
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
            if (this.Runtimes == null || this.Runtimes.Count == 0)
            {
                Logger.Fatal(Strings.CannotDetermineApplicationRuntimes);
                throw new InvalidOperationException(Strings.CannotDetermineApplicationRuntimes);
            }

            Logger.Info(Strings.Checkingruntimes);

            foreach (KeyValuePair<string, DeaRuntime> kvp in this.Runtimes)
            {
                string name = kvp.Key;
                DeaRuntime runtime = kvp.Value;
                
                // Only enable when we succeed
                runtime.Enabled = false;

                // Check that we can get a version from the executable
                string version_flag = string.IsNullOrEmpty(runtime.VersionArgument) ? "-v" : runtime.VersionArgument;

                string expanded_exec = DEAUtilities.RunCommandAndGetOutputAndErrors("where", runtime.Executable).Trim();

                expanded_exec = expanded_exec.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];

                if (!File.Exists(expanded_exec))
                {
                    Logger.Info(Strings.FailedExecutableNot, name, runtime.Executable, Directory.GetCurrentDirectory(), expanded_exec);
                    continue;
                }

                // java prints to stderr, so munch them both..
                string version_check = DEAUtilities.RunCommandAndGetOutputAndErrors(
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
                        string additional_check = DEAUtilities.RunCommandAndGetOutputAndErrors(
                            runtime.Executable, 
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
        /// <param name="hash">The sha1.</param>
        /// <param name="tarZipFile">The TGZ file.</param>
        /// <param name="instance">The instance.</param>
        public void StageAppDirectory(string bitsFile, Uri bitsUri, string hash, string tarZipFile, DropletInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // What we do here, in order of preference..
            // 1. Check our own staged directory.
            // 2. Check shared directory from CloudController that could be mounted (bits_file)
            // 3. Pull from http if needed.
            string instanceDir = instance.Properties.Directory;

            lock (this.stagerLock)
            {
                // check before downloading
                if (instance.Properties.StopProcessed)
                {
                    return;
                }

                if (File.Exists(tarZipFile))
                {
                    Logger.Debug(Strings.FoundStagedBitsInLocalCache);
                }
                else
                {
                    // If we have a shared volume from the CloudController we can see the bits directly, just link into our staged version.
                    DateTime start = DateTime.Now;
                    if (!this.ForceHttpFileSharing && File.Exists(bitsFile))
                    {
                        Logger.Debug(Strings.SharingCloudControllerStagingDirectory);
                        File.Copy(bitsFile, tarZipFile);
                        Logger.Debug(Strings.TookXSecondsToCopyFromShared, DateTime.Now - start);
                    }
                    else
                    {
                        Logger.Debug(Strings.Needtodownloadappbitsfrom, bitsUri);

                        this.DownloadAppBits(bitsUri, hash, tarZipFile);

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
                Directory.CreateDirectory(instanceDir);

                string tarFileName = Path.GetFileName(tarZipFile);
                tarFileName = Path.ChangeExtension(tarFileName, ".tar");

                DEAUtilities.UnzipFile(instanceDir, tarZipFile); // Unzip
                DEAUtilities.UnzipFile(instanceDir, Path.Combine(instanceDir, tarFileName)); // Untar
                File.Delete(Path.Combine(instanceDir, tarFileName));

                Logger.Debug(Strings.TookXSecondsToStageTheApp, DateTime.Now - startStageing);
            }
        }

        /// <summary>
        /// Downloads the app bits.
        /// </summary>
        /// <param name="bitsUri">The bits URI.</param>
        /// <param name="sha1">The sha1 of the download.</param>
        /// <param name="tgzFile">The TGZ file.</param>
        private void DownloadAppBits(Uri bitsUri, string sha1, string tgzFile)
        {
            WebClient client = new WebClient();

            try
            {
                string pendingTgzFile = Path.Combine(this.StagedDir, string.Format(CultureInfo.InvariantCulture, Strings.Pending, sha1));
                client.DownloadFile(bitsUri, pendingTgzFile);
                File.Move(pendingTgzFile, tgzFile);
            }
            finally
            { 
                client.Dispose();
                }

            string fileSha1;
            using (Stream stream = File.OpenRead(tgzFile))
            {
                using (SHA1 sha = SHA1.Create())
                {
                    fileSha1 = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
            
            if (fileSha1.ToUpperInvariant() != sha1.ToUpperInvariant())
            {
                Logger.Warning(Strings.DonlodedFileFromIs, bitsUri, fileSha1, sha1);
                throw new InvalidOperationException(Strings.Downlodedfileiscorrupt);
            }
        }       
    }
}
