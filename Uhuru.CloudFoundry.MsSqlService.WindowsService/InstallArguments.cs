// -----------------------------------------------------------------------
// <copyright file="InstallArguments.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService.WindowsService
{
    using System.Configuration.Install;

    /// <summary>
    /// Helper class for parsing the arguments received by the installer
    /// </summary>
    internal class InstallArguments
    {
        /// <summary>
        /// Target Directory
        /// </summary>
        private string targetDir;

        /// <summary>
        /// nodeId argument value
        /// </summary>
        private string nodeId;

        /// <summary>
        /// migrationNfs argument value
        /// </summary>
        private string migrationNfs;

        /// <summary>
        /// mbus argument value
        /// </summary>
        private string mbus;

        /// <summary>
        /// index argument value
        /// </summary>
        private string index;

        /// <summary>
        /// zInterval argument value
        /// </summary>
        private string zInterval;

        /// <summary>
        /// maxDbSize argument value
        /// </summary>
        private string maxDbSize;

        /// <summary>
        /// maxLongQuery argument value
        /// </summary>
        private string maxLongQuery;

        /// <summary>
        /// maxLongTx argument value
        /// </summary>
        private string maxLongTx;

        /// <summary>
        /// localDb argument value
        /// </summary>
        private string localDb;

        /// <summary>
        /// baseDir argument value
        /// </summary>
        private string baseDir;

        /// <summary>
        /// localRoute argument value
        /// </summary>
        private string localRoute;

        /// <summary>
        /// availableStorage argument value
        /// </summary>
        private string availableStorage;

        /// <summary>
        /// host argument value
        /// </summary>
        private string host;

        /// <summary>
        /// user argument value
        /// </summary>
        private string user;

        /// <summary>
        /// password argument value
        /// </summary>
        private string password;

        /// <summary>
        /// port argument value
        /// </summary>
        private string port;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallArguments"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public InstallArguments(InstallContext context)
        {
            this.targetDir = context.Parameters[Argument.TargetDir].TrimEnd('\\');
            this.nodeId = context.Parameters[Argument.NodeId];
            this.migrationNfs = context.Parameters[Argument.MigrationNfs];
            this.mbus = context.Parameters[Argument.Mbus];
            this.index = context.Parameters[Argument.Index];
            this.zInterval = context.Parameters[Argument.ZInterval];
            this.maxDbSize = context.Parameters[Argument.MaxDbSize];
            this.maxLongQuery = context.Parameters[Argument.MaxLongQuery];
            this.maxLongTx = context.Parameters[Argument.MaxLongTx];
            this.localDb = context.Parameters[Argument.LocalDb];
            this.baseDir = context.Parameters[Argument.BaseDir];
            this.localRoute = context.Parameters[Argument.LocalRoute];
            this.availableStorage = context.Parameters[Argument.AvailableStorage];
            this.host = context.Parameters[Argument.Host];
            this.user = context.Parameters[Argument.User];
            this.password = context.Parameters[Argument.Password];
            this.port = context.Parameters[Argument.Port];
        }

        /// <summary>
        /// Gets the target dir.
        /// </summary>
        public string TargetDir
        {
            get { return this.targetDir; }
        }

        /// <summary>
        /// Gets the node id.
        /// </summary>
        public string NodeId
        {
            get { return this.nodeId; }
        }

        /// <summary>
        /// Gets the migration NFS.
        /// </summary>
        public string MigrationNfs
        {
            get { return this.migrationNfs; }
        }

        /// <summary>
        /// Gets the mbus.
        /// </summary>
        public string Mbus
        {
            get { return this.mbus; }
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        public string Index
        {
            get { return this.index; }
        }

        /// <summary>
        /// Gets the Z interval.
        /// </summary>
        public string ZInterval
        {
            get { return this.zInterval; }
        }

        /// <summary>
        /// Gets the size of the max db.
        /// </summary>
        /// <value>
        /// The max size of the db.
        /// </value>
        public string MaxDbSize
        {
            get { return this.maxDbSize; }
        }

        /// <summary>
        /// Gets the max long query.
        /// </summary>
        public string MaxLongQuery
        {
            get { return this.maxLongQuery; }
        }

        /// <summary>
        /// Gets the max long tx.
        /// </summary>
        public string MaxLongTx
        {
            get { return this.maxLongTx; }
        }

        /// <summary>
        /// Gets the local db.
        /// </summary>
        public string LocalDb
        {
            get { return this.localDb; }
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
        /// Gets the available storage.
        /// </summary>
        public string AvailableStorage
        {
            get { return this.availableStorage; }
        }

        /// <summary>
        /// Gets the host.
        /// </summary>
        public string Host
        {
            get { return this.host; }
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public string User
        {
            get { return this.user; }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password
        {
            get { return this.password; }
        }

        /// <summary>
        /// Gets the port.
        /// </summary>
        public string Port
        {
            get { return this.port; }
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
            /// Prevents a default instance of the <see cref="Argument"/> class from being created.
            /// </summary>
            private Argument()
            {
            }
        }
    }
}
