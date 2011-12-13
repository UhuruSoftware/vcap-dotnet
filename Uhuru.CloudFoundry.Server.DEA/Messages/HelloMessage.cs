// -----------------------------------------------------------------------
// <copyright file="HelloMessage.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using Uhuru.Utilities;

    class HelloMessage : JsonConvertibleObject
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
    }
}
