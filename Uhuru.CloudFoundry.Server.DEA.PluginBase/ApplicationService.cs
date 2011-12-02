using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    public class ApplicationService : MarshalByRefObject
    {
        public string Name { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }
        public string Port { get; set; }
        public string Plan { get; set; }
        public Dictionary<string, object> PlanOptions { get; set; }
        public string Host { get; set; }
        public string ServiceName { get; set; }
    }
}
