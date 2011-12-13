// -----------------------------------------------------------------------
// <copyright file="Monitoring.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using Uhuru.Utilities;

    /// <summary>
    /// Contains all kind of monitoring related members.
    /// </summary>
    public class Monitoring : IDisposable
    {
        /// <summary>
        /// milliseconds of delay added to dea.disover response per instance of a droplet
        /// todo: configuration: consider putting this constants into configuration file
        /// </summary>
        public const int TaintPerAppMilliseconds = 10;

        /// <summary>
        /// milliseconds of delay added to dea.disover if the whole memory would be full
        /// </summary>
        public const int TaintForMemoryMilliseconds = 100;

        /// <summary>
        /// maximum ms of taint
        /// </summary>
        public const int TaintMaxMilliseconds = 250;

        /// <summary>
        /// The default application RAM, if it is not specified in the start message.
        /// </summary>
        public const int DefaultAppMemoryMbytes = 512;

        /// <summary>
        /// The default disk memory. Used when not specified in the start message.
        /// </summary>
        public const int DefaultAppDiskMbytes = 256;

        /// <summary>
        /// Default number of file descriptors.
        /// </summary>
        public const int DefaultAppFDS = 1024;

        /// <summary>
        /// Default number of maximum number o instances a DEA could host.
        /// </summary>
        public const int DefaultMaxClients = 1024;

        /// <summary>
        /// How long to wait in between logging the structure of the apps directory in the event that a du takes excessively long
        /// </summary>
        public const int AppsDumpIntervalMilliseconds = 30 * 60000;

        /// <summary>
        /// The interval the DEA is sending heartbeat messages.
        /// </summary>
        public const int HeartbeatIntervalMilliseconds = 10 * 1000;

        /// <summary>
        /// The interval at which the DEA is updating the varz values.
        /// </summary>
        public const int VarzUpdateIntervalMilliseconds = 1 * 1000;

        /// <summary>
        /// The interval at which the DEA is monitoring the applications.
        /// </summary>
        public const int MonitorIntervalMilliseconds = 10 * 1000;

        /// <summary>
        /// The interval at which the DEA is invoking the reaper.
        /// </summary>
        public const int CrashesReaperIntervalMilliseconds = 10 * 1000;

        /// <summary>
        /// The timeout after which the reaper is cleaning the crashed instance directories
        /// </summary>
        public const int CrashesReaperTimeoutMilliseconds = 60 * 60 * 1000;

        /// <summary>
        /// The value at which the monitor starts to lower the instance's process priority.
        /// todo: adapt this to windows system
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renice", Justification = "Word is in dictionary, but warning is still generated.")]
        public const int BeginReniceCpuThreshold = 50;

        /// <summary>
        /// The maximum priority value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renice", Justification = "Word is in dictionary, but warning is still generated.")]
        public const int MaxReniceValue = 20;

        /// <summary>
        /// Lock for resource tracking.
        /// </summary>
        private ReaderWriterLockSlim slimLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Gets or sets a lock for resource tracking.
        /// </summary>
        public ReaderWriterLockSlim Lock
        {
            get { return this.slimLock; }
            set { this.slimLock = value; }
        }

        /// <summary>
        /// Gets or sets where to dump the applications disk usage.
        /// </summary>
        public string AppsDumpDirectory 
        {
            get; 
            set; 
        }
        
        /// <summary>
        /// Gets or sets when was the last dump made.
        /// </summary>
        public DateTime LastAppDump
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total memory allocated to instances
        /// </summary>
        public long MemoryReservedMbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total memory used by apps
        /// </summary>
        public long MemoryUsageKbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of instances that have resources allocated
        /// </summary>
        public long Clients
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum memory that can be allocated
        /// </summary>
        public long MaxMemoryMbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum number of instances that can run
        /// </summary>
        public long MaxClients
        {
            get;
            set;
        }

        /// <summary>
        /// Add the instance memory to the total memory usage and flags the instance as tracked.
        /// </summary>
        /// <param name="instance">The instance to be tracked.</param>
        public void AddInstanceResources(DropletInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            try
            {
                this.Lock.EnterWriteLock();
                instance.Lock.EnterWriteLock();

                if (!instance.Properties.ResourcesTracked)
                {
                    instance.Properties.ResourcesTracked = true;
                    this.Clients++;
                    this.MemoryReservedMbytes += instance.Properties.MemoryQuotaBytes / 1024 / 1024;
                }
            }
            finally
            {
                instance.Lock.ExitWriteLock();
                this.Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Untracks the memory used by the instance and flags it/
        /// </summary>
        /// <param name="instance">The instance to be untracked.</param>
        public void RemoveInstanceResources(DropletInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            try
            {
                this.Lock.EnterWriteLock();
                instance.Lock.EnterWriteLock();

                if (instance.Properties.ResourcesTracked)
                {
                    instance.Properties.ResourcesTracked = false;
                    this.MemoryReservedMbytes -= instance.Properties.MemoryQuotaBytes / 1024 / 1024;
                    this.Clients--;
                }
            }
            finally
            {
                instance.Lock.ExitWriteLock();
                this.Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Logs out the directory structure of the apps dir. This produces both a summary
        /// (top level view) of the directory, as well as a detailed view.
        /// </summary>
        /// <param name="appsDirectory">The directory to be analyzed.</param>
        public void DumpAppsDirDiskUsage(string appsDirectory)
        {
            string tsig = DateTime.Now.ToString("yyyyMMdd_hhmm", CultureInfo.InvariantCulture);
            string summary_file = Path.Combine(this.AppsDumpDirectory, string.Format(CultureInfo.InvariantCulture, Strings.AppsDuSummary, tsig));
            string details_file = Path.Combine(this.AppsDumpDirectory, string.Format(CultureInfo.InvariantCulture, Strings.AppsDuDetails, tsig));

            // todo: vladi: removed max depth level (6) from call
            DiskUsage.WriteDiskUsageToFile(summary_file, appsDirectory, true);
            DiskUsage.WriteDiskUsageToFile(details_file, appsDirectory, false);
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
                if (this.slimLock != null)
                {
                    this.slimLock.Dispose();
                }
            }
        }
    }
}
