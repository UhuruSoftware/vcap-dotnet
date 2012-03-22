// -----------------------------------------------------------------------
// <copyright file="ProjectInstaller.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.WindowsService
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
    /// Installer Class for Windows DEA Service
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification = "No way around this right now.")]
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            string targetDir = Context.Parameters[Arguments.TargetDir].TrimEnd('\\');
            string configFile = Path.Combine(targetDir, Assembly.GetExecutingAssembly().Location + ".config");

            System.Configuration.ConfigurationFileMap fileMap = new ConfigurationFileMap(configFile);
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenMappedMachineConfiguration(fileMap);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(delegate(object sender, ResolveEventArgs args)
            {
                return Assembly.LoadFile(Path.Combine(targetDir, args.Name + ".dll"));
            });

            UhuruSection section = (UhuruSection)config.GetSection("uhuru");
            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.BaseDir]))
            {
                section.DEA.BaseDir = Context.Parameters[Arguments.BaseDir];
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.EnforceUlimit]))
            {
                section.DEA.EnforceUsageLimit = Convert.ToBoolean(Context.Parameters[Arguments.EnforceUlimit], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.FilerPort]))
            {
                int port = Convert.ToInt32(Context.Parameters[Arguments.FilerPort], CultureInfo.InvariantCulture);
                section.DEA.FilerPort = port;
                FirewallTools.OpenPort(port, "DEA FileServer");
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.StatusPort]))
            {
                int port = Convert.ToInt32(Context.Parameters[Arguments.StatusPort], CultureInfo.InvariantCulture);
                section.DEA.StatusPort = port;
                FirewallTools.OpenPort(port, "DEA Status");
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.ForceHttpSharing]))
            {
                section.DEA.ForceHttpSharing = Convert.ToBoolean(Context.Parameters[Arguments.ForceHttpSharing], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.HeartBeatInterval]))
            {
                section.DEA.HeartbeatInterval = Convert.ToInt32(Context.Parameters[Arguments.HeartBeatInterval], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.LocalRoute]))
            {
                section.DEA.LocalRoute = Context.Parameters[Arguments.LocalRoute];
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

                section.DEA.LocalRoute = ip;
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.MaxMemory]))
            {
                section.DEA.MaxMemory = Convert.ToInt32(Context.Parameters[Arguments.MaxMemory], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.MessageBus]))
            {
                section.DEA.MessageBus = Context.Parameters[Arguments.MessageBus];
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.MultiTenant]))
            {
                section.DEA.Multitenant = Convert.ToBoolean(Context.Parameters[Arguments.MultiTenant], CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(Context.Parameters[Arguments.Secure]))
            {
                section.DEA.Secure = Convert.ToBoolean(Context.Parameters[Arguments.Secure], CultureInfo.InvariantCulture);
            }

            section.Service = null;
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
        private class Arguments
        {
            /// <summary>
            /// Directory where service is being installed
            /// </summary>
            public const string TargetDir = "TARGETDIR";

            /// <summary>
            /// Parameter name for BaseDir
            /// </summary>
            public const string BaseDir = "baseDir";

            /// <summary>
            /// Parameter name for LocalRoute
            /// </summary>
            public const string LocalRoute = "localRoute";

            /// <summary>
            /// Parameter name for FilerPort
            /// </summary>
            public const string FilerPort = "filerPort";

            /// <summary>
            /// Parameter name for StatusPort
            /// </summary>
            public const string StatusPort = "statusPort";

            /// <summary>
            /// Parameter name for MessageBus
            /// </summary>
            public const string MessageBus = "messageBus";

            /// <summary>
            /// Parameter name for MultiTenant
            /// </summary>
            public const string MultiTenant = "multiTenant";

            /// <summary>
            /// Parameter name for MaxMemory
            /// </summary>
            public const string MaxMemory = "maxMemory";

            /// <summary>
            /// Parameter name for Secure
            /// </summary>
            public const string Secure = "secure";

            /// <summary>
            /// Parameter name for EnforceUlimit
            /// </summary>
            public const string EnforceUlimit = "enforceUlimit";

            /// <summary>
            /// Parameter name for heartbeat
            /// </summary>
            public const string HeartBeatInterval = "heartBeatInterval";

            /// <summary>
            /// Parameter name for forceHttpSharing
            /// </summary>
            public const string ForceHttpSharing = "forceHttpSharing";

            /// <summary>
            /// Prevents a default instance of the <see cref="Arguments"/> class from being created.
            /// </summary>
            private Arguments()
            {
            }
        }
    }
}
