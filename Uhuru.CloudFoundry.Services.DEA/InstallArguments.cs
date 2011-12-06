// -----------------------------------------------------------------------
// <copyright file="InstallArguments.cs" company="">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.WindowsService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
using System.Configuration.Install;

    class InstallArguments
    {
        private class Arguments
        {
            public const string targetDir = "TARGETDIR";
            public const string baseDir = "baseDir";
            public const string localRoute = "localRoute";
            public const string filerPort = "filerPort";
            public const string messageBus = "messageBus";
            public const string multiTenant = "multiTenant";
            public const string maxMemory = "maxMemory";
            public const string secure = "secure";
            public const string enforceUlimit = "enforceUlimit";
            public const string heartBeatInterval = "heartBeatInterval";
            public const string forceHttpSharing = "forceHttpSharing";
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
        private string filerPort;

        public string FilerPort
        {
            get { return filerPort; }
            set { filerPort = value; }
        }
        private string messageBus;

        public string MessageBus
        {
            get { return messageBus; }
            set { messageBus = value; }
        }
        private string multiTenant;

        public string MultiTenant
        {
            get { return multiTenant; }
            set { multiTenant = value; }
        }
        private string maxMemory;

        public string MaxMemory
        {
            get { return maxMemory; }
            set { maxMemory = value; }
        }
        private string secure;

        public string Secure
        {
            get { return secure; }
            set { secure = value; }
        }
        private string enforceUlimit;

        public string EnforceUlimit
        {
            get { return enforceUlimit; }
            set { enforceUlimit = value; }
        }
        private string heartBeatInterval;

        public string HeartBeatInterval
        {
            get { return heartBeatInterval; }
            set { heartBeatInterval = value; }
        }
        private string forceHttpSharing;

        public string ForceHttpSharing
        {
            get { return forceHttpSharing; }
            set { forceHttpSharing = value; }
        }

        private string targetDir;

        public string TargetDir
        {
            get { return targetDir; }
            set { targetDir = value; }
        }

        public InstallArguments(InstallContext context)
        {
            baseDir = context.Parameters[Arguments.baseDir];
            localRoute = context.Parameters[Arguments.localRoute];
            filerPort = context.Parameters[Arguments.filerPort];
            messageBus = context.Parameters[Arguments.messageBus];
            multiTenant = context.Parameters[Arguments.multiTenant];
            maxMemory = context.Parameters[Arguments.maxMemory];
            secure = context.Parameters[Arguments.secure];
            enforceUlimit = context.Parameters[Arguments.enforceUlimit];
            heartBeatInterval = context.Parameters[Arguments.heartBeatInterval];
            forceHttpSharing = context.Parameters[Arguments.forceHttpSharing];
            targetDir = context.Parameters[Arguments.targetDir].TrimEnd('\\');
        }
    }
}
