// -----------------------------------------------------------------------
// <copyright file="MsSqlWindowsService.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService.WindowsService
{
    using Uhuru.CloudFoundry.MSSqlService;
    using Uhuru.CloudFoundry.ServiceBase;
    using Uhuru.Configuration;
    using Uhuru.Configuration.Service;

    /// <summary>
    /// This is the Windows Service class that hosts an MS SQL Node.
    /// </summary>
    internal partial class MSSqlWindowsService : System.ServiceProcess.ServiceBase
    {
        /// <summary>
        /// The MS Sql Server Node.
        /// </summary>
        private Node node;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSSqlWindowsService"/> class.
        /// </summary>
        public MSSqlWindowsService()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Starts the MS SQL Node using the specified arguments.
        /// </summary>
        internal void Start()
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
            options.Plan = serviceConfig.Plan;
            options.Uri = new System.Uri(serviceConfig.MBus);
            options.ZInterval = serviceConfig.ZInterval;
            options.LocalRoute = serviceConfig.LocalRoute;
            options.StatusPort = serviceConfig.StatusPort;

            MSSqlOptions sqlServerOptions = new MSSqlOptions();
            sqlServerOptions.Host = serviceConfig.MSSql.Host;
            sqlServerOptions.User = serviceConfig.MSSql.User;
            sqlServerOptions.Port = serviceConfig.MSSql.Port;
            sqlServerOptions.Password = serviceConfig.MSSql.Password;
            sqlServerOptions.LogicalStorageUnits = serviceConfig.MSSql.LogicalStorageUnits;

            this.node = new Node();
            this.node.Start(options, sqlServerOptions);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            this.Start();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            this.node.Shutdown();
        }
    }
}
