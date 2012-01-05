// -----------------------------------------------------------------------
// <copyright file="DropletExitedMessage.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using System;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;
    
    /// <summary>
    /// This encapsulates a message that is sent after a droplet instance has exited.
    /// </summary>
    public class DropletExitedMessage : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the id of the droplet the instance belongs to.
        /// </summary>
        [JsonName("droplet")]
        public int DropletId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the droplet version.
        /// </summary>
        [JsonName("version")]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the id of the droplet instance.
        /// </summary>
        [JsonName("instance")]
        public string InstanceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the droplet instance.
        /// </summary>
        [JsonName("index")]
        public int Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the reason, if known, why the droplet instance has exited.
        /// </summary>
        [JsonName("reason")]
        public DropletExitReason? ExitReason
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timestamp corresponding to the moment the instance has crashed (if that is what happened), in interchangeable format.
        /// </summary>
        [JsonName("crash_timestamp")]
        public int? StateTimestampInterchangeableFormat
        {
            get { return this.CrashedTimestamp != null ? (int?)RubyCompatibility.DateTimeToEpochSeconds((DateTime)this.CrashedTimestamp) : null; }
            set { this.CrashedTimestamp = value != null ? (DateTime?)RubyCompatibility.DateTimeFromEpochSeconds((int)value) : null; }
        }

        /// <summary>
        /// Gets or sets the timestamp corresponding to the moment the instance has crashed (if that is what happened).
        /// </summary>
        public DateTime? CrashedTimestamp
        {
            get;
            set;
        }
    }
}
