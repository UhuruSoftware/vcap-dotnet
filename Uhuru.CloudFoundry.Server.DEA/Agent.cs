using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.CloudFoundry.Server.DEA.Configuration;
using System.IO;
using CFNet = CloudFoundry.Net;

namespace Uhuru.CloudFoundry.Server.DEA
{
    public class Agent
    {
        private const decimal Version = 0.99m;

        private const int DefaultAppMemMb = 512;
        private const int DefaultAppDiskMb = 256;
        private const int DefaultAppFds = 1024;
        private const int DefaultMaxClients = 1024;

        private DropletCollection Droplets = new DropletCollection();
        private Stager AgentStager = new Stager();

        private FileViewer AgentFileViewer = new FileViewer();
        private VcapComponent AgentVcapComponenet = new VcapComponent();
        private Monitoring AgentMonitoring = new Monitoring();
        private CFNet.Nats.Client NatsClient = new CFNet.Nats.Client();

        private bool DisableDirCleanup;
        private bool EnforceUlimit;
        private bool MultiTenant;

        private int MaxMemoryMb;
        private int MaxClients;

        
        public Agent()
        {
            foreach (Configuration.DEA.RuntimeElement deaConf in UhuruSection.GetSection().DEA.Runtimes)
            {
                DeaRuntime dea = new DeaRuntime();


                dea.Executable = deaConf.Executable;
                dea.Version = deaConf.Version;
                dea.VersionFlag = deaConf.VersionFlag;
                dea.AdditionalChecks = deaConf.AdditionalChecks;
                dea.Enabled = true;

                

                foreach (Configuration.DEA.EnvironmentElement ienv in deaConf.Environment)
                {
                    dea.Environment.Add(ienv.Name, ienv.Value);
                }
                
                foreach (Configuration.DEA.DebugElement debugEnv in deaConf.Debug)
                {
                    dea.DebugEnv.Add(debugEnv.Name, new List<string>());
                    foreach (Configuration.DEA.EnvironmentElement ienv in debugEnv.Environment)
                    {
                        dea.DebugEnv[debugEnv.Name].Add(ienv.Name + "=" + ienv.Value);
                    }   
                    

                }

                
                AgentStager.Runtimes.Add(deaConf.Name, dea);
            }

            AgentStager.DropletDir = UhuruSection.GetSection().DEA.BaseDir;

            EnforceUlimit = UhuruSection.GetSection().DEA.EnforceUlimit;
            DisableDirCleanup = UhuruSection.GetSection().DEA.DisableDirCleanup;
            MultiTenant = UhuruSection.GetSection().DEA.MultiTenant;

            string local_route = UhuruSection.GetSection().DEA.LocalRoute;
            MaxMemoryMb = UhuruSection.GetSection().DEA.MaxMemory;

            AgentFileViewer.Port = UhuruSection.GetSection().DEA.FilerPort;
            
            AgentStager.ForeHttpFileSharing = UhuruSection.GetSection().DEA.ForceHttpSharing;


            //apps_dump_dir = ConfigurationManager.AppSettings["logFile"] ?? Path.GetTempPath();
            AgentMonitoring.AppsDumpDirectory = Path.GetTempPath();

            NatsClient.URI = new Uri( UhuruSection.GetSection().DEA.MessageBus );
            
            //heartbeat_interval = UhuruSection.GetSection().DEA.HeartBeatInterval;

            MaxClients = MultiTenant ? DefaultMaxClients : 1;

            AgentStager.StagedDir = Path.Combine(AgentStager.DropletDir, "staged");
            AgentStager.AppsDir = Path.Combine(AgentStager.DropletDir, "apps");
            AgentStager.DbDir = Path.Combine(AgentStager.DropletDir, "db");

            Droplets.AppStateFile = Path.Combine(AgentStager.DropletDir, "applications.json");

        }

        public void Run()
        {
            throw new System.NotImplementedException();
        }

        public void EvacuateAppsThenQuit()
        {
            throw new System.NotImplementedException();
        }

        public void Shutdown()
        {
            throw new System.NotImplementedException();
        }

        private void SendHeartbeat(string HeartbeatMessage)
        {
            throw new System.NotImplementedException();
        }
    }
}
