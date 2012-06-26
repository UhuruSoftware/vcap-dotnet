// -----------------------------------------------------------------------
// <copyright file="ProjectInstaller.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService.WindowsService
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using Microsoft.Web.Administration;
    using Uhuru.Configuration;
    using Uhuru.Utilities;

    /// <summary>
    /// InstallerClass for MSSQLNode Service
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInstaller"/> class.
        /// </summary>
        public ProjectInstaller()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// When overridden in a derived class, performs the installation.
        /// </summary>
        /// <param name="stateSaver">An <see cref="T:System.Collections.IDictionary"/> used to save information needed to perform a commit, rollback, or uninstall operation.</param>
        /// <exception cref="T:System.ArgumentException">The <paramref name="stateSaver"/> parameter is null. </exception>
        /// Custom install method. Writes configuration values to uhuru.config
        /// <exception cref="T:System.Exception">An exception occurred in the <see cref="E:System.Configuration.Install.Installer.BeforeInstall"/> event handler of one of the installers in the collection.-or- An exception occurred in the <see cref="E:System.Configuration.Install.Installer.AfterInstall"/> event handler of one of the installers in the collection. </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification = "We need Assembly.Load to parse the uhuru configuration section.")]
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            string targetDir = Context.Parameters[Argument.TargetDir].TrimEnd('\\');
            string configFile = Path.Combine(targetDir, Assembly.GetExecutingAssembly().Location + ".config");

            System.Configuration.ConfigurationFileMap fileMap = new ConfigurationFileMap(configFile);

            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenMappedMachineConfiguration(fileMap);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(delegate(object sender, ResolveEventArgs args)
                {
                    return Assembly.LoadFile(Path.Combine(targetDir, args.Name + ".dll"));
                });

            UhuruSection section = (UhuruSection)config.GetSection("uhuru");

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.Capacity]))
            {
                section.Service.Capacity = int.Parse(Context.Parameters[Argument.Capacity], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.BaseDir]))
            {
                section.Service.BaseDir = Context.Parameters[Argument.BaseDir];
                if (!Directory.Exists(section.Service.BaseDir))
                {
                    Directory.CreateDirectory(section.Service.BaseDir);
                }
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.Index]))
            {
                section.Service.Index = int.Parse(Context.Parameters[Argument.Index], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.StatusPort]))
            {
                int port = Convert.ToInt32(Context.Parameters[Argument.StatusPort], CultureInfo.InvariantCulture);
                section.Service.StatusPort = port;
                if (port != 0)
                {
                    FirewallTools.OpenPort(port, "FileService Status");
                }
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.LocalDb]))
            {
                section.Service.LocalDB = Context.Parameters[Argument.LocalDb];
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.LocalRoute]))
            {
                section.Service.LocalRoute = Context.Parameters[Argument.LocalRoute];
            }
            else
            {
                string ip = string.Empty;
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

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.Mbus]))
            {
                section.Service.MBus = Context.Parameters[Argument.Mbus];
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.MigrationNfs]))
            {
                section.Service.MigrationNFS = Context.Parameters[Argument.MigrationNfs];
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.NodeId]))
            {
                section.Service.NodeId = Context.Parameters[Argument.NodeId];
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.ZInterval]))
            {
                section.Service.ZInterval = int.Parse(Context.Parameters[Argument.ZInterval], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.Plan]))
            {
                section.Service.Plan = Context.Parameters[Argument.Plan];
            }

            section.DEA = null;
            config.Save();

            int lowPort = 5000;
            int highPort = 6000;

            using (ServerManager serverManager = new ServerManager())
            {
                Microsoft.Web.Administration.Configuration iisConfig = serverManager.GetApplicationHostConfiguration();

                Microsoft.Web.Administration.ConfigurationSection firewallSupportSection = iisConfig.GetSection("system.ftpServer/firewallSupport");
                firewallSupportSection["lowDataChannelPort"] = lowPort;
                firewallSupportSection["highDataChannelPort"] = highPort;

                Microsoft.Web.Administration.ConfigurationSection sitesSection = iisConfig.GetSection("system.applicationHost/sites");
                Microsoft.Web.Administration.ConfigurationElement siteDefaultsElement = sitesSection.GetChildElement("siteDefaults");
                Microsoft.Web.Administration.ConfigurationElement ftpServerElement = siteDefaultsElement.GetChildElement("ftpServer");
                Microsoft.Web.Administration.ConfigurationElement firewallSupportElement = ftpServerElement.GetChildElement("firewallSupport");
                firewallSupportElement["externalIp4Address"] = @"0.0.0.0";

                serverManager.CommitChanges(); 
            }

            FirewallTools.OpenPortRange(lowPort, highPort, "UhuruFS Ports");
        }

        /// <summary>
        /// Raises the <see cref="E:System.Configuration.Install.Installer.AfterInstall"/> event.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary"/> that contains the state of the computer after all the installers contained in the <see cref="P:System.Configuration.Install.Installer.Installers"/> property have completed their installations.</param>
        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            ProcessStartInfo info = new ProcessStartInfo();
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.Arguments = "/C sc failure " + this.serviceInstaller1.ServiceName + " actions= restart/0 reset= 0";
            info.FileName = "cmd.exe";

            Process p = Process.Start(info);
            p.WaitForExit();
        }

        /// <summary>
        /// Class defining all argument names
        /// </summary>
        private class Argument
        {
            /// <summary>
            /// Target Directory
            /// </summary>
            public const string TargetDir = "TARGETDIR";

            /// <summary>
            /// Parameter name for nodeId
            /// </summary>
            public const string NodeId = "nodeId";

            /// <summary>
            /// Parameter name for migrationNfs
            /// </summary>
            public const string MigrationNfs = "migrationNfs";

            /// <summary>
            /// Parameter name for messageBus
            /// </summary>
            public const string Mbus = "mbus";

            /// <summary>
            /// Parameter name for index
            /// </summary>
            public const string Index = "index";

            /// <summary>
            /// Parameter name for zInterval
            /// </summary>
            public const string ZInterval = "zInterval";

            /// <summary>
            /// Parameter name for localDb
            /// </summary>
            public const string LocalDb = "localDb";

            /// <summary>
            /// Parameter name for baseDir
            /// </summary>
            public const string BaseDir = "baseDir";

            /// <summary>
            /// Parameter name for localRoute
            /// </summary>
            public const string LocalRoute = "localRoute";

            /// <summary>
            /// Parameter name for capacity
            /// </summary>
            public const string Capacity = "capacity";

            /// <summary>
            /// Parameter name for plan
            /// </summary>
            public const string Plan = "plan";

            /// <summary>
            /// Parameter name for StatusPort
            /// </summary>
            public const string StatusPort = "statusPort";

            /// <summary>
            /// Prevents a default instance of the <see cref="Argument"/> class from being created.
            /// </summary>
            private Argument()
            {
            }
        }
    }
}
