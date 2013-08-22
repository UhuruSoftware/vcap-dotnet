using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.Isolation
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

        public bool CreateWindow
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
            this.CreateWindow = false;
            this.FileName = null;
            this.EnvironmentVariables = new Dictionary<string, string>();
            this.WorkingDirectory = null;
        }

        
    }
}
