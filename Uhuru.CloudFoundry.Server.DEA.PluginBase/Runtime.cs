using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    /// <summary>
    /// holds the data related to a particular runtime
    /// </summary>
    public class Runtime : MarshalByRefObject
    {
        /// <summary>
        /// the name of the runtime
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// a short description of the runtime
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// the current version of the runtime
        /// </summary>
        public string Version { get; set; }
    }
}
