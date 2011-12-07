// -----------------------------------------------------------------------
// <copyright file="DropletInstance.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Uhuru.Utilities.ProcessPerformance;
    using Uhuru.CloudFoundry.Server.DEA.PluginBase;
	using System.Net.Sockets;
    using System.IO;
	
    public class DropletInstance
    {
        public const int MaxUsageSamples = 30;

        private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private DropletInstanceProperties properties = new DropletInstanceProperties();
        private List<DropletInstanceUsage> usage = new List<DropletInstanceUsage>();

        public IAgentPlugin Plugin;

        public ReaderWriterLockSlim Lock
        {
            get
            {
                return readerWriterLock;
            }

            set
            {
                readerWriterLock = value;
            }
        }

        public DropletInstanceProperties Properties
        {
            get
            {
                return properties;
            }

            set
            {
                properties = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<DropletInstanceUsage> Usage
        {
            get
            {
                return usage;
            }

            set
            {
                usage = value;
            }
        }

        public bool IsRunning
        {
            get
            {
                if (Properties.ProcessId == 0)
                    return false;

                return ProcessInformation.GetProcessUsage(Properties.ProcessId) != null;
            }
        }

        public bool IsPortReady
        {
            get
            {
                using (AutoResetEvent connectedEvent = new AutoResetEvent(false))
                {
                    using (TcpClient client = new TcpClient())
                    {
                        IAsyncResult result = client.BeginConnect("localhost", properties.Port, null, null);
                        result.AsyncWaitHandle.WaitOne(100);

                        if (client.Connected)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        
        public HeartbeatMessage.InstanceHeartbeat GenerateInstanceHeartbeat()
        {
            HeartbeatMessage.InstanceHeartbeat beat = new HeartbeatMessage.InstanceHeartbeat();
            try
            {
                Lock.EnterReadLock();
                
                beat.DropletId = Properties.DropletId;
                beat.Version = Properties.Version;
                beat.InstanceId = Properties.InstanceId;
                beat.InstanceIndex = Properties.InstanceIndex;
                beat.State = Properties.State;
                beat.StateTimestamp = Properties.StateTimestamp;
            }
            finally
            {
                Lock.ExitReadLock();
            }
            return beat;
        }

        public HeartbeatMessage GenerateHeartbeat()
        {
            HeartbeatMessage response = new HeartbeatMessage();
            response.Droplets.Add(GenerateInstanceHeartbeat().ToJsonIntermediateObject());
            return response;
        }

        /// <summary>
        /// returns the instances exited message
        /// </summary>
        public DropletExitedMessage GenerateDropletExitedMessage()
        {
            DropletExitedMessage response = new DropletExitedMessage();

            try
            {
                Lock.EnterReadLock();
                response.DropletId = Properties.DropletId;
                response.Version = Properties.Version;
                response.InstanceId = Properties.InstanceId;
                response.Index = Properties.InstanceIndex;
                response.ExitReason = Properties.ExitReason;

                if (Properties.State == DropletInstanceState.Crashed)
                {
                    response.CrashedTimestamp = Properties.StateTimestamp;
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }

            return response;

        }

        public DropletStatusMessageResponse GenerateDropletStatusMessage()
        {
            DropletStatusMessageResponse response = new DropletStatusMessageResponse();

            try
            {
                Lock.EnterReadLock();
                response.Name = Properties.Name;
                response.Port = Properties.Port;
                response.Uris = Properties.Uris;
                response.Uptime = (DateTime.Now - Properties.Start).TotalSeconds;
                response.MemoryQuotaBytes = Properties.MemoryQuotaBytes;
                response.DiskQuotaBytes = Properties.DiskQuotaBytes;
                response.FdsQuota = Properties.FdsQuota;
                if (Usage.Count > 0)
                {
                    response.Usage = Usage[Usage.Count - 1];
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }

            return response;
        }

        public ApplicationInfo PopulateApplicationInfo(ApplicationInfo appInfo)
        {
            if(appInfo == null) appInfo = new ApplicationInfo();
            appInfo.InstanceId = Properties.InstanceId;
            appInfo.Name = Properties.Name;
            appInfo.Path = Properties.Directory;
            appInfo.Port = Properties.Port;
            appInfo.WindowsPassword = Properties.WindowsPassword;
            appInfo.WindowsUsername = Properties.WindowsUsername;
            return appInfo;
        }


        public void LoadPlugin()
        {
            // in startup, we have the classname and assembly to load as a plugin
            string[] startMetadata = File.ReadAllLines(Path.Combine(Properties.Directory, "startup"));
            string assemblyName = startMetadata[0].Trim();
            string className = startMetadata[1].Trim();

            try
            {
                Guid PluginId = PluginHost.LoadPlugin(Path.Combine(Properties.Directory + assemblyName), className);
                Plugin = PluginHost.CreateInstance(PluginId);
            }
            catch { }

            if (Plugin == null)
            {
                Guid PluginId = PluginHost.LoadPlugin(assemblyName, className);
                Plugin = PluginHost.CreateInstance(PluginId);
            }
        }

        public void GenerateDeaFindDropletResponse()
        {
            throw new System.NotImplementedException();
        }

        public void DetectAppProcessId()
        {
            throw new System.NotImplementedException();
        }

        public void DetectAppReady()
        {
            throw new System.NotImplementedException();
        }
    }
}
