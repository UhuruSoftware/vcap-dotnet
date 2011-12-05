using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Uhuru.CloudFoundry.DEA.WindowsService
{
    partial class DeaWindowsService : ServiceBase
    {
        Agent agent;

        public DeaWindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
        }

        protected override void OnStop()
        {
            agent.Shutdown();
        }

        internal void Start(string[] p)
        {
            agent = new Agent();
            agent.Run();
        }
    }
}
