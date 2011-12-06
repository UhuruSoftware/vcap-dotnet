using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DeaStatusMessageResponse : JsonConvertibleObject
    {

        [JsonName("id")]
        public string Id
        {
            get;
            set;
        }

        [JsonName("version")]
        public decimal Version
        {
            get;
            set;
        }

        [JsonName("ip")]
        public string Host
        {
            get;
            set;
        }

        [JsonName("port")]
        public int FileViewerPort
        {
            get;
            set;
        }

        [JsonName("max_memory")]
        public long MaxMemoryMbytes
        {
            get;
            set;
        }

        [JsonName("reserved_memory")]
        public long MemoryReservedMbytes
        {
            get;
            set;
        }

        [JsonName("used_memory")]
        public long MemoryUsageKbytes
        {
            get;
            set;
        }

        [JsonName("num_clients")]
        public long NumberOfClients
        {
            get;
            set;
        }

        [JsonName("state")]
        public string State
        {
            get;
            set;
        }
    }
}
