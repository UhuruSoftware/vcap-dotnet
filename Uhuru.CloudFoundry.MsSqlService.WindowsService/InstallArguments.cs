// -----------------------------------------------------------------------
// <copyright file="InstallArguments.cs" company="">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService.WindowsService
{
    using System.Configuration.Install;

    class InstallArguments
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

        #region Properties

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
