using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    public interface IAgentPlugin
    {
        void ConfigureApplication(ApplicationInfo appInfo, Runtime runtime, ApplicationVariable[] variables, ApplicationService[] services);

        void ConfigureApplication(ApplicationInfo appInfo, Runtime runtime, ApplicationVariable[] variables, ApplicationService[] services, int[] processIds);

        event ApplicationCrashDelegate OnApplicationCrash;

        void ConfigureDebug(string debugPort, string debugIp, ApplicationVariable[] debugVariables);

        void StartApplication();

        int[] GetApplicationProcessIDs();

        void StopApplication();

        void KillApplication();
    }
}
