// -----------------------------------------------------------------------
// <copyright file="DropletInstance.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using Uhuru.CloudFoundry.Server.DEA.PluginBase;
    using Uhuru.Utilities;
    using Uhuru.Utilities.ProcessPerformance;

    public class DropletInstance
    {
        public const int MaxUsageSamples = 30;

        /// <summary>
        /// The lock for the droplet instance.
        /// </summary>
        private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Properties for the droplet instance which are saved when snapshoting the applications.
        /// </summary>
        private DropletInstanceProperties properties = new DropletInstanceProperties();

        /// <summary>
        /// The history of resource usage of the instance.
        /// </summary>
        private List<DropletInstanceUsage> usage = new List<DropletInstanceUsage>();

        /// <summary>
        /// The plugin associated with the instance.
        /// </summary>
        public IAgentPlugin Plugin;

        public ReaderWriterLockSlim Lock
        {
            get
            {
                return this.readerWriterLock;
            }

            set
            {
                this.readerWriterLock = value;
            }
        }

        public DropletInstanceProperties Properties
        {
            get
            {
                return this.properties;
            }

            set
            {
                this.properties = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<DropletInstanceUsage> Usage
        {
            get
            {
                return this.usage;
            }

            set
            {
                this.usage = value;
            }
        }
        
        /// <summary>
        /// Detect if the Pid is still valid and running
        /// </summary>
        public bool IsPidRunning
        {
            get
            {
                if (this.Properties.ProcessId == 0)
                {
                    return false;
                }

                return ProcessInformation.GetProcessUsage(this.Properties.ProcessId) != null;
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
                        IAsyncResult result = client.BeginConnect("localhost", this.properties.Port, null, null);
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

        /// <summary>
        /// Returns the heartbeat info of the current droplet instance.
        /// </summary>
        /// <returns>The requested heartbeat info.</returns>
        public HeartbeatMessage.InstanceHeartbeat GenerateInstanceHeartbeat()
        {
            HeartbeatMessage.InstanceHeartbeat beat = new HeartbeatMessage.InstanceHeartbeat();
            try
            {
                this.Lock.EnterReadLock();

                beat.DropletId = this.Properties.DropletId;
                beat.Version = this.Properties.Version;
                beat.InstanceId = this.Properties.InstanceId;
                beat.InstanceIndex = this.Properties.InstanceIndex;
                beat.State = this.Properties.State;
                beat.StateTimestamp = this.Properties.StateTimestamp;
            }
            finally
            {
                this.Lock.ExitReadLock();
            }

            return beat;
        }

        public HeartbeatMessage GenerateHeartbeat()
        {
            HeartbeatMessage response = new HeartbeatMessage();
            response.Droplets.Add(this.GenerateInstanceHeartbeat().ToJsonIntermediateObject());
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
                this.Lock.EnterReadLock();
                response.DropletId = this.Properties.DropletId;
                response.Version = this.Properties.Version;
                response.InstanceId = this.Properties.InstanceId;
                response.Index = this.Properties.InstanceIndex;
                response.ExitReason = this.Properties.ExitReason;

                if (this.Properties.State == DropletInstanceState.Crashed)
                {
                    response.CrashedTimestamp = this.Properties.StateTimestamp;
                }
            }
            finally
            {
                this.Lock.ExitReadLock();
            }

            return response;
        }

        /// <summary>
        /// Generates a status message reflecting the properties of the current droplet instance.
        /// </summary>
        /// <returns>The generated status message.</returns>
        public DropletStatusMessageResponse GenerateDropletStatusMessage()
        {
            DropletStatusMessageResponse response = new DropletStatusMessageResponse();

            try
            {
                this.Lock.EnterReadLock();
                response.Name = this.Properties.Name;
                response.Port = this.Properties.Port;
                response.Uris = this.Properties.Uris;
                response.Uptime = (DateTime.Now - this.Properties.Start).TotalSeconds;
                response.MemoryQuotaBytes = this.Properties.MemoryQuotaBytes;
                response.DiskQuotaBytes = this.Properties.DiskQuotaBytes;
                response.FdsQuota = this.Properties.FdsQuota;
                if (this.Usage.Count > 0)
                {
                    response.Usage = this.Usage[this.Usage.Count - 1];
                }
            }
            finally
            {
                this.Lock.ExitReadLock();
            }

            return response;
        }

        /// <summary>
        /// Updates an ApplicationInfo object with the information of the current droplet instance.
        /// </summary>
        /// <param name="appInfo">The object whose info is to be updated (if this is null a new ApplicationInfo object will be used instead).</param>
        /// <returns>The updated ApplicationInfo object.</returns>
        public ApplicationInfo PopulateApplicationInfo(ApplicationInfo appInfo)
        {
            if (appInfo == null)
            {
                appInfo = new ApplicationInfo();
            }

            appInfo.InstanceId = this.Properties.InstanceId;
            appInfo.Name = this.Properties.Name;
            appInfo.Path = this.Properties.Directory;
            appInfo.Port = this.Properties.Port;
            appInfo.WindowsPassword = this.Properties.WindowsPassword;
            appInfo.WindowsUserName = this.Properties.WindowsUsername;
            return appInfo;
        }

        public void LoadPlugin()
        {
            // in startup, we have the classname and assembly to load as a plugin
            string startup = File.ReadAllText(Path.Combine(this.Properties.Directory, "startup"));

            VcapPluginStagingInfo pluginInfo = new VcapPluginStagingInfo();
            pluginInfo.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(startup));

            this.ErrorLog = new Utilities.FileLogger(Path.Combine(this.properties.Directory, pluginInfo.Logs.DeaErrorLog));

            // check to see if the pluging is in the instance directory
            if (File.Exists(Path.Combine(this.Properties.Directory, pluginInfo.Assembly)))
            {
                Guid pluginId = PluginHost.LoadPlugin(Path.Combine(this.Properties.Directory, pluginInfo.Assembly), pluginInfo.ClassName);
                this.Plugin = PluginHost.CreateInstance(pluginId);
            }
            else
            {
                // if not load the plugin from the dea
                Guid PluginId = PluginHost.LoadPlugin(pluginInfo.Assembly, pluginInfo.ClassName);
                this.Plugin = PluginHost.CreateInstance(PluginId);
            }
        }

        public DropletInstanceUsage AddUsage(long memBytes, long cpu, long diskBytes)
        {
            DropletInstanceUsage curUsage = new DropletInstanceUsage();
            curUsage.Time = DateTime.Now;
            curUsage.Cpu = cpu;
            curUsage.MemoryKbytes = memBytes / 1024;
            curUsage.DiskBytes = diskBytes;

            this.Usage.Add(curUsage);
            if (this.Usage.Count > DropletInstance.MaxUsageSamples)
            {
                this.Usage.RemoveAt(0);
            }

            this.Properties.LastUsage = curUsage;
            return curUsage;
        }

        public FileLogger ErrorLog { get; set; }

        class VcapPluginStagingInfo : JsonConvertibleObject
        {
            [JsonName("assembly")]
            public string Assembly { get; set; }

            [JsonName("class_name")]
            public string ClassName { get; set; }

            [JsonName("logs")]
            public VcapPluginStagingInfoLogs Logs
            {
                get;
                set;
            }
        }

        class VcapPluginStagingInfoLogs : JsonConvertibleObject
        {
            [JsonName("dea_error")]
            public string DeaErrorLog
            {
                get;
                set;
            }
        }
    }
}
