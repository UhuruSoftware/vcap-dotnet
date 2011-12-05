using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DropletInstanceUsage : JsonConvertibleObject
    {

        [JsonName("mem")]
        public long MemoryKbytes;

        [JsonName("cpu")]
        public long Cpu;

        [JsonName("disk")]
        public long DiskBytes;


        
        [JsonName("time")]
        public int TimeInterchangelbeFormat
        {
            get { return Utils.DateTimeToEpochSeconds(Time); }
            set { Time = Utils.DateTimeFromEpochSeconds(value); }
        }
        public DateTime Time;

    }
}
