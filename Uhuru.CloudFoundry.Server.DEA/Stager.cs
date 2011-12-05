using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Security.Cryptography;


namespace Uhuru.CloudFoundry.DEA
{
    class Stager
    {

        private object Lock = new object();

        public Dictionary<string, DeaRuntime> Runtimes {get; set;}

        public string DropletDir {get; set;}
        public string StagedDir {get; set; }
        public string AppsDir {get; set;}
        public string DbDir {get; set;}
            
        public bool ForeHttpFileSharing{get; set; }

        public bool DisableDirCleanup
        {
            get;
            set;
        }

        public Stager()
        {
            Runtimes = new Dictionary<string, DeaRuntime>();
            if (!DisableDirCleanup)
            {
                TimerHelper.RecurringCall(CleanCacheIntervalMs, delegate(){ CleanCacheDirectory(); });
            }
        }

        public const int CleanCacheIntervalMs = 20000;

        public void CleanCacheDirectory()
        {
            if (DisableDirCleanup) return;
            lock (Lock)
            {
                try
                {
                    DirectoryInfo directory = new DirectoryInfo(StagedDir);
                    foreach (System.IO.FileInfo file in directory.GetFiles())
                        file.Delete();
                    foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories())
                        subDirectory.Delete(true);
                }
                catch (Exception e)
                {
                    Logger.warn(String.Format("Cloud not clean cache directory: {0}", e.ToString()));
                }
            }
        }

        public bool RuntimeSupported(string runtime)
        {
            if (String.IsNullOrEmpty(runtime) || !Runtimes.ContainsKey(runtime))
            {
                Logger.debug(String.Format("Ignoring request, no suitable runtimes available for '{0}'", runtime));
                return false;
            }

            if (!Runtimes[runtime].Enabled)
            {
                Logger.debug(String.Format("Ignoring request, runtime not enabled for '{0}'", runtime));
                return false;
            }

            return true;

        }

        public void GetRuntimeEnvironment()
        {
            throw new System.NotImplementedException();
        }

