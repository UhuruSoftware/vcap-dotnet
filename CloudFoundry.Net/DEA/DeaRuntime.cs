using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.DEA
{
    class DeaRuntime
    {
        public string Executable;
        public string Version;
        public string VersionFlag;
        public string AdditionalChecks;
        public Dictionary<string, List<string>> DebugEnv;
        public Dictionary<string, string> Environment;
        public bool Enabled;
    }
}
