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
        // InstanceId -> DropletInstance
        private Dictionary<string, DropletInstance> dropletInstances = new Dictionary<string, DropletInstance>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
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
