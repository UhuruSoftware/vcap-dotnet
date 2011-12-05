using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.DEA
{

    class DeaRuntime
    {
        public string Executable;
        public string Version;
        public string VersionFlag;
        public string AdditionalChecks;
        public Dictionary<string, List<string>> DebugEnv = new Dictionary<string,List<string>>();
        public Dictionary<string, string> Environment = new Dictionary<string,string>();
        public bool Enabled;
    }

}
