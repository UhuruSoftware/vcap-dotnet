using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Uhuru.Utilities;

namespace CloudFoundry.Net.DEA
{
    [Serializable]
    class DropletInstanceCollection : IEnumerable<DropletInstance>
    {
        private readonly object collectionLock = new object();

        Dictionary<string, DropletInstance> instances;

        public DropletInstanceCollection()
        {
            instances = new Dictionary<string, DropletInstance>();
        }

        public IEnumerator<DropletInstance> GetEnumerator()
        {
            lock (collectionLock)
            {
                List<DropletInstance> clonedInstances = new List<DropletInstance>(this.Count);
                foreach (DropletInstance instance in instances.Values)
                {
                    clonedInstances.Add(instance);
                }

                return clonedInstances.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (collectionLock)
            {
                List<DropletInstance> clonedInstances = new List<DropletInstance>(this.Count);
                foreach (DropletInstance instance in instances.Values)
                {
                    clonedInstances.Add(instance);
                }

                return clonedInstances.GetEnumerator();
            }
        }

        public void Add(DropletInstance instance)
        {
            lock (collectionLock)
            {
                instances[instance.InstanceId] = instance;
            }
        }

        public DropletInstance this[string instanceId]
        {
            get
            {
                lock (collectionLock)
                {
                    return instances[instanceId];
                }
            }
        }

        public bool InstanceExists(string instanceId)
        {
            lock (collectionLock)
            {
                return instances.ContainsKey(instanceId);
            }
        }

        public void Remove(string instanceId)
        {
            lock (collectionLock)
            {
                instances.Remove(instanceId);
            }
        }

        public void Remove(DropletInstance instance)
        {
            instances.Remove(instance.InstanceId);
        }

        public int Count
        {
            get
            {
                lock (collectionLock)
                {
                    return instances.Count;
                }
            }
        }

        internal string ToJson()
        {
            Dictionary<string, object> jInstances = new Dictionary<string, object>();

            lock (collectionLock)
            {
                foreach (string key in instances.Keys)
                {
                    jInstances.Add(key, instances[key].ToDictionary());
                }
            }

            return jInstances.ToJson();
        }
    }
}
