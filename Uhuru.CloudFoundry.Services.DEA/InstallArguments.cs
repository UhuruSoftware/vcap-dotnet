// -----------------------------------------------------------------------
// <copyright file="InstallArguments.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.WindowsService
{
    using System.Configuration.Install;

    /// <summary>
    /// Helper class for parsing the arguments received by the installer
    /// </summary>
    internal class InstallArguments
    {
        /// <summary>
        /// baseDir argument value
        /// </summary>
        private string baseDir;

        /// <summary>
        /// localRoute argument value
        /// </summary>
        private string localRoute;

        /// <summary>
        /// filerPort argument value
        /// </summary>
        private string filerPort;

        /// <summary>
        /// messageBus argument value
        /// </summary>
        private string messageBus;

        /// <summary>
        /// multiTenant argument value
        /// </summary>
        private string multiTenant;

        /// <summary>
        /// maxMemory argument value
        /// </summary>
        private string maxMemory;

        /// <summary>
        /// secure argument value
        /// </summary>
        private string secure;

        /// <summary>
        /// enforceUlimit argument value
        /// </summary>
        private string enforceUlimit;

        /// <summary>
        /// heartbeatInterval argument value
        /// </summary>
        private string heartBeatInterval;

        /// <summary>
        /// forceHttpSharing argument value
        /// </summary>
        private string forceHttpSharing;

        /// <summary>
        /// Target Directory
        /// </summary>
        private string targetDir;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallArguments"/> class.
        /// </summary>
        /// <param name="context">current installer context</param>
        public InstallArguments(InstallContext context)
        {
            this.baseDir = context.Parameters[Arguments.BaseDir];
            this.localRoute = context.Parameters[Arguments.LocalRoute];
            this.filerPort = context.Parameters[Arguments.FilerPort];
            this.messageBus = context.Parameters[Arguments.MessageBus];
            this.multiTenant = context.Parameters[Arguments.MultiTenant];
            this.maxMemory = context.Parameters[Arguments.MaxMemory];
            this.secure = context.Parameters[Arguments.Secure];
            this.enforceUlimit = context.Parameters[Arguments.EnforceUlimit];
            this.heartBeatInterval = context.Parameters[Arguments.HeartBeatInterval];
            this.forceHttpSharing = context.Parameters[Arguments.ForceHttpSharing];
            this.targetDir = context.Parameters[Arguments.TargetDir].TrimEnd('\\');
        }

        /// <summary>
        /// Gets the base dir.
        /// </summary>
        public string BaseDir
        {
            get { return this.baseDir; }
        }

        /// <summary>
        /// Gets the local route.
        /// </summary>
        public string LocalRoute
        {
            get { return this.localRoute; }
        }

        /// <summary>
        /// Gets the filer port.
        /// </summary>
        public string FilerPort
        {
            get { return this.filerPort; }
        }

        /// <summary>
        /// Gets the message bus.
        /// </summary>
        public string MessageBus
        {
            get { return this.messageBus; }
        }

        /// <summary>
        /// Gets the multi tenant.
        /// </summary>
        public string MultiTenant
        {
            get { return this.multiTenant; }
        }

        /// <summary>
        /// Gets the max memory.
        /// </summary>
        public string MaxMemory
        {
            get { return this.maxMemory; }
        }

        /// <summary>
        /// Gets the secure.
        /// </summary>
        public string Secure
        {
            get { return this.secure; }
        }

        /// <summary>
        /// Gets the enforce ulimit.
        /// </summary>
        public string EnforceUlimit
        {
            get { return this.enforceUlimit; }
        }

        /// <summary>
        /// Gets the heart beat interval.
        /// </summary>
        public string HeartBeatInterval
        {
            get { return this.heartBeatInterval; }
        }

        /// <summary>
        /// Gets the force HTTP sharing.
        /// </summary>
        public string ForceHttpSharing
        {
            get { return this.forceHttpSharing; }
        }

        /// <summary>
        /// Gets the target dir.
        /// </summary>
        public string TargetDir
        {
            get { return this.targetDir; }
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
            /// Paremeter name for BaseDir
            /// </summary>
            public const string BaseDir = "baseDir";

            /// <summary>
            /// Paremeter name for LocalRoute
            /// </summary>
            public const string LocalRoute = "localRoute";

            /// <summary>
            /// Paremeter name for FilerPort
            /// </summary>
            public const string FilerPort = "filerPort";

            /// <summary>
            /// Paremeter name for MessageBus
            /// </summary>
            public const string MessageBus = "messageBus";

            /// <summary>
            /// Paremeter name for MultiTenant
            /// </summary>
            public const string MultiTenant = "multiTenant";

            /// <summary>
            /// Paremeter name for MaxMemory
            /// </summary>
            public const string MaxMemory = "maxMemory";

            /// <summary>
            /// Paremeter name for Secure
            /// </summary>
            public const string Secure = "secure";

            /// <summary>
            /// Paremeter name for EnforceUlimit
            /// </summary>
            public const string EnforceUlimit = "enforceUlimit";

            /// <summary>
            /// Paremeter name for heartbeat
            /// </summary>
            public const string HeartBeatInterval = "heartBeatInterval";

            /// <summary>
            /// Paremeter name for forceHttpSharing
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
