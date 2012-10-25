// -----------------------------------------------------------------------
// <copyright file="BackupBase.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Management;
    using System.Threading;
    using Uhuru.Configuration;
    using Uhuru.Utilities;

    /// <summary>
    /// Base class for Windows Services Backup workers
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Code more readable and easier to maintain.")]
    public abstract class BackupBase
    {
        /// <summary>
        /// Boolean value indicating if process is shuting down
        /// </summary>
        protected bool shutdown;
        
        /// <summary>
        /// If true, backup will be done even if backupDir is not mount point
        /// </summary>
        protected bool tolerant;
        
        /// <summary>
        /// uhuru configuration section
        /// </summary>
        protected UhuruSection uhuruSection;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupBase"/> class.
        /// </summary>
        protected BackupBase()
        {
            this.uhuruSection = (UhuruSection)ConfigurationManager.GetSection("uhuru");
            this.shutdown = false;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Code is more readable"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged")]
        public virtual void Start()
        {
            this.CheckMountPoints();
            string nfsBase = Path.Combine(this.uhuruSection.Service.Backup.BackupBaseDir, "backups", this.uhuruSection.Service.Backup.ServiceName);
            Logger.Info("Check NFS base");
            if (Directory.Exists(nfsBase))
            {
                Logger.Info("{0} exists.", nfsBase);
            }
            else
            {
                Logger.Info("{0} does not exist, create it.", nfsBase);
                try
                {
                    Directory.CreateDirectory(nfsBase);
                }
                catch (Exception ex)
                {
                    Logger.Error("Could not create directory on NFS : [{0}]", ex.ToString());
                    Environment.Exit(1);
                }
            }

            this.BackupServices();
            Logger.Info("Task completed");
        }

        /// <summary>
        /// Determines whether [is mount point] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if [is mount point] [the specified path]; otherwise, <c>false</c>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Code is more readable")]
        protected static bool IsMountPoint(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            char[] trimChars = { '\\' };
            int retryCount = 5;

            while (retryCount > 0)
            {
                using (ManagementObjectSearcher search = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Volume "))
                {
                    foreach (ManagementObject queryObj in search.Get())
                    {
                        if (queryObj["Caption"].ToString().TrimEnd(trimChars).Equals(path.TrimEnd(trimChars), StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Debug("{0} is mount point.", path);
                            return true;
                        }
                    }
                }

                retryCount--;
                Thread.Sleep(1000);
            }

            Logger.Debug("{0} is not mount point.", path);
            return false;
        }

        /// <summary>
        /// Gets the epoch time.
        /// </summary>
        /// <param name="time">The current UTC time</param>
        /// <returns>
        /// seconds since last epoch
        /// </returns>
        protected static long EpochTime(DateTime time)
        {
            DateTime currentTime = DateTime.UtcNow;
            if (time != null)
            {
                currentTime = time;
            }

            DateTime epochStartTime = Convert.ToDateTime("1/1/1970 0:00:00 AM", CultureInfo.InvariantCulture);
            TimeSpan ts = currentTime.Subtract(epochStartTime);
            long epochtime;
            epochtime = (((((ts.Days * 24) + ts.Hours) * 60) + ts.Minutes) * 60) + ts.Seconds;
            return epochtime;
        }

        /// <summary>
        /// Gets the dump path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="time">The time.</param>
        /// <returns>
        /// service dump path
        /// </returns>
        protected string GetDumpPath(string name, int mode, DateTime time)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (mode == 1)
            {
                return Path.Combine(this.uhuruSection.Service.Backup.BackupBaseDir, "backups", this.uhuruSection.Service.Backup.ServiceName, name, EpochTime(time).ToString(CultureInfo.InvariantCulture), this.uhuruSection.Service.NodeId);
            }
            else
            {
                return Path.Combine(this.uhuruSection.Service.Backup.BackupBaseDir, "backups", this.uhuruSection.Service.Backup.ServiceName, name.Substring(0, 2), name.Substring(2, 2), name.Substring(4, 2), name, EpochTime(time).ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Checks the mount points.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Code is more readable")]
        protected void CheckMountPoints()
        {
            string path = this.uhuruSection.Service.Backup.BackupBaseDir;
            if (!this.tolerant && !IsMountPoint(path))
            {
                Logger.Error("{0} is not mounted, exit.", path);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Subclasses have to implement this to backup the service
        /// </summary>
        protected abstract void BackupServices();
    }
}
