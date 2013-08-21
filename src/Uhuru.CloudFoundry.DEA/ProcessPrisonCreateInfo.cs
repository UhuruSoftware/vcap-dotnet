using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.DEA
{
    public class ProcessPrisonCreateInfo
    {
        public bool TerminateContainerOnDispose
        {
            get;
            set;
        }

        public long TotalMemoryLimit
        {
            get;
            set;
        }

        public int RunningProcessesLimit
        {
            get;
            set;
        }

        public string WindowsUsername
        {
            get;
            set;
        }

        public string WindowsUsernamePassword
        {
            get;
            set;
        }

        public ProcessPrisonCreateInfo()
        {
            this.TerminateContainerOnDispose = false;
            this.TotalMemoryLimit = 0;
            this.RunningProcessesLimit = 0;

        }
    }
}
