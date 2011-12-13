// -----------------------------------------------------------------------
// <copyright file="Droplet.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System.Collections.Generic;

    /// <summary>
    /// The droplet contains a collection of instances.
    /// </summary>
    public class Droplet
    {
        /// <summary>
        /// The hash table of droplet instances. The Instance ID key points to the Instance.
        /// </summary>
        private Dictionary<string, DropletInstance> dropletInstances = new Dictionary<string, DropletInstance>();

        /// <summary>
        /// Gets or sets the droplet instances.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Suitable for this context.")]
        public Dictionary<string, DropletInstance> DropletInstances
        {
            get
            {
                return this.dropletInstances;
            }

            set
            {
                this.dropletInstances = value;
            }
        }
    }
}
