using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public enum DropletInstanceState
    {
        STARTING,
        RUNNING,
        STOPPED,
        DELETED,
        CRASHED

    }

    public enum DropletExitReason
    {
        NONE,
        DEA_EVACUATION,
        DEA_SHUTDOWN,
        STOPPED,
        CRASHED
    }


    public class DropletInstanceProperties : JsonConvertibleObject
    {


        [JsonName("state")]
        public string StateInterchangableFormat
        {
            get { return State.ToString(); }
            set { State = (DropletInstanceState)Enum.Parse(typeof(DropletInstanceState), value); }
        }
        public volatile DropletInstanceState State;


        [JsonName("exit_reason")]
        public string ExitReasonInterchangableFormat
        {
            get { return ExitReason != null ? ExitReason.ToString() : null; }
            set { ExitReason = value != null ? (DropletExitReason?)Enum.Parse(typeof(DropletExitReason), value) : null; }
        }
        public DropletExitReason? ExitReason;


        [JsonName("orphaned")]
        public bool Orphaned;


        [JsonName("start")]
        public string StartInterchangelbeFormat
        {
            get { return Utils.DateTimeToRubyString(Start); }
            set { Start = Utils.DateTimeFromRubyString(value); }
        }
        public DateTime Start;


        [JsonName("state_timestamp")]
        public int StateTimestampInterchangelbeFormat
        {
            get { return Utils.DateTimeToEpochSeconds(StateTimestamp); }
            set { StateTimestamp = Utils.DateTimeFromEpochSeconds(value); }
        }
        public DateTime StateTimestamp;


        [JsonName("resources_tracked")]
        public bool ResourcesTracked;

        [JsonName("stop_processed")]
        public volatile bool StopProcessed;

        [JsonName("debug_mode")]
        public string DebugMode;

        [JsonName("port")]
        public int Port;

        [JsonName("debug_port")]
        public int? DebugPort;

        [JsonName("debug_ip")]
        public string DebugIp;

        [JsonName("runtime")]
        public string Runtime;

        [JsonName("framework")]
        public string Framework;

        [JsonName("fds_quota")]
        public long FdsQuota;

        [JsonName("disk_quota")]
        public long DiskQuotaBytes;

        [JsonName("mem_quota")]
        public long MemoryQuotaBytes;

        [JsonName("name")]
        public string Name;

        [JsonName("instance_id")]
        public string InstanceId;

        [JsonName("version")]
        public string Version;

        [JsonName("droplet_id")]
        public int DropletId;

        [JsonName("instance_index")]
        public int InstanceIndex;

        [JsonName("dir")]
        public string Directory;

        [JsonName("uris")]
        public List<string> Uris;

        [JsonName("users")]
        public List<string> Users;

        [JsonName("log_id")]
        public string LoggingId;

        [JsonName("evacuated")]
        public bool Evacuated;

        [JsonName("pid")]
        public int Pid;

        [JsonName("notified")]
        public bool NotifiedExited;


        [JsonName("nice")]
        public int Nice;

        [JsonName("secure_user")]
        public string SecureUser;

        [JsonName("staged")]
        public string Staged;

        [JsonName("usage")]
        public DropletInstanceUsage UsageRecent;

    }
}
