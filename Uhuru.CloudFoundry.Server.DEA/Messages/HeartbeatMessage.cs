// -----------------------------------------------------------------------
// <copyright file="HeartbeatMessage.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using Uhuru.Utilities;
    
    public class HeartbeatMessage : JsonConvertibleObject
    {
        /// <summary>
        /// A class containing a set of droplet instance properties.
        /// </summary>
        public class InstanceHeartbeat : JsonConvertibleObject
        {
            /// <summary>
            /// The id of the droplet the instance belongs to.
            /// </summary>
            [JsonName("droplet")]
            public int DropletId
            {
                get;
                set;
            }

            /// <summary>
            /// The version of the droplet.
            /// </summary>
            [JsonName("version")]
            public string Version
            {
                get;
                set;
            }

            /// <summary>
            /// The id of the current instance.
            /// </summary>
            [JsonName("instance")]
            public string InstanceId
            {
                get;
                set;
            }

            /// <summary>
            /// The index of the current instance.
            /// </summary>
            [JsonName("index")]
            public int InstanceIndex
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

            /// <summary>
            /// The state of the droplet instance.
            /// </summary>
            [JsonName("state")]
            public DropletInstanceState State
            {
                get;
                set;
            }

            /// <summary>
            /// The timestamp corresponding to the moment the state was read, in interchangeable format.
            /// </summary>
            [JsonName("state_timestamp")]
            public int StateTimestampInterchangeableFormat
            {
                get { return RubyCompatibility.DateTimeToEpochSeconds(this.StateTimestamp); }
                set { this.StateTimestamp = RubyCompatibility.DateTimeFromEpochSeconds(value); }
            }

            /// <summary>
            /// The timestamp corresponding to the moment the state was read.
            /// </summary>
            public DateTime StateTimestamp
            {
                get;
                set;
            }
        }

        // todo: stefi: change the type when json helper class can go deep into generic collections
        [JsonName("droplets")]
        public List<Dictionary<string, object>> Droplets = new List<Dictionary<string, object>>();
    }
}
