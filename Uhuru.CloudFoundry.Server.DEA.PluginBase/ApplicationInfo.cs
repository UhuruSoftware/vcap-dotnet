using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    /// <summary>
    /// a delegate used to define the OnApplicationCrash event
    /// </summary>
    /// <param name="instanceId">the id of the application that has crashed</param>
    /// <param name="exception">the exception thrown upon crashing</param>
    public delegate void ApplicationCrashDelegate(string instanceId, Exception exception);
    
    /// <summary>
    /// a class holding the basic information of an application
    /// </summary>
    public class ApplicationInfo : MarshalByRefObject
    {
        /// <summary>
        /// the id of the current application instance
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// the ip where the app is to be found
        /// </summary>
        public string LocalIp { get; set; }

        /// <summary>
        /// the port where the app is to be found
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// the name of the application
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// the physical path of the app
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// the username to authenticate
        /// </summary>
        public string WindowsUsername { get; set; }

        /// <summary>
        /// the password of the user to authenticate
        /// </summary>
        public string WindowsPassword { get; set; }
    }
}
