// -----------------------------------------------------------------------
// <copyright file="FileServiceBackup.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Uhuru.CloudFoundry.ServiceBase;
    using Uhuru.Utilities;

    /// <summary>
    /// Handles FileService node backups
    /// </summary>
    public class FileServiceBackup : BackupBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FileServiceBackup"/> is tolerant.
        /// </summary>
        /// <value>
        ///   <c>true</c> if tolerant; otherwise, <c>false</c>.
        /// </value>
        public bool IsTolerant
        {
            get
            {
                return this.tolerant;
            }

            set
            {
                this.tolerant = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FileServiceBackup"/> is shutting down.
        /// </summary>
        /// <value>
        ///   <c>true</c> if shutdown; otherwise, <c>false</c>.
        /// </value>
        public bool ShutdownJob
        {
            get
            {
                return this.shutdown;
            }

            set
            {
                this.shutdown = value;
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Subclasses have to implement this to backup the service
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Code more readable")]
        protected override void BackupServices()
        {
            Collection<string> services = this.ReadAllServices();
            DateTime currentTime = DateTime.UtcNow;

            Logger.Info("Begin backup at {0}", currentTime.ToLocalTime().ToString(CultureInfo.InvariantCulture));
            foreach (string service in services)
            {
                this.ExecuteBackupDb(service, currentTime);
                
                if (this.shutdown)
                {
                    Environment.Exit(0);
                }
            }

            Logger.Info("Backup started at {0} completed.", currentTime.ToString(CultureInfo.InvariantCulture));
            Environment.Exit(0);
        }

        /// <summary>
        /// Executes the backup.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="currentTime">The current time.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Code more readable"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged")]
        private void ExecuteBackupDb(string databaseName, DateTime currentTime)
        {
            string fullPath;

            try
            {
                fullPath = this.GetDumpPath(databaseName, 0, currentTime);

                string backupFile = Path.Combine(fullPath, databaseName);
                backupFile = Path.ChangeExtension(backupFile, ".zip");

                string instanceDirectory = this.GetInstanceDirectory(databaseName);

                if (!Directory.EnumerateFileSystemEntries(instanceDirectory).Any())
                {
                    //// Directory empty, exit
                    return;
                }

                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                DateTime t1 = DateTime.Now;

                ZipUtilities.ZipFile(instanceDirectory, backupFile);
                
                DateTime t2 = DateTime.Now;
                Logger.Info("Backup for db {0} completed in {1} seconds", databaseName, (t2 - t1).Seconds);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when backup db {0}: [{1}]", databaseName, ex.ToString());
            }
        }

        /// <summary>
        /// Reads all services.
        /// </summary>
        /// <returns>Collection containing all service names</returns>
        private Collection<string> ReadAllServices()
        {
            Collection<string> services = new Collection<string>();
            List<string> directories = Directory.GetDirectories(uhuruSection.Service.BaseDir, "*", SearchOption.TopDirectoryOnly).ToList();
            foreach (string dir in directories)
            {
                DirectoryInfo info = new DirectoryInfo(dir);
                services.Add(info.Name);
            }

            return services;
        }

        /// <summary>
        /// Gets the instance dir.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        /// <returns>Instance directory</returns>
        private string GetInstanceDirectory(string instanceName)
        {
            if (this.uhuruSection.Service.Uhurufs.UseVHD)
            {
                return Path.Combine(this.uhuruSection.Service.BaseDir, instanceName, instanceName);
            }
            else
            {
                return Path.Combine(this.uhuruSection.Service.BaseDir, instanceName);
            }
        }
    }
}
