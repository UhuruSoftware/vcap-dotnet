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
        private object Lock = new object();

        public Dictionary<string, DeaRuntime> Runtimes { get; set; }

        public string DropletDir { get; set; }

        public string StagedDir { get; set; }

        public string AppsDir { get; set; }

        public string DbDir { get; set; }
            
        public bool ForceHttpFileSharing { get; set; }

        public bool DisableDirCleanup
        {
            get;
            set;
        }

        public Stager()
        {
            this.Runtimes = new Dictionary<string, DeaRuntime>();
            if (!this.DisableDirCleanup)
            {
                TimerHelper.RecurringCall(CleanCacheIntervalMs, delegate { this.CleanCacheDirectory(); });
            }
        }

        public const int CleanCacheIntervalMs = 20000;

        public void CleanCacheDirectory()
        {
            if (this.DisableDirCleanup)
            {
                return;
            }

            lock (this.Lock)
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
                catch (Exception e)
                {
                    Logger.Warning(Strings.CloudNotCleanCacheDirectory, e.ToString());
                }
            }
        }

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

        public void StageAppDirectory(string bitsFile, string bitsUri, string sha1, string tgzFile, DropletInstance instance)
        {
            // What we do here, in order of preference..
            // 1. Check our own staged directory.
            // 2. Check shared directory from CloudController that could be mounted (bits_file)
            // 3. Pull from http if needed.
            string InstanceDir = instance.Properties.Directory;

            lock (this.Lock)
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
                    if (!this.ForceHttpFileSharing && File.Exists(bitsFile))
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

                // consider: maybe not needed because of the new plugin system
                this.BindLocalRuntimes(InstanceDir, instance.Properties.Runtime);

                Logger.Debug(Strings.TookXSecondsToStageTheApp, DateTime.Now - startStageing);
            }
        }

        private void BindLocalRuntimes(string instanceDir, string runtime)
        {
            if (string.IsNullOrEmpty(instanceDir) || runtime != null)
            {
                return;
            }

            string startup = Path.GetFullPath(Path.Combine(instanceDir, "startup"));

            if (!File.Exists(startup))
            {
                return;
            }

            string startup_contents = File.ReadAllText(startup);
            string newStartup = startup_contents.Replace("%VCAP_LOCAL_RUNTIME%", this.Runtimes[runtime].Executable);

            if (string.IsNullOrEmpty(newStartup))
            {
                return;
            }

            File.WriteAllText(startup, newStartup);
        }

        private void DownloadAppBits(string BitsUri, string Sha1, string TgzFile)
        {
            WebClient client = new WebClient();
            string PendingTgzFile = Path.Combine(this.StagedDir, string.Format(CultureInfo.InvariantCulture, Strings.Pending, Sha1));
            client.DownloadFile(BitsUri, PendingTgzFile);
            File.Move(PendingTgzFile, TgzFile);
            client.Dispose();

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
                throw new Exception(Strings.Downlodedfileiscorrupt);
            }
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
                Directory.CreateDirectory(this.DbDir);
            }
            catch (Exception e)
            {
                Logger.Fatal(Strings.CannotCreateSupported, e.ToString());
                throw;
            }
        }
    }
}
