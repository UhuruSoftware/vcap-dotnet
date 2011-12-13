// -----------------------------------------------------------------------
// <copyright file="ProjectInstaller.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService.WindowsService
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using Uhuru.Configuration;

    /// <summary>
    /// InstallerClass for MSSQLNode Service
    /// </summary>
    [RunInstaller(true)]
    internal partial class ProjectInstaller : System.Configuration.Install.Installer
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

            InstallArguments arguments = new InstallArguments(this.Context);
            string configFile = Path.Combine(arguments.TargetDir, Assembly.GetExecutingAssembly().Location + ".config");

            System.Configuration.ConfigurationFileMap fileMap = new ConfigurationFileMap(configFile);

            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenMappedMachineConfiguration(fileMap);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(delegate(object sender, ResolveEventArgs args)
                {
                    return Assembly.LoadFile(Path.Combine(arguments.TargetDir, args.Name + ".dll"));
                });

            UhuruSection section = (UhuruSection)config.GetSection("uhuru");

            if (!string.IsNullOrEmpty(arguments.AvailableStorage))
            {
                section.Service.AvailableStorage = int.Parse(arguments.AvailableStorage, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(arguments.BaseDir))
            {
                section.Service.BaseDir = arguments.BaseDir;
            }

            if (!string.IsNullOrEmpty(arguments.Index))
            {
                section.Service.Index = int.Parse(arguments.Index, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(arguments.LocalDb))
            {
                section.Service.LocalDB = arguments.LocalDb;
            }

            if (!string.IsNullOrEmpty(arguments.LocalRoute))
            {
                section.Service.LocalRoute = arguments.LocalRoute;
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

            if (!string.IsNullOrEmpty(arguments.MaxDbSize))
            {
                section.Service.MaxDBSize = int.Parse(arguments.MaxDbSize, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(arguments.MaxLongQuery))
            {
                section.Service.MaxLengthyQuery = int.Parse(arguments.MaxLongQuery, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(arguments.MaxLongTx))
            {
                section.Service.MaxLengthyTX = int.Parse(arguments.MaxLongTx, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(arguments.Mbus))
            {
                section.Service.MBus = arguments.Mbus;
            }

            if (!string.IsNullOrEmpty(arguments.MigrationNfs))
            {
                section.Service.MigrationNFS = arguments.MigrationNfs;
            }

            if (!string.IsNullOrEmpty(arguments.NodeId))
            {
                section.Service.NodeId = arguments.NodeId;
            }

            if (!string.IsNullOrEmpty(arguments.ZInterval))
            {
                section.Service.ZInterval = int.Parse(arguments.ZInterval, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(arguments.Host))
            {
                section.Service.MSSql.Host = arguments.Host;
            }

            if (!string.IsNullOrEmpty(arguments.Password))
            {
                section.Service.MSSql.Password = arguments.Password;
            }

            if (!string.IsNullOrEmpty(arguments.Port))
            {
                section.Service.MSSql.Port = int.Parse(arguments.Port, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(arguments.User))
            {
                section.Service.MSSql.User = arguments.User;
            }

            section.DEA = null;
            config.Save();
        }
    }
}
