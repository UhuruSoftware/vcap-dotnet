using System.Collections.Generic;

namespace Uhuru.CloudFoundry.DEA
{
    public class Droplet
    {
        //InstanceId -> DropletInstance
        
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
