// -----------------------------------------------------------------------
// <copyright file="DropletLimits.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using Uhuru.Utilities;

    /// <summary>
    /// This class contains information about the resource limits of a droplet.
    /// </summary>
    public class DropletLimits : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the maximum memory in megabytes.
        /// </summary>
        [JsonName("mem")]
        public long MemoryMbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum disk usage in megabytes.
        /// </summary>
        [JsonName("disk")]
        public long DiskMbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum number of open files and sockets.
        /// </summary>
        [JsonName("fds")]
        public long FDS
        {
            get;
            set;
        }
    }
}
