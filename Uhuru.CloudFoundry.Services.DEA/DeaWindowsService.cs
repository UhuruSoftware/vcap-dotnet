// -----------------------------------------------------------------------
// <copyright file="DeaWindowsService.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.WindowsService
{
    using System.ServiceProcess;
    
    partial class DeaWindowsService : ServiceBase
    {
        private Agent agent;

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
            this.agent.Shutdown();
        }

        internal void Start(string[] p)
        {
            this.agent = new Agent();
            this.agent.Run();
        }
    }
}
