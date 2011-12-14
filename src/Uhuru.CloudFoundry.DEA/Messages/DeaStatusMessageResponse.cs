// -----------------------------------------------------------------------
// <copyright file="DeaStatusMessageResponse.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using Uhuru.Utilities;

    /// <summary>
    /// This class is a representation of a DEA status message response.
    /// </summary>
    public class DeaStatusMessageResponse : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the id of the DEA service.
        /// </summary>
        [JsonName("id")]
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the DEA component.
        /// </summary>
        [JsonName("version")]
        public decimal Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ip of the machine hosting the DEA service.
        /// </summary>
        [JsonName("ip")]
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file viewer port.
        /// </summary>
        [JsonName("port")]
        public int FileViewerPort
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the max memory to be used by the DEA in megabytes.
        /// </summary>
        [JsonName("max_memory")]
        public long MaxMemoryMbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the reserved memory of the DEA in megabytes (sum of max memory for each droplet instance).
        /// </summary>
        [JsonName("reserved_memory")]
        public long MemoryReservedMbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the memory usage of the DEA in kilobytes.
        /// </summary>
        [JsonName("used_memory")]
        public long MemoryUsageKbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of instances hosted by the DEA service.
        /// </summary>
        [JsonName("num_clients")]
        public long NumberOfClients
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the state of the DEA service.
        /// </summary>
        [JsonName("state")]
        public string State
        {
            get;
            set;
        }
    }
}
