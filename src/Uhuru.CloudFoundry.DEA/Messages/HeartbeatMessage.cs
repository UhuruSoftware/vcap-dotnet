// -----------------------------------------------------------------------
// <copyright file="HeartbeatMessage.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using System;
    using System.Collections.Generic;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// A class that encapsulates a heartbeat message.
    /// </summary>
    public class HeartbeatMessage : JsonConvertibleObject
    {
        /// <summary>
        /// All the droplets in the DEA.
        /// </summary>
        private List<Dictionary<string, object>> droplets = new List<Dictionary<string, object>>();

        /// <summary>
        /// Gets or sets all the droplets hosted in the DEA.
        /// TODO: stefi: change the type when json helper class can go deep into generic collections
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "It is used for JSON (de)serialization."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "It is used for JSON (de)serialization."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "It is used for JSON (de)serialization."),
        JsonName("droplets")]
        public List<Dictionary<string, object>> Droplets
        {
            get
            {
                return this.droplets;
            }

            set
            {
                this.droplets = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the Dea id.
        /// </summary>
        [JsonName("dea")]
        public string Dea
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether it accepts only production apps.
        /// </summary>
        [JsonName("prod")]
        public bool Prod
        {
            get;
            set;
        }

        /// <summary>
        /// A class that encapsulates a message containing a set of droplet instance properties.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Code is cleaner this way.")]
        public class InstanceHeartbeat : JsonConvertibleObject
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
            /// Gets or sets the version of the droplet.
            /// </summary>
            [JsonName("version")]
            public string Version
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the id of the current instance.
            /// </summary>
            [JsonName("instance")]
            public string InstanceId
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the index of the current instance.
            /// </summary>
            [JsonName("index")]
            public int InstanceIndex
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
            /// Gets or sets the timestamp corresponding to the moment the state was read, in interchangeable format.
            /// </summary>
            [JsonName("state_timestamp")]
            public int StateTimestampInterchangeableFormat
            {
                get { return RubyCompatibility.DateTimeToEpochSeconds(this.StateTimestamp); }
                set { this.StateTimestamp = RubyCompatibility.DateTimeFromEpochSeconds(value); }
            }

            /// <summary>
            /// Gets or sets the timestamp corresponding to the moment the state was read.
            /// </summary>
            public DateTime StateTimestamp
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the cloud controller partition.
            /// </summary>
            [JsonName("cc_partition")]
            public string CloudControllerPartition
            {
                get;
                set;
            }
        }
    }
}
