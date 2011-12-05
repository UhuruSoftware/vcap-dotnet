using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Uhuru.Utilities;
using System.IO;

namespace Uhuru.CloudFoundry.DEA
{
    public class Monitoring
    {
        public ReaderWriterLockSlim Lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public string AppsDumpDirectory {get; set; }
        public DateTime LastAppDump;

        public long MemoryReservedMbytes = 0;   //total memory allocead to instances
        public long MemoryUsageKbytes = 0;      //total memory used by apps
        public long Clients = 0;    //number of instances that have resources allocated

        public long MaxMemoryMbytes = 0;
        public long MaxClients  = 0;


        //todo: stefi: configuration: consider putting this constants into configuration file
        public const int TaintPerAppMs = 10; //milliseconds of delay added to dea.disover response per instance of a droplet
        public const int TaintForMemoryMs = 100;  //milliseconds of delay added to dea.disover if the whole memory would be full
        public const int TaintMaxMs = 250; //maximum ms of taint

        public const int DefaultAppMemMbytes = 512;
        public const int DefaultAppDiskMbytes = 256;
        public const int DefaultAppFds = 1024;
        public const int DefaultMaxClients = 1024;

        // How long to wait in between logging the structure of the apps directory in the event that a du takes excessively long
        public const int AppsDumpIntervalMs = 30 * 60000;


        public const int HeartbeatIntervalMs = 10 * 1000;
        public const int VarzUpdateIntervalMs = 1 * 1000;
        public const int MonitorIntervalMs = 2 * 1000;
        public const int CrashesReaperIntervalMs = 30 * 1000;
        public const int CrashesReaperTimeoutMs = 60 * 60 * 1000;

        //todo: adapt this to windows system
        public const int BeginReniceCpuThreshold = 50;
        public const int MaxReniceValue = 20;


        public void AddInstanceResources(DropletInstance instance)
        {
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

        public void RemoveInstanceResources(DropletInstance instance)
        {
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

        // Logs out the directory structure of the apps dir. This produces both a summary
        // (top level view) of the directory, as well as a detailed view.
        public void DumpAppsDirDiskUsage(string AppsDirectory)
        {
            string tsig = DateTime.Now.ToString("yyyyMMdd_hhmm");
            string summary_file = Path.Combine(AppsDumpDirectory, String.Format("apps.du.{0}.summary", tsig));
            string details_file = Path.Combine(AppsDumpDirectory, String.Format("apps.du.{0}.details", tsig));

            // todo: vladi: removed max depth level (6) from call, because netdu does not support it

            DiskUsage.WriteDiskUsageToFile(summary_file, true, AppsDirectory, "*", true);
            DiskUsage.WriteDiskUsageToFile(details_file, true, AppsDirectory, "*", false);
        }
    }


}
