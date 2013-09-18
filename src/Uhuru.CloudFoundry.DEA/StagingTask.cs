// -----------------------------------------------------------------------
// <copyright file="StagingTask.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2013 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Uhuru.CloudFoundry.DEA.Messages;
using Uhuru.Configuration;
using Uhuru.Isolation;
using Uhuru.Utilities;

    class StagingTask
    {
        public delegate void StagingTaskEventHandler(Exception error);

        public event StagingTaskEventHandler AfterSetup;
        public event StagingTaskEventHandler AfterUpload;
        public event StagingTaskEventHandler AfterStop;

        public string TaskId { get; set; }
        public string StreamingLogUrl { get; set; }
        public string TaskLog { get { return File.ReadAllText(this.workspace.StagingLogPath); } }
        public string DetectedBuildpack { get { return StagingInfo.GetDetectedBuildpack(Path.Combine(this.workspace.StagedDir, StagingWorkspace.StagingInfo)); } }
        public string DropletSHA { get; set; }

        public StagingStartMessageRequest Message { get; set; }
        public StagingWorkspace workspace { get; set; }

        private ProcessPrison prison;
        private string buildpacksDir;
        private int stagingTimeout;
        private string gitExe;

        public StagingTask(StagingStartMessageRequest message)
        {
            UhuruSection uhuruSection = (UhuruSection)ConfigurationManager.GetSection("uhuru");

            this.TaskId = message.TaskID;
            this.Message = message;
            this.workspace = new StagingWorkspace(Path.Combine(uhuruSection.DEA.BaseDir, "staging"), message.TaskID);
            this.buildpacksDir = Path.GetFullPath(uhuruSection.DEA.Staging.BuildpacksDirectory);
            this.stagingTimeout = uhuruSection.DEA.Staging.StagingTimeoutMs;
            this.gitExe = Path.GetFullPath(uhuruSection.DEA.Staging.GitExecutable);

            var prisonInfo = new ProcessPrisonCreateInfo();
            prisonInfo.Id = this.TaskId;
            prisonInfo.TotalPrivateMemoryLimit = (long)this.Message.Properties.Resources.MemoryMbytes * 1024 * 1024;

            if (uhuruSection.DEA.UseDiskQuota)
            {
                prisonInfo.DiskQuotaBytes = (long)this.Message.Properties.Resources.DiskMbytes * 1024 * 1024;
                prisonInfo.DiskQuotaPath = this.workspace.BaseDir;
            }

            if (uhuruSection.DEA.UploadThrottleBitsps > 0)
            {
                prisonInfo.NetworkOutboundRateLimitBitsPerSecond = uhuruSection.DEA.UploadThrottleBitsps;
            }            
            
            this.prison = new ProcessPrison();
            prison.Create(prisonInfo);
        }

        public void Start() 
        {
            try
            {
                Logger.Info("Started staging task {0}", this.TaskId);
                StagingSetup();
                Staging();
                UploadDroplet();
                SaveBuildpackCache();
                AfterUpload(null);
            }
            catch(Exception ex)
            {
                Logger.Error(ex.ToString());
                AfterUpload(ex);
                throw ex;
            }
            finally
            {
                Logger.Info("Finished staging task {0}", this.TaskId);                
                Cleanup();
            }
        }        

        public void Stop()
        {
            // TODO stop task
            Logger.Info("Stopping staging task {0}", this.TaskId);
            AfterStop(null);
        }

        private void Cleanup()
        {
            if (this.prison.Created)
            {
                try
                {
                    Logger.Info("Destroying prison for staging instance {0}", this.TaskId);
                    this.prison.Destroy();
                }
                catch (Exception ex)
                {
                    Logger.Warning("Unable to cleanup application {0}. Exception: {1}", this.TaskId, ex.ToString());
                }
            }

            Logger.Debug("Cleaning up directory {0}", this.workspace.BaseDir);
            DEAUtilities.RemoveReadOnlyAttribute(this.workspace.BaseDir);
            Directory.Delete(this.workspace.BaseDir, true);  
        }

        private void StagingSetup()
        {
            try
            {
                DownloadApp();
                if (Message.BuildpackCacheDownloadURI != null)
                {
                    DownloadBuildpackCache();
                }
                Directory.CreateDirectory(new FileInfo(this.workspace.StagingLogPath).DirectoryName);
                if (!File.Exists(this.workspace.StagingLogPath))
                {
                    Logger.Info("Preparing staging log file {0}", this.workspace.StagingLogPath);
                    using (File.Create(this.workspace.StagingLogPath)) ;                    
                }
                AfterSetup(null);
            }
            catch (Exception ex)
            {
                Logger.Error("Error setting up staging environment: ", ex.ToString());                   
                AfterSetup(ex);
                throw ex;
            }            
        }

        private void Staging()
        {            
            UnpackApp();
            UnpackBuildpackCache();
            Stage();
            PackApp();
            
            Directory.CreateDirectory(this.workspace.StagedDropletDir);
            File.Copy(this.workspace.StagedDroplet, this.workspace.StagedDropletPath);

            using (Stream stream = File.OpenRead(this.workspace.StagedDropletPath))
            {
                using (SHA1 sha = SHA1.Create())
                {
                    this.DropletSHA = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        private void DownloadApp()
        {            
            WebClient client = new WebClient();
            try
            {
                Logger.Debug("Staging task {0}: Downloading app bits from {1}", this.TaskId, this.Message.DownloadURI);
                string tempFile = Path.Combine(this.workspace.WorkspaceDir, string.Format(CultureInfo.InvariantCulture, Strings.Pending, "droplet"));
                Uri uri = new Uri(this.Message.DownloadURI);
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(uri.UserInfo)));
                client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                client.DownloadFile(uri.ToString(), tempFile);
                File.Move(tempFile, this.workspace.DownloadDropletPath);
            }
            finally
            {
                client.Dispose();
            }
        }

        private void DownloadBuildpackCache()
        {
            WebClient client = new WebClient();
            try
            {
                Logger.Debug("Staging task {0}: Downloading buildpack cache", this.TaskId);
                string tempFile = Path.Combine(this.workspace.WorkspaceDir, string.Format(CultureInfo.InvariantCulture, Strings.Pending, "droplet"));
                Uri uri = new Uri(this.Message.BuildpackCacheDownloadURI);
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(uri.UserInfo)));
                client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                client.DownloadFile(uri.ToString(), tempFile);
                File.Move(tempFile, this.workspace.DownloadBuildpackCachePath);
            }
            catch
            {
                Logger.Error("Staging task {0}: Failed downloading buildpack cache", this.TaskId);
            }
            finally
            {
                client.Dispose();
            }
        }

        private void UnpackApp() 
        {

            Directory.CreateDirectory(this.workspace.UnstagedDir);
            DEAUtilities.UnzipFile(this.workspace.UnstagedDir, this.workspace.DownloadDropletPath);
        }

        private void UnpackBuildpackCache() 
        {
            Directory.CreateDirectory(this.workspace.Cache);
            if (File.Exists(this.workspace.DownloadBuildpackCachePath))
            {
                Logger.Debug("Staging task {0}: Unpacking buildpack cache {1}", this.TaskId, this.workspace.DownloadBuildpackCachePath);
                DEAUtilities.UnzipFile(this.workspace.Cache, this.workspace.DownloadBuildpackCachePath); // Unzip
                string tarFileName = Directory.GetFiles(this.workspace.DownloadBuildpackCachePath, "*.tar")[0];
                DEAUtilities.UnzipFile(this.workspace.Cache, Path.Combine(this.workspace.Cache, tarFileName)); // Untar
                File.Delete(Path.Combine(this.workspace.Cache, tarFileName));
            }
        }

        private void Stage() 
        { 
            string appDir = Path.Combine(this.workspace.StagedDir, "app");
            string logsDir = Path.Combine(this.workspace.StagedDir, "logs");
            string tmpDir = Path.Combine(this.workspace.StagedDir, "tmp");
            Directory.CreateDirectory(appDir);
            Directory.CreateDirectory(logsDir);
            Directory.CreateDirectory(tmpDir);
            DEAUtilities.DirectoryCopy(this.workspace.UnstagedDir, appDir, true);
            Buildpack buildpack = null;
            if (this.Message.Properties.Buildpack != null)
            {
                Logger.Info("Staging task {0}: Downloading buildpack from {1}", this.TaskId, this.Message.Properties.Buildpack);
                Directory.CreateDirectory(Path.Combine(this.workspace.TempDir, "buildpacks"));
                string buildpackPath = Path.Combine(this.workspace.TempDir, "buildpacks", Path.GetFileName(new Uri(this.Message.Properties.Buildpack).LocalPath));
                string command = string.Format("\"{0}\" clone --recursive {1} {2}", this.gitExe, this.Message.Properties.Buildpack, buildpackPath);
                int success = Command.ExecuteCommand(command);
                if (success != 0)
                {
                    throw new Exception("Failed to git clone buildpack");
                }
                buildpack = new Buildpack(buildpackPath, appDir, this.workspace.Cache, this.workspace.StagingLogPath);
                bool detected = buildpack.Detect(this.prison);
                if (!detected)
                {
                    throw new Exception("Buildpack does not support this application");
                }
            }
            else
            {
                Logger.Info("Staging task {0}: Detecting buildpack", this.TaskId);
                foreach (string dir in Directory.EnumerateDirectories(this.buildpacksDir))
                {
                    Buildpack bp = new Buildpack(dir, appDir, this.workspace.Cache, this.workspace.StagingLogPath);
                    bool success = bp.Detect(this.prison);
                    if (success)
                    {
                        buildpack = bp;
                        break;
                    }
                }

                if (buildpack == null)
                {
                    throw new Exception("Unable to detect a supported application type");
                }
                Logger.Info("Staging task {0}: Detected buildpack {1}", this.TaskId, buildpack.Name);

            }

            Logger.Info("Staging task {0}: Running compilation script", this.TaskId);
            buildpack.Compile(this.prison, this.stagingTimeout);

            Logger.Info("Staging task {0}: Saving buildpackInfo", this.TaskId);
            StagingInfo.SaveBuildpackInfo(Path.Combine(this.workspace.StagedDir, StagingWorkspace.StagingInfo), buildpack.Name, GetStartCommand(buildpack));

        }

        private void PackApp() 
        {
            Logger.Debug("Staging task {0}: Packing droplet {1}", this.TaskId, this.workspace.StagedDroplet);
            string tempFile = Path.ChangeExtension(this.workspace.StagedDroplet, "tar");
            DEAUtilities.TarDirectory(this.workspace.StagedDir, tempFile);
            DEAUtilities.GzipFile(tempFile, this.workspace.StagedDroplet);
            File.Delete(tempFile);
        }

        private string GetStartCommand(Buildpack buildpack)
        {
            if (this.Message.Properties.Meta != null)
            {
                if (this.Message.Properties.Meta.Command != null)
                {
                    return this.Message.Properties.Meta.Command;
                }
            }
            ReleaseInfo info = buildpack.GetReleaseInfo(this.prison);
            if (info.defaultProcessType != null)
            {
                if (info.defaultProcessType.Web != null)
                {
                    return info.defaultProcessType.Web;
                }
            }
            throw new Exception("Please specify a web start command in your manifest.yml");
        }

        private void UploadDroplet() 
        {
            Uri uri = new Uri(this.Message.UploadURI);
            Logger.Debug("Staging task {0}: Uploading droplet {1} to {2}", this.TaskId, this.workspace.StagedDroplet, this.Message.UploadURI);
            DEAUtilities.HttpUploadFile(this.Message.UploadURI, new FileInfo(this.workspace.StagedDropletPath), "upload[droplet]", "application/octet-stream", uri.UserInfo);
        }

        private void SaveBuildpackCache()
        {
            try
            {
                PackBuildpackCache();
            }
            catch
            {
                Logger.Debug("Staging task {0}: Cannot pack buildpack cache", this.TaskId);
                return;
            }
            File.Copy(this.workspace.StagedBuildpackCache, this.workspace.StagedBuildpackCachePath);
            Uri uri = new Uri(this.Message.BuildpackCacheUploadURI);
            Logger.Debug("Staging task {0}: Uploading buildpack cache {1} to {2}", this.TaskId, this.workspace.StagedBuildpackCachePath, this.Message.BuildpackCacheUploadURI);
            DEAUtilities.HttpUploadFile(this.Message.BuildpackCacheUploadURI, new FileInfo(this.workspace.StagedBuildpackCachePath), "upload[droplet]", "application/octet-stream", uri.UserInfo); 
        }

        private void PackBuildpackCache()
        {
            Directory.CreateDirectory(this.workspace.Cache);
            string tempFile = Path.ChangeExtension(this.workspace.StagedBuildpackCache, "tar");
            DEAUtilities.TarDirectory(this.workspace.Cache, tempFile);
            DEAUtilities.GzipFile(tempFile, this.workspace.StagedBuildpackCache);
            File.Delete(tempFile);
        }
        
    }
}
