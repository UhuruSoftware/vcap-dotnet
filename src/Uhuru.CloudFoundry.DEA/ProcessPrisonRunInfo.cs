using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.DEA
{
    public class ProcessPrisonRunInfo
    {

        public string FileName
        {
            get;
            set;
        }

        public string Arguments
        {
            get;
            set;
        }

        public string WorkingDirectory
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
            this.FileName = null;
            this.EnvironmentVariables = new Dictionary<string, string>();
            this.WorkingDirectory = null;
        }
    }
}