        public void SetupRuntimes()
        {
            if (Runtimes == null || Runtimes.Count == 0)
            {
                Logger.fatal("Can't determine application runtimes, exiting");
                throw new ApplicationException();
            }

            Logger.info("Checking runtimes...");

            foreach (KeyValuePair<string, DeaRuntime> kvp in Runtimes)
            {
                string name = kvp.Key;
                DeaRuntime runtime = kvp.Value;
                

                //  Only enable when we succeed
                runtime.Enabled = false;

                // Check that we can get a version from the executable
                string version_flag = String.IsNullOrEmpty(runtime.VersionFlag) ? "-v" : runtime.VersionFlag;

                string expanded_exec = Utils.RunCommandAndGetOutputAndErrors("where", runtime.Executable).Trim();

                expanded_exec = expanded_exec.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];

                if (!File.Exists(expanded_exec))
                {
                    Logger.info(String.Format("{0} FAILED, executable '{1}' not found \r\n Current directory: {2} \r\n Full executable path: {3}", name, runtime.Executable, Directory.GetCurrentDirectory(), expanded_exec));
                    continue;
                }

                // java prints to stderr, so munch them both..
                string version_check = Utils.RunCommandAndGetOutputAndErrors(expanded_exec, String.Format("{0}", expanded_exec, version_flag)).Trim();

                runtime.Executable = expanded_exec;

                if (String.IsNullOrEmpty(runtime.Version))
                {
                    continue;
                }

                // Check the version for a match
                if (new Regex(runtime.Version).IsMatch(version_check))
                {
                    // Additional checks should return true
                    if (!String.IsNullOrEmpty(runtime.AdditionalChecks))
                    {
                        string additional_check = Utils.RunCommandAndGetOutputAndErrors(runtime.Executable, String.Format("{0}", runtime.AdditionalChecks));
                        if (!(new Regex("true").IsMatch(additional_check)))
                        {
                            Logger.info(String.Format("{0} FAILED, additional checks failed", name));
                        }
                    }
                    runtime.Enabled = true;
                    Logger.info(String.Format("{0} OK", name));
                }
                else
                {
                    Logger.info(String.Format("{0} FAILED, version mismatch ({1})", name, version_check));
                }
            }
        }

        public void StageAppDirectory(string BitsFile, string BitsUri, string Sha1, string TgzFile, DropletInstance instance)
        {
            // What we do here, in order of preference..
            // 1. Check our own staged directory.
            // 2. Check shared directory from CloudController that could be mounted (bits_file)
            // 3. Pull from http if needed.
            string InstanceDir = instance.Properties.Directory;

            lock (Lock)
            {
                //check before dowloading
                if (instance.Properties.StopProcessed)
                    return;

                if (File.Exists(TgzFile))
                {
                    Logger.debug("Found staged bits in local cache.");
                }
                else
                {
                    //  If we have a shared volume from the CloudController we can see the bits directly, just link into our staged version.
                    DateTime start = DateTime.Now;
                    if (!ForeHttpFileSharing && File.Exists(BitsFile))
                    {
                        Logger.debug("Sharing cloud controller's staging directories");
                        File.Copy(BitsFile, TgzFile);
                        Logger.debug(String.Format("Took {0} to copy from shared directory", DateTime.Now - start));
                    }
                    else
                    {
                        Logger.debug(String.Format("Need to download app bits from {0}", BitsUri));

                        DownloadAppBits(BitsUri, Sha1, TgzFile);

                        Logger.debug(String.Format("Took {0} to download and write file", DateTime.Now - start));
                    }
                }

                //check before extracting
                if (instance.Properties.StopProcessed)
                    return;

                DateTime startStageing = DateTime.Now;

                // Explode the app into its directory and optionally bind its local runtime.
                Directory.CreateDirectory(InstanceDir);


                string tarFileName = Path.GetFileName(TgzFile);
                tarFileName = Path.ChangeExtension(tarFileName, ".tar");


                Utils.UnZipFile(InstanceDir, TgzFile); //Unzip
                Utils.UnZipFile(InstanceDir, Path.Combine(InstanceDir, tarFileName)); //Untar
                File.Delete(Path.Combine(InstanceDir, tarFileName));


                BindLocalRuntimes(InstanceDir, instance.Properties.Runtime);

                Logger.debug(String.Format("Took {0} to stage the app directory", DateTime.Now - startStageing));

            }

        }

        private void BindLocalRuntimes(string instanceDir, string runtime)
        {
            if (String.IsNullOrEmpty(instanceDir) || runtime != null)
                return;

            string startup = Path.GetFullPath(Path.Combine(instanceDir, "startup"));

            if (!File.Exists(startup))
            {
                return;
            }

            string startup_contents = File.ReadAllText(startup);
            string newStartup = startup_contents.Replace("%VCAP_LOCAL_RUNTIME%", Runtimes[runtime].Executable);

            if (String.IsNullOrEmpty(newStartup))
                return;

            File.WriteAllText(startup, newStartup);
        }

        private void DownloadAppBits(string BitsUri, string Sha1, string TgzFile)
        {
            WebClient client = new WebClient();
            string PendingTgzFile = Path.Combine(StagedDir, String.Format("{0}.pending", Sha1));
            client.DownloadFile(BitsUri, PendingTgzFile);
            File.Move(PendingTgzFile, TgzFile);

            string FileSha1;
            using (Stream stream = File.OpenRead(TgzFile))
            {
                FileSha1 = BitConverter.ToString(SHA1.Create().ComputeHash(stream)).Replace("-", string.Empty);
            }
            
            if(FileSha1.ToUpper() != Sha1.ToUpper()){
                Logger.warn(String.Format("Donloded file from {0} is corrupt: {1} != {2}", BitsUri, FileSha1, Sha1));
                throw new Exception("Downloded file is corrupt.");
            }
            
        }

        public void CreateDirectories()
        {
            try
            {
                Directory.CreateDirectory(DropletDir);
                Directory.CreateDirectory(StagedDir);
                Directory.CreateDirectory(AppsDir);
                Directory.CreateDirectory(DbDir);
            }
            catch (Exception e)
            {
                Logger.fatal(String.Format("Can't create supported directories: {0}", e.ToString()));
                throw e;
            }
        }
    }
}
