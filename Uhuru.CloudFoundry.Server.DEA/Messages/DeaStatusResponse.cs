using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DeaStatusMessageResponse : JsonConvertibleObject
    {

        [JsonName("id")]
        public string Id;

        [JsonName("version")]
        public decimal Version;

        [JsonName("ip")]
        public string Host;

        [JsonName("port")]
        public int FileViewerPort;

        [JsonName("max_memory")]
        public long MaxMemoryMbytes;

        [JsonName("reserved_memory")]
        public long MemoryReservedMbytes;

        [JsonName("used_memory")]
        public long MermoryUsageKbytes;

        [JsonName("num_clients")]
        public long NumberOfClients;

        [JsonName("state")]
        public string State;


    }
}
