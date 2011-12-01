using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    public delegate void ApplicationCrashDelegate(string instanceId, Exception exception);
    
    public class ApplicationInfo
    {
        public string InstanceId { get; set; }
        public string LocalIp { get; set; }
        public string Port { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string WindowsUsername { get; set; }
        public string WindowsPassword { get; set; }
    }
}
