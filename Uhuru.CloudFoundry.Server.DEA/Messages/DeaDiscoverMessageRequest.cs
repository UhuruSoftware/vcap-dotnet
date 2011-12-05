using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    //example: {"droplet":198,"limits":{"mem":128,"disk":2048,"fds":256},"name":"helloworld","runtime":"iis","sha":"98b1159c7d3539dd450fd86f92647d3902a0067b"}'
    public class DeaDiscoverMessageRequest : JsonConvertibleObject
    {

        [JsonName("droplet")]
        public int DropletId;

        [JsonName("name")]
        public string Name;

        [JsonName("runtime")]
        public string Runtime;

        [JsonName("sha")]
        public string Sha;

        [JsonName("limits")]
        public DropletLimits Limits = new DropletLimits();

        public class DropletLimits : JsonConvertibleObject
        {

            [JsonName("mem")]
            public long MemoryMbytes;

            [JsonName("disk")]
            public long DiskMbytes;

            [JsonName("fds")]
            public long Fds;

        }


    }
}
