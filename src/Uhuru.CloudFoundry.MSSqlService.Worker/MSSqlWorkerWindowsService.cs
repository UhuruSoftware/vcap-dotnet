using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Uhuru.CloudFoundry.MSSqlService;
using Uhuru.Configuration;
using System.Configuration;

namespace Uhuru.CloudFoundry.MSSqlWorker
{
    public partial class MSSqlWorkerWindowsService : System.ServiceProcess.ServiceBase
    {
        private Worker worker;

        public MSSqlWorkerWindowsService()
        {
            InitializeComponent();
        }

        internal void Start()
        {
            ServiceElement serviceConfig = ((UhuruSection)ConfigurationManager.GetSection("uhuru")).Service;

            this.worker = new Worker();
            this.worker.Start(serviceConfig);
        }

        protected override void OnStart(string[] args)
        {
            this.Start();
        }

        protected override void OnStop()
        {
            this.worker.Stop();
        }
    }
}
