using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DeaUpdateMessageRequest : JsonConvertibleObject
    {
        
        [JsonName("droplet")]
        public int DropletId
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

    }
}
