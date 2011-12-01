using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.DEA
{
    [Serializable]
    class Droplet
    {
        private int dropletId;

        public int DropletId
        {
            get { return dropletId; }
            set { dropletId = value; }
        }

        public Droplet(int dropletId)
        {
            this.dropletId = dropletId;
            Instances = new DropletInstanceCollection();
        }

        public int Count
        {
            get
            {
                return Instances.Count;
            }
        }

        public DropletInstanceCollection Instances;
    }
}
