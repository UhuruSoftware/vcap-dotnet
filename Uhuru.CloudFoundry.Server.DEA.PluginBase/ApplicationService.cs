using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    /// <summary>
    /// holds the data related to an application service
    /// </summary>
    public class ApplicationService : MarshalByRefObject
    {
        /// <summary>
        /// the name of this instance of the service
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// the user to authenticate
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// the password of the user to authenticate
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// the type of the service (such as RDBMS)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// the port where the service will be available
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// the usage plan of the service
        /// </summary>
        public string Plan { get; set; }

        /// <summary>
        /// details regarding the usage plan of the service
        /// </summary>
        public Dictionary<string, object> PlanOptions { get; set; }

        /// <summary>
        /// the host where the service will be made available
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// the general name of the service (mssql, mysql, etc)
        /// </summary>
        public string ServiceName { get; set; }
    }
}
