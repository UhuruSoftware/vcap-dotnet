using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DropletStatusMessageResponse : JsonConvertibleObject
    {
        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        [JsonName("host")]
        public string Host
        {
            get;
            set;
        }

        [JsonName("port")]
        public int Port
        {
            get;
            set;
        }

        [JsonName("uris")]
        public string[] Uris
        {
            get;
            set;
        }

        [JsonName("uptime")]
        public double Uptime
        {
            get;
            set;
        }

        [JsonName("mem_quota")]
        public long MemoryQuotaBytes
        {
            get;
            set;
        }

        [JsonName("disk_quota")]
        public long DiskQuotaBytes
        {
            get;
            set;
        }

        [JsonName("fds_quota")]
        public long FdsQuota
        {
            get;
            set;
        }

        [JsonName("usage")]
        public DropletInstanceUsage Usage
        {
            get;
            set;
        }

        [JsonName("cores")]
        public int? Cores
        {
            get;
            set;
        }
    }
}
