// -----------------------------------------------------------------------
// <copyright file="StagingInstance.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2013 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Uhuru.CloudFoundry.DEA.Messages;
using Uhuru.Isolation;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class StagingInstance : IDisposable
    {
        /// <summary>
        /// The lock for the staging instance.
        /// </summary>
        private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// The Windows Job Object for the application instance. Used for security/resource sandboxing.
        /// </summary>
        private ProcessPrison processPrison = new ProcessPrison();
        
        private StagingInstanceProperties properties = new StagingInstanceProperties();

        

        /// <summary>
        /// Gets or sets the instances lock.
        /// </summary>
        public ReaderWriterLockSlim Lock
        {
            get
            {
                return this.readerWriterLock;
            }

            set
            {
                this.readerWriterLock = value;
            }
        }

        public Process CompileProcess { get; set; }

        public StagingInstanceProperties Properties
        {
            get { return this.properties; }
            set { this.properties = value; }
        }

        public ProcessPrison Prison
        {
            get { return this.processPrison; }
            set { this.processPrison = value; }
        }

        public Buildpack Buildpack { get; set; }
        public StagingWorkspace Workspace { get; set; }
        public DeaStartMessageRequest StartMessage { get; set; }
        public Exception StagingException { get; set; }
        
        public delegate void StagingTaskEventHandler(StagingInstance instance);

        public event StagingTaskEventHandler AfterSetup;
        public event StagingTaskEventHandler AfterUpload;
        public event StagingTaskEventHandler AfterStop;

        public void SetupStagingEnvironment()
        {
            try
            {
                string instanceDir = this.Properties.Directory;

                // check before downloading
                if (this.Properties.Stopped)
                {
                    return;
                }

                WebClient client = new WebClient();
                try
                {
                    Logger.Debug("Staging task {0}: Downloading app bits from {1}", this.Properties.TaskId, this.Properties.DownloadURI);
                    string tempFile = Path.Combine(this.Workspace.WorkspaceDir, string.Format(CultureInfo.InvariantCulture, Strings.Pending, "droplet"));
                    Uri uri = new Uri(this.Properties.DownloadURI);
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(uri.UserInfo)));
                    client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                    client.DownloadFile(uri.ToString(), tempFile);
                    File.Move(tempFile, this.Workspace.DownloadDropletPath);
                }
                finally
                {
                    client.Dispose();
                }

                if (this.Properties.BuildpackCacheDownloadURI != null)
                {
                    client = new WebClient();
                    try
                    {
                        Logger.Debug("Staging task {0}: Downloading buildpack cache", this.Properties.TaskId);
                        string tempFile = Path.Combine(this.Workspace.WorkspaceDir, string.Format(CultureInfo.InvariantCulture, Strings.Pending, "droplet"));
                        Uri uri = new Uri(this.Properties.BuildpackCacheDownloadURI);
                        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(uri.UserInfo)));
                        client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                        client.DownloadFile(uri.ToString(), tempFile);
                        File.Move(tempFile, this.Workspace.DownloadBuildpackCachePath);
                    }
                    catch
                    {
                        Logger.Error("Staging task {0}: Failed downloading buildpack cache", this.Properties.TaskId);
                    }
                    finally
                    {
                        client.Dispose();
                    }
                }

                Directory.CreateDirectory(new FileInfo(this.Workspace.StagingLogPath).DirectoryName);
                if (!File.Exists(this.Workspace.StagingLogPath))
                {
                    Logger.Info("Preparing staging log file {0}", this.Workspace.StagingLogPath);
                    using (File.Create(this.Workspace.StagingLogPath)) ;
                }
            }
            catch (Exception ex)
            {
                this.StagingException = ex;
                throw ex;
            }
            finally
            {
                this.AfterSetup(this);
            }
        }

        public void UnpackDroplet()
        {
            Directory.CreateDirectory(this.Workspace.UnstagedDir);
            if (File.Exists(this.Workspace.DownloadDropletPath))
            {
                DEAUtilities.UnzipFile(this.Workspace.UnstagedDir, this.Workspace.DownloadDropletPath);
            }
            else
            {
                throw new Exception(string.Format("Could not find file {0}", this.Workspace.DownloadDropletPath));
            }
            Directory.CreateDirectory(this.Workspace.Cache);
            if (File.Exists(this.Workspace.DownloadBuildpackCachePath))
            {
                Logger.Debug("Staging task {0}: Unpacking buildpack cache {1}", this.Properties.TaskId, this.Workspace.DownloadBuildpackCachePath);
                DEAUtilities.UnzipFile(this.Workspace.Cache, this.Workspace.DownloadBuildpackCachePath); // Unzip
                string tarFileName = Directory.GetFiles(this.Workspace.DownloadBuildpackCachePath, "*.tar")[0];
                DEAUtilities.UnzipFile(this.Workspace.Cache, Path.Combine(this.Workspace.Cache, tarFileName)); // Untar
                File.Delete(Path.Combine(this.Workspace.Cache, tarFileName));
            }
        }

        public void PrepareStagingDirs()
        {
            string appDir = Path.Combine(this.Workspace.StagedDir, "app");
            string logsDir = Path.Combine(this.Workspace.StagedDir, "logs");
            string tmpDir = Path.Combine(this.Workspace.StagedDir, "tmp");

            Directory.CreateDirectory(appDir);
            Directory.CreateDirectory(logsDir);
            Directory.CreateDirectory(tmpDir);
            DEAUtilities.DirectoryCopy(this.Workspace.UnstagedDir, appDir, true);
        }

        public void CreatePrison()
        {
            if(this.Prison.Created)
            {
                this.Prison.Destroy();
            }

            this.Lock.EnterWriteLock();
            var prisonInfo = new ProcessPrisonCreateInfo();
            prisonInfo.TotalPrivateMemoryLimitBytes = this.Properties.MemoryQuotaBytes;
            if (this.Properties.UseDiskQuota)
            {
                prisonInfo.DiskQuotaBytes = this.Properties.DiskQuotaBytes;
                prisonInfo.DiskQuotaPath = this.Properties.Directory;
            }

            if (this.Properties.UploadThrottleBitsps > 0)
            {
                prisonInfo.NetworkOutboundRateLimitBitsPerSecond = this.Properties.UploadThrottleBitsps;
            }

            Logger.Info("Creating Process Prisson: {0}", prisonInfo.Id);
            this.Prison.Create(prisonInfo);
            this.Properties.WindowsPassword = this.Prison.WindowsPassword;
            this.Properties.WindowsUserName = this.Prison.WindowsUsername;
            this.Properties.InstanceId = this.Prison.Id;
            this.Lock.ExitWriteLock();

            // Explode the app into its directory and optionally bind its local runtime.
            Directory.CreateDirectory(this.Properties.Directory);

            DirectoryInfo deploymentDirInfo = new DirectoryInfo(this.Properties.Directory);
            DirectorySecurity deploymentDirSecurity = deploymentDirInfo.GetAccessControl();

            // Owner is important to account for disk quota 
            deploymentDirSecurity.SetOwner(new NTAccount(this.Properties.WindowsUserName));
            deploymentDirSecurity.SetAccessRule(
                new FileSystemAccessRule(
                    this.Properties.WindowsUserName,
                    FileSystemRights.Write | FileSystemRights.Read | FileSystemRights.Delete | FileSystemRights.Modify | FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None | PropagationFlags.InheritOnly,
                    AccessControlType.Allow));

            using (new ProcessPrivileges.PrivilegeEnabler(Process.GetCurrentProcess(), ProcessPrivileges.Privilege.Restore))
            {
                deploymentDirInfo.SetAccessControl(deploymentDirSecurity);
            }
        }

        public void GetBuildpack(StagingStartMessageRequest message, string gitPath, string buildpacksDir)
        {           
            try
            {
                this.CreatePrison();
                if (message.Properties.Buildpack != null)
                {
                    Logger.Info("Staging task {0}: Downloading buildpack from {1}", this.Properties.TaskId, message.Properties.Buildpack);
                    Directory.CreateDirectory(Path.Combine(this.Workspace.TempDir, "buildpacks"));
                    string buildpackPath = Path.Combine(this.Workspace.TempDir, "buildpacks", Path.GetFileName(new Uri(message.Properties.Buildpack).LocalPath));
                    string command = string.Format("\"{0}\" clone --quiet --recursive {1} {2}", gitPath, message.Properties.Buildpack, buildpackPath);
                    Logger.Debug(command);
                    int success = Command.ExecuteCommand(command, this.Workspace.TempDir);
                    if (success != 0)
                    {
                        throw new Exception(string.Format("Failed to git clone buildpack. Exit code: {0}", success));
                    }
                    this.Buildpack = new Buildpack(buildpackPath, Path.Combine(this.Workspace.StagedDir, "app"), this.Workspace.Cache, this.Workspace.StagingLogPath);
                    
                    bool detected = this.Buildpack.Detect(this.Prison);
                    if (!detected)
                    {
                        throw new Exception("Buildpack does not support this application.");
                    }
                }
                else
                {
                    Logger.Info("Staging task {0}: Detecting buildpack", this.Properties.TaskId);
                    foreach (string dir in Directory.EnumerateDirectories(buildpacksDir))
                    {
                        DEAUtilities.DirectoryCopy(dir, Path.Combine(this.Workspace.TempDir, "buildpack"), true);
                        Buildpack bp = new Buildpack(Path.Combine(this.Workspace.TempDir, "buildpack"), Path.Combine(this.Workspace.StagedDir, "app"), this.Workspace.Cache, this.Workspace.StagingLogPath);
                        bool success = bp.Detect(this.Prison);
                        if (success)
                        {
                            this.Buildpack = bp;
                            break;
                        }
                        else
                        {
                            Directory.Delete(Path.Combine(this.Workspace.TempDir, "buildpack"), true);
                        }
                    }

                    if (this.Buildpack == null)
                    {
                        throw new Exception("Unable to detect a supported application type");
                    }
                    Logger.Info("Staging task {0}: Detected buildpack {1}", this.Properties.TaskId, this.Buildpack.Name);
                }
                this.Properties.DetectedBuildpack = this.Buildpack.Name;
            }
            finally
            {
                if (this.Prison.Created)
                {
                    this.Prison.Destroy();
                }
            }
        }

        public string GetStartCommand()
        {
            ReleaseInfo info = new ReleaseInfo();
            if (this.Properties.MetaCommand != null)
            {
                return this.Properties.MetaCommand;
            }
            try
            {
                this.CreatePrison();
                info = this.Buildpack.GetReleaseInfo(this.Prison);
            }
            finally
            {
                if (this.Prison.Created)
                {
                    this.Prison.Destroy();
                }
            }
            if (info.defaultProcessType != null)
            {
                if (info.defaultProcessType.Web != null)
                {
                    return info.defaultProcessType.Web;
                }
            }
            throw new Exception("Please specify a web start command in your manifest.yml");
        }

        public void Cleanup()
        {
            DEAUtilities.RemoveReadOnlyAttribute(this.Workspace.BaseDir);
            Directory.Delete(this.Workspace.BaseDir, true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.readerWriterLock != null)
                {
                    this.readerWriterLock.Dispose();
                }
            }
        }
    }
}
