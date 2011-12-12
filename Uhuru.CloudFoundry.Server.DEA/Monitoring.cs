// -----------------------------------------------------------------------
// <copyright file="Monitoring.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.IO;
    using System.Threading;
    using Uhuru.Utilities;
    using System.Globalization;
    
    public class Monitoring
    {

        /// <summary>
        /// Lock for resource tracking.
        /// </summary>
        public ReaderWriterLockSlim Lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Where to dump the applications disk usage.
        /// </summary>
        public string AppsDumpDirectory 
        {
            get; 
            set; 
        }
        
        /// <summary>
        /// When was the last dump made.
        /// </summary>
        public DateTime LastAppDump
        {
            get;
            set;
        }

        /// <summary>
        /// Total memory allocead to instances
        /// </summary>
        public long MemoryReservedMbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Total memory used by apps
        /// </summary>
        public long MemoryUsageKbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Number of instances that have resources allocated
        /// </summary>
        public long Clients
        {
            get;
            set;
        }

        /// <summary>
        /// Maximum memory that can be allocated
        /// </summary>
        public long MaxMemoryMbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Maximum number of instances that can run
        /// </summary>
        public long MaxClients
        {
            get;
            set;
        }


        //todo: stefi: configuration: consider putting this constants into configuration file
        public const int TaintPerAppMilliseconds = 10; //milliseconds of delay added to dea.disover response per instance of a droplet
        public const int TaintForMemoryMilliseconds = 100;  //milliseconds of delay added to dea.disover if the whole memory would be full
        public const int TaintMaxMilliseconds = 250; //maximum ms of taint

        public const int DefaultAppMemoryMbytes = 512;
        public const int DefaultAppDiskMbytes = 256;
        public const int DefaultAppFds = 1024;
        public const int DefaultMaxClients = 1024;

        // How long to wait in between logging the structure of the apps directory in the event that a du takes excessively long
        public const int AppsDumpIntervalMilliseconds = 30 * 60000;


        public const int HeartbeatIntervalMilliseconds = 10 * 1000;
        public const int VarzUpdateIntervalMilliseconds = 1 * 1000;
        public const int MonitorIntervalMilliseconds = 10 * 1000;
        public const int CrashesReaperIntervalMilliseconds = 10 * 1000;
        public const int CrashesReaperTimeoutMilliseconds = 60 * 60 * 1000;

        //todo: adapt this to windows system
        public const int BeginReniceCpuThreshold = 50;
        public const int MaxReniceValue = 20;


        /// <summary>
        /// Add the instance memeory to the total memory usage and flags the instance as tracked.
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
                Lock.EnterWriteLock();
                instance.Lock.EnterWriteLock();

                if (!instance.Properties.ResourcesTracked)
                {
                    instance.Properties.ResourcesTracked = true;
                    Clients++;
                    MemoryReservedMbytes += instance.Properties.MemoryQuotaBytes / 1024 / 1024;
                }
            }
            finally
            {
                instance.Lock.ExitWriteLock();
                Lock.ExitWriteLock();
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
                Lock.EnterWriteLock();
                instance.Lock.EnterWriteLock();

                if (instance.Properties.ResourcesTracked)
                {
                    instance.Properties.ResourcesTracked = false;
                    MemoryReservedMbytes -= instance.Properties.MemoryQuotaBytes / 1024 / 1024;
                    Clients--;
                }
            }
            finally
            {
                instance.Lock.ExitWriteLock();
                Lock.ExitWriteLock();
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
            string summary_file = Path.Combine(AppsDumpDirectory, String.Format(CultureInfo.InvariantCulture, Strings.AppsDuSummary, tsig));
            string details_file = Path.Combine(AppsDumpDirectory, String.Format(CultureInfo.InvariantCulture, Strings.AppsDuDetails, tsig));

            // todo: vladi: removed max depth level (6) from call

            DiskUsage.WriteDiskUsageToFile(summary_file, appsDirectory, true);
            DiskUsage.WriteDiskUsageToFile(details_file, appsDirectory, false);
        }
    }
}
