// -----------------------------------------------------------------------
// <copyright file="DropletInstanceProperties.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// An enum containing the possible states a droplet instance can be in.
    /// </summary>
    public enum DropletInstanceState
    {
        /// <summary>
        /// The instance is in a starting state.
        /// </summary>
        [JsonName("STARTING")]
        Starting,

        /// <summary>
        /// The instance is healthy and running.
        /// </summary>
        [JsonName("RUNNING")]
        Running,

        /// <summary>
        /// The instance is stopped gracefully.
        /// </summary>
        [JsonName("STOPPED")]
        Stopped,

        /// <summary>
        /// A crashed instance was deleted.
        /// </summary>
        [JsonName("DELETED")]
        Deleted,

        /// <summary>
        /// The instance crashed after starting or running.
        /// </summary>
        [JsonName("CRASHED")]
        Crashed
    }

    /// <summary>
    /// An enum containing the possible exit reasons for a droplet.
    /// </summary>
    public enum DropletExitReason
    {
        /// <summary>
        /// The instance is evacuated. Set when the evacuate routine is invoked.
        /// </summary>
        [JsonName("DEA_EVACUATION")]
        DeaEvacuation,

        /// <summary>
        /// The instance was stopped because the DEA is shutting down.
        /// </summary>
        [JsonName("DEA_SHUTDOWN")]
        DeaShutdown,

        /// <summary>
        /// The instance was gracefully stopped.
        /// </summary>
        [JsonName("STOPPED")]
        Stopped,

        /// <summary>
        /// The instance is not running because it crashed.
        /// </summary>
        [JsonName("CRASHED")]
        Crashed
    }

    /// <summary>
    /// JSON serializable instance properties.
    /// </summary>
    public class DropletInstanceProperties : JsonConvertibleObject
    {
        /// <summary>
        /// Indicated if the StopDroplet routine was completely invoked on this instance.
        /// </summary>
        private bool stopProcessed;

        /// <summary>
        /// Gets or sets the state of a droplet instance at a given time.
        /// </summary>
        [JsonName("state")]
        public DropletInstanceState State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the exit reason.
        /// </summary>
        [JsonName("exit_reason")]
        public DropletExitReason? ExitReason
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DropletInstanceProperties"/> is orphaned. It is set when a running instance is recoverd.
        /// </summary>
        /// <value>
        ///   <c>true</c> if orphaned; otherwise, <c>false</c>.
        /// </value>
        [JsonName("orphaned")]
        public bool Orphaned
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instance start timestamp in ruby format.
        /// </summary>
        [JsonName("start")]
        public string StartInterchangeableFormat
        {
            get { return RubyCompatibility.DateTimeToRubyString(this.Start); }
            set { this.Start = RubyCompatibility.DateTimeFromRubyString(value); }
        }

        /// <summary>
        /// Gets or sets the instance start timestamp.
        /// </summary>
        public DateTime Start
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the state timestamp interchangeable format. The state timestamp is updated when the instance state is changed.
        /// </summary>
        [JsonName("state_timestamp")]
        public int StateTimestampInterchangeableFormat
        {
            get { return RubyCompatibility.DateTimeToEpochSeconds(this.StateTimestamp); }
            set { this.StateTimestamp = RubyCompatibility.DateTimeFromEpochSeconds(value); }
        }

        /// <summary>
        /// Gets or sets the state timestamp. The state timestamp is updated when the instance state is changed.
        /// </summary>
        public DateTime StateTimestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [resources tracked]. Flag if the instance resources have been accounted to avoid tracking or untracking them several times.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [resources tracked]; otherwise, <c>false</c>.
        /// </value>
        [JsonName("resources_tracked")]
        public bool ResourcesTracked
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [stop processed]. Indicated if the StopDroplet routine was completely invoked on this instance.
        /// </summary>
        [JsonName("stop_processed")]
        public bool StopProcessed
        {
            get
            {
                return this.stopProcessed;
            }

            set
            {
                this.stopProcessed = value;
            }
        }

        /// <summary>
        /// Gets or sets the debug mode. This is received from the start message.
        /// </summary>
        [JsonName("debug_mode")]
        public string DebugMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port the application is listening on.
        /// </summary>
        [JsonName("port")]
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the debug port the application is listening on.
        /// </summary>
        [JsonName("debug_port")]
        public int? DebugPort
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the debug IP to connect to.
        /// </summary>
        [JsonName("debug_ip")]
        public string DebugIP
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the runtime the application is using.
        /// </summary>
        [JsonName("runtime")]
        public string Runtime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the framework the application is using.
        /// </summary>
        [JsonName("framework")]
        public string Framework
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the File Descriptors Quota.
        /// </summary>
        [JsonName("fds_quota")]
        public long FDSQuota
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the disk memory quota.
        /// </summary>
        [JsonName("disk_quota")]
        public long DiskQuotaBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the RAM quota.
        /// </summary>
        [JsonName("mem_quota")]
        public long MemoryQuotaBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the application/droplet.
        /// </summary>
        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unique instance id.
        /// </summary>
        [JsonName("instance_id")]
        public string InstanceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the application/droplet.
        /// </summary>
        [JsonName("version")]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the droplet/application id.
        /// </summary>
        [JsonName("droplet_id")]
        public int DropletId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the instance respective to a specific droplet/application.
        /// </summary>
        [JsonName("instance_index")]
        public int InstanceIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the directory the instance is stored.
        /// </summary>
        [JsonName("dir")]
        public string Directory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the uris the application/droplet is assigned.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It is used for JSON (de)serialization."), 
        JsonName("uris")]
        public string[] Uris
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the VCAP users that are associated to the application/droplet.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It is used for JSON (de)serialization."), 
        JsonName("users")]
        public string[] Users
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ID used for logging events for the instance.
        /// </summary>
        [JsonName("log_id")]
        public string LoggingId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DropletInstanceProperties"/> is evacuated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if evacuated; otherwise, <c>false</c>.
        /// </value>
        [JsonName("evacuated")]
        public bool Evacuated
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the process id associated to the instance. Used to track the application resource usage.
        /// </summary>
        [JsonName("pid")]
        public int ProcessId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the droplet.exited is sent through the message bus.
        /// </summary>
        [JsonName("notified")]
        public bool NotifiedExited
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the nice/priority value of the process associated to the instance.
        /// </summary>
        [JsonName("nice")]
        public int Nice
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the secure user.
        /// </summary>s
        [JsonName("secure_user")]
        public string SecureUser
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the staged directory.
        /// </summary>
        [JsonName("staged")]
        public string Staged
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the last usage
        /// </summary>
        [JsonName("usage")]
        public DropletInstanceUsage LastUsage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the windows username used for the instance.
        /// </summary>
        [JsonName("windows_username")]
        public string WindowsUserName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the windows password associated with the windows user.
        /// </summary>
        [JsonName("windows_password")]
        public string WindowsPassword
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the application variables used to start the instance. Also used when trying to recover the instance.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "It is used for JSON (de)serialization."), 
        JsonName("environment_variables")]
        public Dictionary<string, string> EnvironmentVariables
        {
            get;
            set;
        }
    }
}
