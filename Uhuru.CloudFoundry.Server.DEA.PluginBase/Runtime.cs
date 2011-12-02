using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    public class Runtime : MarshalByRefObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
    }
}
