// -----------------------------------------------------------------------
// <copyright file="StagingTask.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2013 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using Uhuru.CloudFoundry.DEA.Messages;
    using Uhuru.Utilities;

    class StagingTask
    {
        public delegate void StagingTaskEventHandler(Exception error);

        public event StagingTaskEventHandler AfterSetup;
        public event StagingTaskEventHandler AfterUpload;
        public event StagingTaskEventHandler AfterStop;

        public string TaskId { get; set; }
        public string StreamingLogUrl { get; set; }
        public string TaskLog { get; set; }
        public string DetectedBuildpack { get { return StagingInfo.GetDetectedBuildpack(Path.Combine(this.workspace.StagedDir, StagingWorkspace.StagingInfo)); } }
        public string DropletSHA { get; set; }

        private StagingStartMessageRequest message;
        private StagingWorkspace workspace;
        private string buildpacksDir;

        public StagingTask(StagingStartMessageRequest message, string dropletDir, string buildpacksDirectory)
        {
            this.TaskId = message.TaskID;
            this.message = message;
            this.workspace = new StagingWorkspace(dropletDir, message.TaskID);
            this.buildpacksDir = buildpacksDirectory;
        }

        public void Start() 
        {
            try
            {
                StagingSetup();
                Staging();
                UploadDroplet();
                SaveBuildpackCache();
                AfterUpload(null);
            }
            catch(Exception ex)
            {
                AfterUpload(ex);
                throw ex;
            }
            finally
            {                
                Directory.Delete(this.workspace.WorkspaceDir, true);
            }
        }

        private void StagingSetup()
        {
            try
            {
                DownloadApp();
                if (message.BuildpackCacheDownloadURI != null)
                {
                    DownloadBuildpackCache();
                }
                Directory.CreateDirectory(new FileInfo(this.workspace.StagingLogPath).DirectoryName);
                if(!File.Exists(this.workspace.StagingLogPath))
                    File.Create(this.workspace.StagingLogPath);
                AfterSetup(null);
            }
            catch (Exception ex)
            {
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
                string tempFile = Path.Combine(this.workspace.WorkspaceDir, string.Format(CultureInfo.InvariantCulture, Strings.Pending, "droplet"));
                Uri uri = new Uri(this.message.DownloadURI);
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
                string tempFile = Path.Combine(this.workspace.WorkspaceDir, string.Format(CultureInfo.InvariantCulture, Strings.Pending, "droplet"));
                Uri uri = new Uri(this.message.BuildpackCacheDownloadURI);
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(uri.UserInfo)));
                client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                client.DownloadFile(uri.ToString(), tempFile);
                File.Move(tempFile, this.workspace.DownloadBuildpackCachePath);
            }
            catch
            {
                Logger.Error("Failed to download buildpack");
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
            if (this.message.Properties.Buildpack != null)
            {
                Directory.CreateDirectory(Path.Combine(this.workspace.TempDir, "buildpacks"));
                string buildpackPath = Path.Combine(this.workspace.TempDir, "buildpacks", Path.GetFileName(new Uri(this.message.Properties.Buildpack).LocalPath));
                string command = string.Format("\"E:\\Program Files (x86)\\Git\\bin\\git.exe\" clone --recursive {0} {1}", this.message.Properties.Buildpack, buildpackPath);
                int success = DEAUtilities.ExecuteCommand(command);
                if (success != 0)
                {
                    throw new Exception("Failed to git clone buildpack");
                }
                buildpack = new Buildpack(buildpackPath, appDir, this.workspace.Cache);
            }
            else
            {
                foreach (string dir in Directory.EnumerateDirectories(this.buildpacksDir))
                {
                    Buildpack bp = new Buildpack(dir, appDir, this.workspace.Cache);
                    bool success = bp.Detect();
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
            }
            buildpack.Compile(); // TODO need to add timeout

            StagingInfo.SaveBuildpackInfo(Path.Combine(this.workspace.StagedDir, StagingWorkspace.StagingInfo), buildpack.Name, GetStartCommand(buildpack));

        }

        private void PackApp() 
        {
            string tempFile = Path.ChangeExtension(this.workspace.StagedDroplet, "tar");
            DEAUtilities.TarDirectory(this.workspace.StagedDir, tempFile);
            DEAUtilities.GzipFile(tempFile, this.workspace.StagedDroplet);
            File.Delete(tempFile);
        }

        private string GetStartCommand(Buildpack buildpack)
        {
            if (this.message.Properties.Meta != null)
            {
                if (this.message.Properties.Meta.Command != null)
                {
                    return this.message.Properties.Meta.Command;
                }
            }
            ReleaseInfo info = buildpack.GetReleaseInfo();
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
            Uri uri = new Uri(this.message.UploadURI);
            DEAUtilities.HttpUploadFile(this.message.UploadURI, new FileInfo(this.workspace.StagedDropletPath), "upload[droplet]", "application/octet-stream", uri.UserInfo);
        }

        private void SaveBuildpackCache()
        {
            try
            {
                PackBuildpackCache();
            }
            catch
            {
                return;
            }
            File.Copy(this.workspace.StagedBuildpackCache, this.workspace.StagedBuildpackCachePath);
            Uri uri = new Uri(this.message.BuildpackCacheUploadURI);
            DEAUtilities.HttpUploadFile(this.message.BuildpackCacheUploadURI, new FileInfo(this.workspace.StagedBuildpackCachePath), "upload[droplet]", "application/octet-stream", uri.UserInfo); 
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
