using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.IO;
using System.Xml;
using System.Configuration;
using Uhuru.Configuration;
using System.Net;
using System.Diagnostics;
using System.Reflection;


namespace Uhuru.CloudFoundry.MsSqlService.WindowsService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            InstallArguments arguments = new InstallArguments(this.Context);
            string configFile = Path.Combine(arguments.TargetDir, Assembly.GetExecutingAssembly().Location + ".config");


            System.Configuration.ConfigurationFileMap fileMap = new ConfigurationFileMap(configFile);

            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenMappedMachineConfiguration(fileMap);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(delegate(object sender, ResolveEventArgs args)
                {
                    return Assembly.LoadFile(Path.Combine(arguments.TargetDir, args.Name + ".dll"));
                });

            UhuruSection section = (UhuruSection)config.GetSection("uhuru");

            if (!String.IsNullOrEmpty(arguments.AvailableStorage))
            {
                section.Service.AvailableStorage = int.Parse(arguments.AvailableStorage);
            }

            if (!String.IsNullOrEmpty(arguments.BaseDir))
            {
                section.Service.BaseDir = arguments.BaseDir;
            }

            if (!String.IsNullOrEmpty(arguments.Index))
            {
                section.Service.Index = int.Parse(arguments.Index);
            }

            if (!String.IsNullOrEmpty(arguments.LocalDb))
            {
                section.Service.LocalDB = arguments.LocalDb;
            }

            string ip = "";
            foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ip = address.ToString();
                }
            }
            if (!String.IsNullOrEmpty(arguments.LocalRoute))
            {
                section.Service.LocalRoute = arguments.LocalRoute;
            }
            else
            {
                section.Service.LocalRoute = ip;
            }

            if (!String.IsNullOrEmpty(arguments.MaxDbSize))
            {
                section.Service.MaxDBSize = int.Parse(arguments.MaxDbSize);
            }

            if (!String.IsNullOrEmpty(arguments.MaxLongQuery))
            {
                section.Service.MaxLengthyQuery = int.Parse(arguments.MaxLongQuery);
            }

            if (!String.IsNullOrEmpty(arguments.MaxLongTx))
            {
                section.Service.MaxLengthyTX = int.Parse(arguments.MaxLongTx);
            }

            if (!String.IsNullOrEmpty(arguments.Mbus))
            {
                section.Service.MBus = arguments.Mbus;
            }

            if (!String.IsNullOrEmpty(arguments.MigrationNfs))
            {
                section.Service.MigrationNFS = arguments.MigrationNfs;
            }

            if (!String.IsNullOrEmpty(arguments.NodeId))
            {
                section.Service.NodeId = arguments.NodeId;
            }

            if (!String.IsNullOrEmpty(arguments.ZInterval))
            {
                section.Service.ZInterval = int.Parse(arguments.ZInterval);
            }

            if (!String.IsNullOrEmpty(arguments.Host))
            {
                section.Service.MSSql.Host = arguments.Host;
            }

            if (!String.IsNullOrEmpty(arguments.Password))
            {
                section.Service.MSSql.Password = arguments.Password;
            }

            if (!String.IsNullOrEmpty(arguments.Port))
            {
                section.Service.MSSql.Port = int.Parse(arguments.Port);
            }

            if (!String.IsNullOrEmpty(arguments.User))
            {
                section.Service.MSSql.User = arguments.User;
            }
            config.Save();

        }

        private class InstallArguments
        {
            private class Argument
            {
                public const string targetDir = "TARGETDIR";
                public const string nodeId = "nodeId";
                public const string migrationNfs = "migrationNfs";
                public const string mbus = "mbus";
                public const string index = "index";
                public const string zInterval = "zInterval";
                public const string maxDbSize = "maxDbSize";
                public const string maxLongQuery = "maxLongQuery";
                public const string maxLongTx = "maxLongTx";
                public const string localDb = "localDb";
                public const string baseDir = "baseDir";
                public const string localRoute = "localRoute";
                public const string availableStorage = "availableStorage";
                public const string host = "host";
                public const string user = "user";
                public const string password = "password";
                public const string port = "port";
            }

            #region Propoerties

            private string targetDir;

            public string TargetDir
            {
                get { return targetDir; }
                set { targetDir = value; }
            }
            private string nodeId;

            public string NodeId
            {
                get { return nodeId; }
                set { nodeId = value; }
            }
            private string migrationNfs;

            public string MigrationNfs
            {
                get { return migrationNfs; }
                set { migrationNfs = value; }
            }
            private string mbus;

            public string Mbus
            {
                get { return mbus; }
                set { mbus = value; }
            }
            private string index;

            public string Index
            {
                get { return index; }
                set { index = value; }
            }
            private string zInterval;

            public string ZInterval
            {
                get { return zInterval; }
                set { zInterval = value; }
            }
            private string maxDbSize;

            public string MaxDbSize
            {
                get { return maxDbSize; }
                set { maxDbSize = value; }
            }
            private string maxLongQuery;

            public string MaxLongQuery
            {
                get { return maxLongQuery; }
                set { maxLongQuery = value; }
            }
            private string maxLongTx;

            public string MaxLongTx
            {
                get { return maxLongTx; }
                set { maxLongTx = value; }
            }
            private string localDb;

            public string LocalDb
            {
                get { return localDb; }
                set { localDb = value; }
            }
            private string baseDir;

            public string BaseDir
            {
                get { return baseDir; }
                set { baseDir = value; }
            }
            private string localRoute;

            public string LocalRoute
            {
                get { return localRoute; }
                set { localRoute = value; }
            }
            private string availableStorage;

            public string AvailableStorage
            {
                get { return availableStorage; }
                set { availableStorage = value; }
            }
            private string host;

            public string Host
            {
                get { return host; }
                set { host = value; }
            }
            private string user;

            public string User
            {
                get { return user; }
                set { user = value; }
            }
            private string password;

            public string Password
            {
                get { return password; }
                set { password = value; }
            }
            private string port;

            public string Port
            {
                get { return port; }
                set { port = value; }
            }

            #endregion

            public InstallArguments(InstallContext context)
            {
                targetDir = context.Parameters[Argument.targetDir].TrimEnd('\\');
                nodeId = context.Parameters[Argument.nodeId];
                migrationNfs = context.Parameters[Argument.migrationNfs];
                mbus = context.Parameters[Argument.mbus];
                index = context.Parameters[Argument.index];
                zInterval = context.Parameters[Argument.zInterval];
                maxDbSize = context.Parameters[Argument.maxDbSize];
                maxLongQuery = context.Parameters[Argument.maxLongQuery];
                maxLongTx = context.Parameters[Argument.maxLongTx];
                localDb = context.Parameters[Argument.localDb];
                baseDir = context.Parameters[Argument.baseDir];
                localRoute = context.Parameters[Argument.localRoute];
                availableStorage = context.Parameters[Argument.availableStorage];
                host = context.Parameters[Argument.host];
                user = context.Parameters[Argument.user];
                password = context.Parameters[Argument.password];
                port = context.Parameters[Argument.port]; 
            }
        }
    }
}
