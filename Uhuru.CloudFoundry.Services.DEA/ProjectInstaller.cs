// -----------------------------------------------------------------------
// <copyright file="ProjectInstaller.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.WindowsService
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using Uhuru.Configuration;
    using System.Diagnostics;
    using System.Globalization;

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

            Debugger.Launch();

            InstallArguments arguments = new InstallArguments(this.Context);
            string configFile = Path.Combine(arguments.TargetDir, Assembly.GetExecutingAssembly().Location + ".config");

            System.Configuration.ConfigurationFileMap fileMap = new ConfigurationFileMap(configFile);

            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenMappedMachineConfiguration(fileMap);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(delegate(object sender, ResolveEventArgs args)
            {
                return Assembly.LoadFile(Path.Combine(arguments.TargetDir, args.Name + ".dll"));
            });

            UhuruSection section = (UhuruSection)config.GetSection("uhuru");
            if (!String.IsNullOrEmpty(arguments.BaseDir))
            {
                section.DEA.BaseDir = arguments.BaseDir;
            }
            if (!String.IsNullOrEmpty(arguments.EnforceUlimit))
            {
                section.DEA.EnforceUlimit = Convert.ToBoolean(arguments.EnforceUlimit, CultureInfo.InvariantCulture);
            }
            if (!String.IsNullOrEmpty(arguments.FilerPort))
            {
                section.DEA.FilerPort = Convert.ToInt32(arguments.FilerPort, CultureInfo.InvariantCulture);
            }
            if (!String.IsNullOrEmpty(arguments.ForceHttpSharing))
            {
                section.DEA.ForceHttpSharing = Convert.ToBoolean(arguments.ForceHttpSharing, CultureInfo.InvariantCulture);
            }
            if (!String.IsNullOrEmpty(arguments.HeartBeatInterval))
            {
                section.DEA.HeartBeatInterval = Convert.ToInt32(arguments.HeartBeatInterval, CultureInfo.InvariantCulture);
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
            if (!String.IsNullOrEmpty(arguments.MaxMemory))
            {
                section.DEA.MaxMemory = Convert.ToInt32(arguments.MaxMemory, CultureInfo.InvariantCulture);
            }
            if (!String.IsNullOrEmpty(arguments.MessageBus))
            {
                section.DEA.MessageBus = arguments.MessageBus;
            }
            if (!String.IsNullOrEmpty(arguments.MultiTenant))
            {
                section.DEA.MultiTenant = Convert.ToBoolean(arguments.MultiTenant, CultureInfo.InvariantCulture);
            }
            if (!String.IsNullOrEmpty(arguments.Secure))
            {
                section.DEA.Secure = Convert.ToBoolean(arguments.Secure, CultureInfo.InvariantCulture);
            }

            section.Service = null;
            config.Save();
        }
    }
}
