// -----------------------------------------------------------------------
// <copyright file="RouterMessage.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using Uhuru.Utilities;

    public class RouterMessage : JsonConvertibleObject
    {
        [JsonName("dea")]
        public string DeaId
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

        public class TagsObject : JsonConvertibleObject
        {
            [JsonName("framework")]
            public string Framework
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
        }

        [JsonName("tags")]
        public TagsObject Tags
        {
            get;
            set;
        }
    }
}