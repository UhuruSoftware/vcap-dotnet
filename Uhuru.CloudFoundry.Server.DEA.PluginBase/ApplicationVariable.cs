using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    /// <summary>
    /// application variable basic data
    /// </summary>
    public class ApplicationVariable : MarshalByRefObject
    {
        /// <summary>
        /// the name of the variable
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// the value of the variable
        /// </summary>
        public string Value { get; set; }
    }

}
