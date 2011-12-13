// -----------------------------------------------------------------------
// <copyright file="DeaStopMessageRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// This class encapsulates a request message to stop droplet instances.
    /// </summary>
    public class DeaStopMessageRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the droplet id to be stopped.
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
        /// Gets or sets the instance IDs that have to be stopped.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), 
        JsonName("instances")]
        public HashSet<string> InstanceIds
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the indexes of the instances that have to be stopped.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), 
        JsonName("indices")]
        public HashSet<int> Indexes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the states of the instances; this property is used for (de)serialization only.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), 
        JsonName("states")]
        public HashSet<string> StatesInterchangeableFormat
        {
            get
            {
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
                    this.States.Add(JsonConvertibleObject.ObjectToValue<DropletInstanceState>(state));
                }
            }
        }

        /// <summary>
        /// Gets or sets the states of the instances.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public HashSet<DropletInstanceState> States
        {
            get;
            set;
        }
    }
}
