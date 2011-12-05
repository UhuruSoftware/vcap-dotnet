using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class RouterMessage : JsonConvertibleObject
    {

        [JsonName("dea")]
        public string DeaId;

        [JsonName("host")]
        public string Host;

        [JsonName("port")]
        public int Port;

        [JsonName("uris")]
        public List<string> Uris;

        public class TagsObject : JsonConvertibleObject
        {
            [JsonName("framework")]
            public string Framwork;

            [JsonName("runtime")]
            public string Runtime;

        }

        [JsonName("tags")]
        public TagsObject Tags;

    }
}
