using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DeaUpdateMessageRequest : JsonConvertibleObject
    {
        
        [JsonName("droplet")]
        public int DropletId;

        [JsonName("uris")]
        public List<string> Uris;

    }
}
