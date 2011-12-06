// -----------------------------------------------------------------------
// <copyright file="ProjectInstaller.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MsSqlService.WindowsService
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using Uhuru.CloudFoundry.MSSqlService.WindowsService;
    using Uhuru.Configuration;

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

            if (!String.IsNullOrEmpty(arguments.LocalRoute))
            {
                section.Service.LocalRoute = arguments.LocalRoute;
            }
            else
            {
                string ip = "";
                foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ip = address.ToString();
                        break;
                    }
                }
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
            section.DEA = null;
            config.Save();
        }
    }
}
