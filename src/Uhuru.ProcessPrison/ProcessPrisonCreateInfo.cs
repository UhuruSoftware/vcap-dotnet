using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.Isolation
{
    public class ProcessPrisonCreateInfo
    {
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Setting this flag will create a job object with the KillProcessesOnJobClose flag set.
        /// If set to true the Job Object will be terminated (including all its processes) when all handles to the Job are released.
        /// Disabling this flag could create orphan Job Objects that cannot be opend or attached to.
        /// </summary>
        public bool KillProcessesrOnPrisonClose
        {
            get;
            set;
        }

        public long TotalPrivateMemoryLimit
        {
            get;
            set;
        }

        public int RunningProcessesLimit
        {
            get;
            set;
        }

        public string WindowsPassword
        {
            get;
            set;
        }

        /// <summary>
        /// The space usage quota for VolumeRootPath.
        /// Use -1 to disable disk quota.
        /// </summary>
        public long DiskQuotaBytes
        {
            get;
            set;
        }


        /// <summary>
        /// The path in the disk volume to apply quota on.
        /// Ex. "C:\dir" for volume "C:\"
        /// </summary>
        public string DiskQuotaPath
        {
            get;
            set;
        }

        /// <summary>
        /// The limit for network data upload rate in bits per second.
        /// Policy not enforced for local traffic.
        /// Use -1 to disable network throtteling. NetworkOutboundRateLimitBitsPerSecond
        /// </summary>
        public long NetworkOutboundRateLimitBitsPerSecond
        {
            get;
            set;
        }

        public ProcessPrisonCreateInfo()
        {
            this.KillProcessesrOnPrisonClose = true;
            this.TotalPrivateMemoryLimit = 0;
            this.RunningProcessesLimit = 0;
            this.DiskQuotaBytes = -1;
            this.NetworkOutboundRateLimitBitsPerSecond = -1;
        }
    }
}
