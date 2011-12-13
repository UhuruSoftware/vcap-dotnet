// -----------------------------------------------------------------------
// <copyright file="DeaUpdateMessageRequest.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using Uhuru.Utilities;
    
    public class DeaUpdateMessageRequest : JsonConvertibleObject
    {
        [JsonName("droplet")]
        public int DropletId { get; set; }

        [JsonName("uris")]
        public string[] Uris { get; set; }
    }
}
