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
            Start();
        }

        protected override void OnStop()
        {
            this.agent.Shutdown();
        }

        internal void Start()
        {
            this.agent = new Agent();
            this.agent.Run();
        }
    }
}
