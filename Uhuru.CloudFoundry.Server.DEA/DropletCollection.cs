// -----------------------------------------------------------------------
// <copyright file="DropletCollection.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using Uhuru.Utilities;
    
    public delegate void ForEachCallback(DropletInstance instance);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class DropletCollection
    {
        //DropletId -> Droplet
        private Dictionary<int, Droplet> droplets = new Dictionary<int,Droplet>();
        private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public Dictionary<int, Droplet> Droplets
        {
            get
            {
                return droplets;
            }

            set
            {
                droplets = value;
            }
        }

        public ReaderWriterLockSlim Lock
        {
            get
            {
                return readerWriterLock;
            }

            set
            {
                readerWriterLock = value;
            }
        }

        public string AppStateFile { get; set; }

        public bool RecoveredDroplets
        {
            get;
            set;
        }

        private volatile bool SnapshotScheduled;

        public HeartbeatMessage GenerateHeartbeatMessage()
        {
            HeartbeatMessage response = new HeartbeatMessage();

            ForEach(delegate(DropletInstance instance)
            {
                if (instance.Properties.State != DropletInstanceState.Stopped)
                {
                    response.Droplets.Add(instance.GenerateInstanceHeartbeat().ToJsonIntermediateObject());
                }
            });

            return response;
        }

        public bool NoMonitorableApps()
        {
            bool result = true;
            ForEach(delegate(DropletInstance instance)
            {
                if (instance.Properties.State == DropletInstanceState.Starting || instance.Properties.State == DropletInstanceState.Running)
                {
                    result = false;
                }
            });
            return result;
        }

        public void ForEach(bool upgradableReadLock, ForEachCallback doThat)
        {
            if (doThat == null)
            {
                throw new ArgumentNullException("doThat");
            }
            if (upgradableReadLock)
            {
                List<DropletInstance> ephemeralInstances = new List<DropletInstance>();

                try
                {
                    Lock.EnterReadLock();
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
                    Lock.ExitReadLock();
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

        public void ForEach(ForEachCallback doThat)
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

            string tmpFilename = Path.Combine(Path.GetDirectoryName(AppStateFile), 
                String.Format(CultureInfo.InvariantCulture, Strings.SnapshotTemplate, start.Ticks));

            File.WriteAllText(tmpFilename, appState);

            if (File.Exists(AppStateFile))
                File.Delete(AppStateFile);

            File.Move(tmpFilename, AppStateFile);

            Logger.Debug(Strings.TookXSecondsToSnapshotApplication, DateTime.Now - start);

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
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

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

            ScheduleSnapshotAppState();
        }
       
        public void AddDropletInstance(DropletInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
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

            ScheduleSnapshotAppState();
        }

        public DropletInstance CreateDropletInstance(DeaStartMessageRequest message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            DropletInstance instance = new DropletInstance();
            
            //stefi: consider changing the format
            string instanceId = Guid.NewGuid().ToString();
            
            instance.Properties.State = DropletInstanceState.Starting;
            instance.Properties.Start = DateTime.Now;

            instance.Properties.InstanceId = instanceId;

            instance.Properties.DropletId = message.DropletId;
            instance.Properties.InstanceIndex = message.Index;
            instance.Properties.Name = message.Name;
            instance.Properties.Uris = message.Uris;
            instance.Properties.Users = message.Users;
            instance.Properties.Version = message.Version;
            instance.Properties.Framework = message.Framework;
            instance.Properties.Runtime = message.Runtime;
            instance.Properties.LoggingId = String.Format(CultureInfo.InvariantCulture, Strings.NameAppIdInstance, message.Name, message.DropletId, instanceId, message.Index);
            instance.Properties.WindowsPassword = Credentials.GenerateCredential();
            instance.Properties.WindowsUsername = WindowsVcapUsers.CreateUser(instanceId, instance.Properties.WindowsPassword);

            AddDropletInstance(instance);
            
            return instance;
        }
    }
}
