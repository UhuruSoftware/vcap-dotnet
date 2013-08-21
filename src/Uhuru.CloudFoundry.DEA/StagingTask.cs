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
        public string DetectedBuildpack { get; set; }
        public string DropletSHA { get; set; }

        private StagingStartMessageRequest message;
        private StagingWorkspace workspace;
        private string buildpacksDir;

        public StagingTask(StagingStartMessageRequest message, string dropletDir, string buildpacksDirectory)
        {
            this.TaskId = message.TaskID;
            this.message = message;
            this.workspace = new StagingWorkspace(dropletDir);
            this.buildpacksDir = buildpacksDirectory;
        }

        public void Start() 
        {
            StagingSetup();
            Staging();
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
                File.Create(this.workspace.StagingLogPath);                
            }
            catch (Exception ex)
            {
                AfterSetup(ex);
            }
            finally
            {
                AfterSetup(null);
            }
        }

        private void Staging()
        {
            UnpackApp();
            UnpackBuildpackCache();
            Stage();
            PackApp();

            File.Copy(this.workspace.StagedDroplet, Path.Combine(this.workspace.StagedDropletDir, Path.GetFileName(this.workspace.StagedDroplet)));

            LogUploadStarted();
            StagingInfo();
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
            DEAUtilities.UnzipFile(this.workspace.UnstagedDir, this.workspace.DownloadDropletPath);
        }
        private void UnpackBuildpackCache() 
        {
            if (File.Exists(this.workspace.DownloadBuildpackCachePath))
            {
                string tarFileName = Path.GetFileName(this.workspace.DownloadBuildpackCachePath);
                tarFileName = Path.ChangeExtension(tarFileName, ".tar");
                DEAUtilities.UnzipFile(this.workspace.Cache, this.workspace.DownloadBuildpackCachePath); // Unzip
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
                string buildpackPath = Path.Combine(this.workspace.TempDir, "buildpacks", Path.GetFileName(new Uri(this.message.Properties.Buildpack).LocalPath));
                string command = string.Format("git clone --recursive {0} {1}", this.message.Properties.Buildpack, buildpackPath);
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

            

        }

        private void PackApp() 
        {
            string tempFile = Path.ChangeExtension(this.workspace.StagedDroplet, "tar");
            DEAUtilities.TarDirectory(this.workspace.StagedDir, tempFile);
            DEAUtilities.GzipFile(tempFile, this.workspace.StagedDroplet);
            File.Delete(tempFile);
        }

        private void CopyOut() 
        {
            
        }
        private void SaveDroplet() { }
        private void LogUploadStarted() { }
        private void StagingInfo() { }

        
    }
}
