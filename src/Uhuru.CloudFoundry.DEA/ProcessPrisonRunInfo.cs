using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.DEA
{
    public class ProcessPrisonRunInfo
    {

        public string ExecutablePath
        {
            get;
            set;
        }

        public Dictionary<string, string> EnvironmentVariables
        {
            get;
            set;
        }

        public ProcessPrisonRunInfo()
        {
        }
    }
}
