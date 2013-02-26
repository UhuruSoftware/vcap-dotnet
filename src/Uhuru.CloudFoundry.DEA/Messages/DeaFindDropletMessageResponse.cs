// -----------------------------------------------------------------------
// <copyright file="DeaFindDropletMessageResponse.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using System;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class encapsulates a response message to a <see cref="DeaFindDropletMessageRequest"/>
    /// </summary>
    public class DeaFindDropletMessageResponse : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the DEA service ID.
        /// </summary>
        [JsonName("dea")]
        public string DeaId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the droplet instance.
        /// </summary>
        [JsonName("version")]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the droplet id.
        /// </summary>
        [JsonName("droplet")]
        public string DropletId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instance id.
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
        /// Gets or sets the state of the droplet instance.
        /// </summary>
        [JsonName("state")]
        public DropletInstanceState State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the last snapshot timestamp for the droplet instance state in a ruby-compatible format.
        /// </summary>
        [JsonName("state_timestamp")]
        public int StateTimestampInterchangeableFormat
        {
            get { return RubyCompatibility.DateTimeToEpochSeconds(this.StateTimestamp); }
            set { this.StateTimestamp = RubyCompatibility.DateTimeFromEpochSeconds(value); }
        }

        /// <summary>
        /// Gets or sets the last snapshot timestamp for the droplet instance state.
        /// </summary>
        public DateTime StateTimestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the URI to the file server that can serve the droplet instance.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Suitable for the current context"), 
        JsonName("file_uri")]
        public string FileUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the URI to the file server V2 that can serve droplet instance files.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Suitable for the current context"), 
        JsonName("file_uri_v2")]
        public string FileUriV2
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the authentication credentials for the file server.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Suitable for the current context"), 
        JsonName("credentials")]
        public string[] FileAuth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the staged value.
        /// </summary>
        [JsonName("staged")]
        public string Staged
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the debug IP for the droplet instance.
        /// </summary>
        [JsonName("debug_ip")]
        public string DebugIP
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the debug port for the droplet instance.
        /// </summary>
        [JsonName("debug_port")]
        public int? DebugPort
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the stats of the droplet instance.
        /// </summary>
        [JsonName("stats")]
        public DropletStatusMessageResponse Stats
        {
            get;
            set;
        }
    }
}
