using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.DEA
{
    public class ProcessPrisonCreateInfo
    {
        public bool DiskQuota
        {
            get;
            set;
        }

        public long MaxMemoryBytes
        {
            get;
            set;
        }

        public int MaxRunningProcesses
        {
            get;
            set;
        }

        public ProcessPrisonCreateInfo()
        {
            this.DiskQuota = false;
            this.MaxMemoryBytes = -1;
            this.MaxRunningProcesses = -1;
        }
    }
}
