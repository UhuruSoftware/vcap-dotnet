// -----------------------------------------------------------------------
// <copyright file="DropletStatusMessageResponse.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// ----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using Uhuru.Utilities;
    
    /// <summary>
    /// A class containing a set of data reflecting the status of a droplet instance.
    /// </summary>
    public class DropletStatusMessageResponse : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the name of the droplet.
        /// </summary>
        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the host of the DEA service hosting the droplet.
        /// </summary>
        [JsonName("host")]
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port that the droplet instance uses.
        /// </summary>
        [JsonName("port")]
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the mapped URLs of the droplet.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It is used for JSON (de)serialization."), 
        JsonName("uris")]
        public string[] Uris
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the uptime of the droplet.
        /// </summary>
        [JsonName("uptime")]
        public double Uptime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the memory quota in bytes.
        /// </summary>
        [JsonName("mem_quota")]
        public long MemoryQuotaBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the disk quota in bytes.
        /// </summary>
        [JsonName("disk_quota")]
        public long DiskQuotaBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the quota files and sockets.
        /// </summary>
        [JsonName("fds_quota")]
        public long FDSQuota
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the resource usage for a droplet instance.
        /// </summary>
        [JsonName("usage")]
        public DropletInstanceUsage Usage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of cores available to the droplet instance.
        /// </summary>
        [JsonName("cores")]
        public int? Cores
        {
            get;
            set;
        }
    }
}
