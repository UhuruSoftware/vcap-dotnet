using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;
using System.IO;
using System.Threading;

namespace Uhuru.CloudFoundry.DEA
{
    public class DropletCollection
    {

        //DropletId -> Droplet
        public Dictionary<int, Droplet> Droplets = new Dictionary<int,Droplet>();
        public ReaderWriterLockSlim Lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public string AppStateFile { get; set; }

        public bool RecoverdDroplets
        {
            get;
            set;
        }

        private volatile bool SnapshotScheduled;

        public HearbeatMessage GenerateHearbeatMessage()
        {
            HearbeatMessage response = new HearbeatMessage();

            ForEach(delegate(DropletInstance instance)
            {
                response.Droplets.Add(instance.GenerateInstanceHearbeat().ToJsonIntermediateObject());
            });

            return response;
        }


        public bool NoMonitorableApps()
        {
            bool result = true;
            ForEach(delegate(DropletInstance instance)
            {
                if (instance.Properties.State == DropletInstanceState.STARTING || instance.Properties.State == DropletInstanceState.RUNNING)
                {
                    result = false;
                }
            });
            return result;

        }

        public delegate void ForEachDelegate(DropletInstance instance);
        public void ForEach(bool UpgradableReadLock, ForEachDelegate doThat)
        {
            if (UpgradableReadLock)
            {
                List<DropletInstance> ephemeralInstances = new List<DropletInstance>();

                try
                {
                    Lock.EnterUpgradeableReadLock();
                    foreach (KeyValuePair<int, Droplet> instances in Droplets)
                    {
                        foreach (KeyValuePair<string, DropletInstance> instance in instances.Value.DropletInstances)
                        {
                            ephemeralInstances.Add(instance.Value);
                        }
                    }

                }
                finally
                {
                    Lock.ExitUpgradeableReadLock();
                }

                foreach (DropletInstance instance in ephemeralInstances)
                {
                    doThat(instance);
                }

            }
            else
            {
                try
                {
                    Lock.EnterReadLock();
                    foreach (KeyValuePair<int, Droplet> instances in Droplets)
                    {
                        foreach (KeyValuePair<string, DropletInstance> instance in instances.Value.DropletInstances)
                        {
                            doThat(instance.Value);
                        }
                    }
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
            
        }

        public void ForEach(ForEachDelegate doThat)
        {
            ForEach(false, doThat);
        }
   

        public void SnapshotAppState()
        {
            
            List<object> instances = new List<object>();

            ForEach(delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterReadLock();
                    instances.Add(instance.Properties.ToJsonIntermediateObject());
                }
                finally
                {
                    instance.Lock.ExitReadLock();
                }

            });

            string appState = JsonConvertibleObject.SerializeToJson(instances);
            

            DateTime start = DateTime.Now;

            string tmpFilename = Path.Combine(Path.GetDirectoryName(AppStateFile), String.Format("snap_{0}", start.Ticks));

            File.WriteAllText(tmpFilename, appState);

            if (File.Exists(AppStateFile))
                File.Delete(AppStateFile);

            File.Move(tmpFilename, AppStateFile);

            Logger.debug(String.Format("Took {0} to snapshot application state.", DateTime.Now - start));

            SnapshotScheduled = false;
        }

        

        public void ScheduleSnapshotAppState()
        {
            if (!SnapshotScheduled)
            {
                SnapshotScheduled = true;
                ThreadPool.QueueUserWorkItem(delegate(object data)
                {
                    SnapshotAppState();
                    SnapshotScheduled = false;
                });
            }
        }


        public void RemoveDropletInstance(DropletInstance instance)
        {
            try
            {
                Lock.EnterWriteLock();

                if (Droplets.ContainsKey(instance.Properties.DropletId))
                {
                    Droplet droplet = Droplets[instance.Properties.DropletId];
                    if (droplet.DropletInstances.ContainsKey(instance.Properties.InstanceId))
                    {
                        droplet.DropletInstances.Remove(instance.Properties.InstanceId);
                        if (droplet.DropletInstances.Count == 0)
                        {
                            Droplets.Remove(instance.Properties.DropletId);
                        }
                    }
                }
            }
            finally
            {
                Lock.ExitWriteLock();
            }

        }

        

        public void AddDropletInstance(DropletInstance instance)
        {
            try
            {
                Lock.EnterWriteLock();
                instance.Lock.EnterReadLock();

                if (!Droplets.ContainsKey(instance.Properties.DropletId))
                {
                    Droplets.Add(instance.Properties.DropletId, new Droplet());
                }
                Droplets[instance.Properties.DropletId].DropletInstances[instance.Properties.InstanceId] = instance;
            }
            finally
            {
                instance.Lock.ExitReadLock();
                Lock.ExitWriteLock();
            }
        }

        public DropletInstance CreateDropletInstance(DeaStartMessageRequest pmessage)
        {
            DropletInstance instance = new DropletInstance();
            
            //stefi: consider changing the format
            string instanceId = Guid.NewGuid().ToString();


            instance.Properties.State = DropletInstanceState.STARTING;
            instance.Properties.Start = DateTime.Now;

            instance.Properties.InstanceId = instanceId;

            instance.Properties.DropletId = pmessage.DropletId;
            instance.Properties.InstanceIndex = pmessage.Index;
            instance.Properties.Name = pmessage.Name;
            instance.Properties.Uris = pmessage.Uris;
            instance.Properties.Users = pmessage.Users;
            instance.Properties.Version = pmessage.Version;
            instance.Properties.Framework = pmessage.Framework;
            instance.Properties.Runtime = pmessage.Runtime;
            instance.Properties.LoggingId = String.Format("name={0} app_id={1} instance={2} index={3}", pmessage.Name, pmessage.DropletId, instanceId, pmessage.Index);

            AddDropletInstance(instance);
            
            return instance;
        }

    }
}
