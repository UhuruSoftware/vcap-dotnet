using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    //example: {"droplet":198,"limits":{"mem":128,"disk":2048,"fds":256},"name":"helloworld","runtime":"iis","sha":"98b1159c7d3539dd450fd86f92647d3902a0067b"}'
    public class DeaDiscoverMessageRequest : JsonConvertibleObject
    {

        [JsonName("droplet")]
        public int DropletId
        {
            get;
            set;
        }

        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        [JsonName("runtime")]
        public string Runtime
        {
            get;
            set;
        }

        [JsonName("sha")]
        public string Sha
        {
            get;
            set;
        }

        [JsonName("limits")]
        public DropletLimits Limits
        {
            get;
            set;
        }
        
        public DeaDiscoverMessageRequest()
        {
            Limits = new DropletLimits();
        }
    }

    public class DropletLimits : JsonConvertibleObject
    {

        [JsonName("mem")]
        public long MemoryMbytes
        {
            get;
            set;
        }

        [JsonName("disk")]
        public long DiskMbytes
        {
            get;
            set;
        }

        [JsonName("fds")]
        public long Fds
        {
            get;
            set;
        }
    }
}
