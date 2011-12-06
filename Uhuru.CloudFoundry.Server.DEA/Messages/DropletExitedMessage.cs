using System;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DropletExitedMessage : JsonConvertibleObject
    {

        [JsonName("droplet")]
        public int DropletId
        {
            get;
            set;
        }

        [JsonName("version")]
        public string Version
        {
            get;
            set;
        }

        [JsonName("instance")]
        public string InstanceId
        {
            get;
            set;
        }

        [JsonName("index")]
        public int Index
        {
            get;
            set;
        }

        [JsonName("reason")]
        public string ExitReasonInterchangeableFormat
        {
            get { return ExitReason != null ? ExitReason.ToString() : null; }
            set { ExitReason = value != null ? (DropletExitReason?)Enum.Parse(typeof(DropletExitReason), value) : null; }
        }

        public DropletExitReason? ExitReason
        {
            get;
            set;
        }

        [JsonName("crash_timestamp")]
        public int? StateTimestampInterchangeableFormat
        {
            get { return CrashedTimestamp != null ? (int?)RubyCompatibility.DateTimeToEpochSeconds((DateTime)CrashedTimestamp) : null; }
            set { CrashedTimestamp = value != null ? (DateTime?)RubyCompatibility.DateTimeFromEpochSeconds((int)value) : null; }
        }

        public DateTime? CrashedTimestamp
        {
            get;
            set;
        }
    }
}
