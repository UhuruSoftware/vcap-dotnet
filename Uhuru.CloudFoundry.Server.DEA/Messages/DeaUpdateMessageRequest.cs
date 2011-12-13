// -----------------------------------------------------------------------
// <copyright file="DeaUpdateMessageRequest.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using Uhuru.Utilities;

    /// <summary>
    /// This class encapsulates a request message to udpate a droplet with new URLs
    /// </summary>
    public class DeaUpdateMessageRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the droplet id.
        /// </summary>
        [JsonName("droplet")]
        public int DropletId { get; set; }

        /// <summary>
        /// Gets or sets the new uris of the droplet.
        /// </summary>
        [JsonName("uris")]
        public string[] Uris { get; set; }
    }
}
