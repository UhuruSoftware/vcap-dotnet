using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DropletExitedMessage : JsonConvertibleObject
    {

        [JsonName("droplet")]
        public int DropletId;

        [JsonName("version")]
        public string Version;

        [JsonName("instance")]
        public string InstanceId;

        [JsonName("index")]
        public int Index;


        [JsonName("reason")]
        public string ExitReasonInterchangableFormat
        {
            get { return ExitReason != null ? ExitReason.ToString() : null; }
            set { ExitReason = value != null ? (DropletExitReason?)Enum.Parse(typeof(DropletExitReason), value) : null; }
        }
        public DropletExitReason? ExitReason;

        [JsonName("crash_timestamp")]
        public int? StateTimestampInterchangelbeFormat
        {
            get { return CrashedTimestamp != null ? (int?)Utils.DateTimeToEpochSeconds((DateTime)CrashedTimestamp) : null; }
            set { CrashedTimestamp = value != null ? (DateTime?)Utils.DateTimeFromEpochSeconds((int)value) : null; }
        }
        public DateTime? CrashedTimestamp;

    }
}
