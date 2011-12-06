using System;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DropletInstanceUsage : JsonConvertibleObject
    {

        [JsonName("mem")]
        public long MemoryKbytes
        {
            get;
            set;
        }

        [JsonName("cpu")]
        public long Cpu
        {
            get;
            set;
        }

        [JsonName("disk")]
        public long DiskBytes
        {
            get;
            set;
        }

        [JsonName("time")]
        public int TimeInterchangeableFormat
        {
            get { return RubyCompatibility.DateTimeToEpochSeconds(Time); }
            set { Time = RubyCompatibility.DateTimeFromEpochSeconds(value); }
        }

        public DateTime Time
        {
            get;
            set;
        }
    }
}
