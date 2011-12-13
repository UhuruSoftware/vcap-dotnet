// -----------------------------------------------------------------------
// <copyright file="DeaFindDropletMessageResponse.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using Uhuru.Utilities;

    public class DeaFindDropletMessageResponse : JsonConvertibleObject
    {
        [JsonName("dea")]
        public string DeaId
        {
            get;
            set;
        }

        [JsonName("version")]
        public string Version
        {
            get;
            set;
        }

        [JsonName("droplet")]
        public int DropletId
        {
            get;
            set;
        }

        [JsonName("instance")]
        public string InstanceId
        {
            get;
            set;
        }

        [JsonName("index")]
        public int Index
        {
            get;
            set;
        }

        /*
        public string StateInterchangeableFormat
        {
            get { return State.ToString(); }
            set { State = (DropletInstanceState)Enum.Parse(typeof(DropletInstanceState), value); }
        }
         */
        [JsonName("state")]
        public DropletInstanceState State
        {
            get;
            set;
        }

        [JsonName("state_timestamp")]
        public int StateTimestampInterchangeableFormat
        {
            get { return RubyCompatibility.DateTimeToEpochSeconds(this.StateTimestamp); }
            set { this.StateTimestamp = RubyCompatibility.DateTimeFromEpochSeconds(value); }
        }

        public DateTime StateTimestamp
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), 
        JsonName("file_uri")]
        public string FileUri
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), 
        JsonName("credentials")]
        public string[] FileAuth
        {
            get;
            set;
        }

        [JsonName("staged")]
        public string Staged
        {
            get;
            set;
        }

        [JsonName("debug_ip")]
        public string DebugIP
        {
            get;
            set;
        }

        [JsonName("debug_port")]
        public int? DebugPort
        {
            get;
            set;
        }

        [JsonName("stats")]
        public DropletStatusMessageResponse Stats
        {
            get;
            set;
        }
    }
}
