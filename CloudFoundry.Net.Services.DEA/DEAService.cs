using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;

namespace CloudFoundry.Net.DEA
{
    public partial class DEAService : ServiceBase
    {


        Agent agent;

        public DEAService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Start(args);
        }

        protected override void OnStop()
        {
            agent.Shutdown();
        }

        internal void Start(string[] p)
        {
            agent = new Agent();

            Utils.SystemCleanup(agent.GetAppsDir, agent.GetAppStateFile);

            agent.Run();
        }
    }
}
