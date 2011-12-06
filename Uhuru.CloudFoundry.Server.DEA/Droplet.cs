// -----------------------------------------------------------------------
// <copyright file="Droplet.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System.Collections.Generic;
    
    public class Droplet
    {
        // InstanceId -> DropletInstance
        private Dictionary<string, DropletInstance> dropletInstances = new Dictionary<string,DropletInstance>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public Dictionary<string, DropletInstance> DropletInstances
        {
            get
            {
                return dropletInstances;
            }

            set
            {
                dropletInstances = value;
            }
        }
    }
}
