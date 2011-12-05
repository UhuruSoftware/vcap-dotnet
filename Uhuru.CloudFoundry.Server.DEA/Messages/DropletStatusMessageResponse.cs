using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DropletStatusMessageResponse : JsonConvertibleObject
    {
        [JsonName("name")]
        public string Name;

        [JsonName("host")]
        public string Host;

        [JsonName("port")]
        public int Port;

        [JsonName("uris")]
        public List<string> Uris;

        [JsonName("uptime")]
        public double Uptime;

        [JsonName("mem_quota")]
        public long MemoryQuotaBytes;

        [JsonName("disk_quota")]
        public long DiskQuotaBytes;

        [JsonName("fds_quota")]
        public long FdsQuota;

        [JsonName("usage")]
        public DropletInstanceUsage Usage;

        [JsonName("cores")]
        public int? Cores;


    }
}
