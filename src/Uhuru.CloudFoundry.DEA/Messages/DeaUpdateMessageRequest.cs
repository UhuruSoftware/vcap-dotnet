// -----------------------------------------------------------------------
// <copyright file="DeaUpdateMessageRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class encapsulates a request message to udpate a droplet with new URLs
    /// </summary>
    public class DeaUpdateMessageRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the droplet id.
        /// </summary>
        [JsonName("droplet")]
        public string DropletId { get; set; }

        /// <summary>
        /// Gets or sets the new uris of the droplet.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It is used for JSON (de)serialization."), 
        JsonName("uris")]
        public string[] Uris { get; set; }
    }
}
