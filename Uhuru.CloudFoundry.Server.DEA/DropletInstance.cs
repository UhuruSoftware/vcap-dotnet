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
    using Uhuru.Utilities;

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
        
        /// <summary>
        /// Detect if the Pid is still valid and running
        /// </summary>
        public bool IsPidRunning
        {
            get
            {
                if (Properties.ProcessId == 0)
                {
                    return false;
                }

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

        /// <summary>
        /// Returns the heartbeat info of the current droplet instance.
        /// </summary>
        /// <returns>The requested heartbeat info.</returns>
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

        /// <summary>
        /// Generates a status message reflecting the properties of the current droplet instance.
        /// </summary>
        /// <returns>The generated status message.</returns>
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
            string startup = File.ReadAllText(Path.Combine(Properties.Directory, "startup"));

            VcapPluginStagingInfo pluginInfo = new VcapPluginStagingInfo();
            pluginInfo.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(startup));

            ErrorLog = new Utilities.FileLogger(Path.Combine(properties.Directory, pluginInfo.Logs.DeaErrorLog));

            // check to see if the pluging is in the instance directory
            if (File.Exists(Path.Combine(Properties.Directory, pluginInfo.Assembly)))
            {
                Guid PluginId = PluginHost.LoadPlugin(Path.Combine(Properties.Directory, pluginInfo.Assembly), pluginInfo.ClassName);
                Plugin = PluginHost.CreateInstance(PluginId);
            }
            else
            // if not load the plugin from the dea
            {
                Guid PluginId = PluginHost.LoadPlugin(pluginInfo.Assembly, pluginInfo.ClassName);
                Plugin = PluginHost.CreateInstance(PluginId);
            }
        }

        public DropletInstanceUsage AddUsage(long memBytes, long cpu, long diskBytes)
        {
            DropletInstanceUsage curUsage = new DropletInstanceUsage();
            curUsage.Time = DateTime.Now;
            curUsage.Cpu = cpu;
            curUsage.MemoryKbytes = memBytes / 1024;
            curUsage.DiskBytes = diskBytes;

            Usage.Add(curUsage);
            if (Usage.Count > DropletInstance.MaxUsageSamples)
            {
                Usage.RemoveAt(0);
            }

            Properties.LastUsage = curUsage;
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
