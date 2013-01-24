// -----------------------------------------------------------------------
// <copyright file="DeaFindDropletMessageRequest.cs" company="Uhuru Software, Inc.">
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
    /// This class encapsulates a request message to find a droplet.
    /// </summary>
    public class DeaFindDropletMessageRequest : JsonConvertibleObject
    {
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
        /// Gets or sets the version of the droplet.
        /// </summary>
        [JsonName("version")]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instance ids of the droplet that we have to find.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "It is used for JSON (de)serialization."),
        JsonName("instance_ids")]
        public HashSet<string> InstanceIds
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the indexes of the instances that we have to find..
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Suitable for the current context"), 
        JsonName("indices")]
        public HashSet<int> Indexes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the desired states of the instances that have to be found.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Suitable for the current context"), 
        JsonName("states")]
        public HashSet<string> StatesInterchangeableFormat
        {
            get 
            {         
                // todo: change this conversion mechanism
                HashSet<string> res = new HashSet<string>();
                foreach (DropletInstanceState state in this.States)
                {
                    res.Add(state.ToString());
                }

                return res;
            }

            set 
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.States = new HashSet<DropletInstanceState>();
                foreach (string state in value)
                {
                    // States.Add((DropletInstanceState)Enum.Parse(typeof(DropletInstanceState), state));
                    this.States.Add(JsonConvertibleObject.ObjectToValue<DropletInstanceState>(state));
                }
            }
        }

        /// <summary>
        /// Gets or sets the states of the instances that have to be found.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Suitable for the current context")]
        public HashSet<DropletInstanceState> States
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include states in the response message.
        /// </summary>
        [JsonName("include_stats")]
        public bool IncludeStates
        {
            get;
            set;
        }
    }
}
