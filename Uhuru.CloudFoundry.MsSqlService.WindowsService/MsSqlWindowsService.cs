// -----------------------------------------------------------------------
// <copyright file="MsSqlWindowsService.cs" company="">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MsSqlService.WindowsService
{
    using Uhuru.CloudFoundry.Server.MSSqlNode;
    using Uhuru.CloudFoundry.ServiceBase;
    using Uhuru.Configuration;
    using Uhuru.Configuration.Service;
    
    public partial class MsSqlWindowsService : System.ServiceProcess.ServiceBase
    {
        private Node node;

        public MsSqlWindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Start(args);
        }

        protected override void OnStop()
        {
            this.node.Shutdown();
        }

        internal void Start(string[] args)
        {
            ServiceElement serviceConfig = UhuruSection.GetSection().Service;

            Options options = new Options();
            options.AvailableStorage = serviceConfig.AvailableStorage;
            options.BaseDir = serviceConfig.BaseDir;
            options.Index = serviceConfig.Index;
            options.LocalDB = serviceConfig.LocalDB;
            options.MaxDBSize = serviceConfig.MaxDBSize;
            options.MaxLengthyQuery = serviceConfig.MaxLengthyQuery;
            options.MaxLengthyTX = serviceConfig.MaxLengthyTX;
            options.MigrationNFS = serviceConfig.MigrationNFS;
            options.NodeId = serviceConfig.NodeId;
            options.Uri = serviceConfig.MBus;
            options.ZInterval = serviceConfig.ZInterval;
            options.LocalRoute = serviceConfig.LocalRoute;

            MSSqlOptions msSqlOptions = new MSSqlOptions();
            msSqlOptions.Host = serviceConfig.MSSql.Host;
            msSqlOptions.User = serviceConfig.MSSql.User;
            msSqlOptions.Port = serviceConfig.MSSql.Port;
            msSqlOptions.Password = serviceConfig.MSSql.Password;

            this.node = new Node();
            this.node.Start(options, msSqlOptions);
        }
    }
}
