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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
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

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.LogicalStorageUnits]))
            {
                string lsu = Context.Parameters[Argument.LogicalStorageUnits];
                section.Service.MSSql.LogicalStorageUnits = lsu;
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.AvailableStorage]))
            {
                section.Service.AvailableStorage = long.Parse(Context.Parameters[Argument.AvailableStorage], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.BaseDir]))
            {
                section.Service.BaseDir = Context.Parameters[Argument.BaseDir];
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.Index]))
            {
                section.Service.Index = int.Parse(Context.Parameters[Argument.Index], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.StatusPort]))
            {
                int port = Convert.ToInt32(Context.Parameters[Argument.StatusPort], CultureInfo.InvariantCulture);
                section.Service.StatusPort = port;
                FirewallTools.OpenPort(port, "MsSqlNode Status");
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

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.MaxDbSize]))
            {
                section.Service.MaxDBSize = long.Parse(Context.Parameters[Argument.MaxDbSize], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.MaxLongQuery]))
            {
                section.Service.MaxLengthyQuery = int.Parse(Context.Parameters[Argument.MaxLongQuery], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.MaxLongTx]))
            {
                section.Service.MaxLengthyTX = int.Parse(Context.Parameters[Argument.MaxLongTx], CultureInfo.InvariantCulture);
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

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.Host]))
            {
                section.Service.MSSql.Host = Context.Parameters[Argument.Host];
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.Password]))
            {
                section.Service.MSSql.Password = Context.Parameters[Argument.Password];
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.Port]))
            {
                section.Service.MSSql.Port = int.Parse(Context.Parameters[Argument.Port], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Argument.User]))
            {
                section.Service.MSSql.User = Context.Parameters[Argument.User];
            }

            section.DEA = null;
            config.Save();
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
            /// Parameter name for maxDbSize
            /// </summary>
            public const string MaxDbSize = "maxDbSize";

            /// <summary>
            /// Parameter name for maxLongQuery
            /// </summary>
            public const string MaxLongQuery = "maxLongQuery";

            /// <summary>
            /// Parameter name for maxLongTx
            /// </summary>
            public const string MaxLongTx = "maxLongTx";

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
            /// Parameter name for availableStorage
            /// </summary>
            public const string AvailableStorage = "availableStorage";

            /// <summary>
            /// Parameter name for logical storage units
            /// </summary>
            public const string LogicalStorageUnits = "logicalStorageUnits";

            /// <summary>
            /// Parameter name for host
            /// </summary>
            public const string Host = "host";

            /// <summary>
            /// Parameter name for user
            /// </summary>
            public const string User = "user";

            /// <summary>
            /// Parameter name for password
            /// </summary>
            public const string Password = "password";

            /// <summary>
            /// Parameter name for port
            /// </summary>
            public const string Port = "port";

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
