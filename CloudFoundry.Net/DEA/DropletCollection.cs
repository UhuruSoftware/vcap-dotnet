using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Uhuru.Utilities;

namespace CloudFoundry.Net.DEA
{
    class DropletCollection : IEnumerable<Droplet>
    {
        private readonly object collectionLock = new object();

        private Dictionary<int, Droplet> droplets;

        public DropletCollection()
        {
            droplets = new Dictionary<int, Droplet>();
        }

        public int Count
        {
            get
            {
                lock (collectionLock)
                {
                    return droplets.Count;
                }
            }
        }
        public void Add(Droplet droplet)
        {
            lock (collectionLock)
            {
                droplets[droplet.DropletId] = droplet;
            }
        }

        public IEnumerator<Droplet> GetEnumerator()
        {
            lock (collectionLock)
            {
                List<Droplet> clonedDroplets = new List<Droplet>(this.Count);
                foreach (Droplet droplet in droplets.Values)
                {
                    clonedDroplets.Add(droplet);
                }

                return clonedDroplets.GetEnumerator();
            }
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (collectionLock)
            {
                List<Droplet> clonedDroplets = new List<Droplet>(this.Count);
                foreach (Droplet droplet in droplets.Values)
                {
                    clonedDroplets.Add(droplet);
                }

                return clonedDroplets.GetEnumerator();
            }
        }

        public string ToJson()
        {
            lock (collectionLock)
            {

                Dictionary<int, Dictionary<string, Dictionary<string, object>>> jObject = new Dictionary<int, Dictionary<string, Dictionary<string, object>>>();

                foreach (Droplet droplet in droplets.Values)
                {
                    Dictionary<string, Dictionary<string, object>> instances = new Dictionary<string,Dictionary<string,object>>();

                    jObject.Add(droplet.DropletId, instances);

                    foreach (DropletInstance instance in droplet.Instances)
                    {
                        instances.Add(instance.InstanceId, instance.ToDictionary());
                    }
                }

                return jObject.ToJson();
            }
        }

        public Droplet this[int dropletId]
        {
            get
            {
                lock (collectionLock)
                {
                    return droplets[dropletId];
                }
            }
        }

        public bool DropletExists(int dropletId)
        {
            lock (collectionLock)
            {
                return droplets.ContainsKey(dropletId);
            }
        }

        public void Delete(int dropletId)
        {
            lock (collectionLock)
            {
                droplets.Remove(dropletId);
            }
        }


    }
}
