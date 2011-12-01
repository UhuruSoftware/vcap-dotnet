using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    public class ApplicationVariable : MarshalByRefObject
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

}
