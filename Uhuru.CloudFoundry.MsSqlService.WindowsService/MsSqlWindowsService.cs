using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Uhuru.CloudFoundry.Server.MSSqlNode;
using Uhuru.Configuration.Service;
using Uhuru.Configuration;
using Uhuru.CloudFoundry.ServiceBase;

namespace Uhuru.CloudFoundry.MsSqlService.WindowsService
{
    public partial class MsSqlWindowsService : System.ServiceProcess.ServiceBase
    {
        Node node;

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
            node.Shutdown();
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

            MSSqlOptions msSqlOptions = new MSSqlOptions();
            msSqlOptions.Host = serviceConfig.MSSql.Host;
            msSqlOptions.User = serviceConfig.MSSql.User;
            msSqlOptions.Port = serviceConfig.MSSql.Port;
            msSqlOptions.Password = serviceConfig.MSSql.Password;

            Node node = new Node();
            node.Start(options, msSqlOptions);
        }
    }
}
