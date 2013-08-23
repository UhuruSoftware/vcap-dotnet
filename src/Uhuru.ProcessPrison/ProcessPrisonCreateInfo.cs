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

        public bool TerminateContainerOnDispose
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

        public ProcessPrisonCreateInfo()
        {
            this.TerminateContainerOnDispose = false;
            this.TotalPrivateMemoryLimit = 0;
            this.RunningProcessesLimit = 0;
            this.DiskQuotaBytes = -1;
        }
    }
}
