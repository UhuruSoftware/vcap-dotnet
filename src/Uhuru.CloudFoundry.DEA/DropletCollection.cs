// -----------------------------------------------------------------------
// <copyright file="DropletCollection.cs" company="Uhuru Software, Inc.">
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
    using Uhuru.CloudFoundry.DEA.Messages;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// Callback used to iterate all droplet instances.
    /// </summary>
    /// <param name="instance">The instance.</param>
    public delegate void ForEachCallback(DropletInstance instance);

    /// <summary>
    /// The collection of droplets.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Suitable for this context.")]
    public class DropletCollection : IDisposable
    {
        /// <summary>
        /// Where the droplet collection is stored, keyed with the droplet ID.
        /// DropletId -> Droplet
        /// </summary>
        private Dictionary<int, Droplet> droplets = new Dictionary<int, Droplet>();

        /// <summary>
        /// The collection's lock.
        /// </summary>
        private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// If the snapshot has been scheduled to avoid saving it two times with the same value.
        /// </summary>
        private volatile bool snapshotScheduled;

        /// <summary>
        /// Gets or sets the collection members, organized by IDs.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Suitable for this context.")]
        public Dictionary<int, Droplet> Droplets
        {
            get
            {
                return this.droplets;
            }

            set
            {
                this.droplets = value;
            }
        }

        /// <summary>
        /// Gets or sets the lock.
        /// </summary>
        public ReaderWriterLockSlim Lock
        {
            get
            {
                return this.readerWriterLock;
            }

            set
            {
                this.readerWriterLock = value;
            }
        }

        /// <summary>
        /// Gets or sets the app state file. This is where the recovery is made from.
        /// </summary>
        public string AppStateFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the droplets have been recovered so that there a snapshot is not made only after the recovery.
        /// </summary>
        public bool RecoveredDroplets
        {
            get;
            set;
        }

        /// <summary>
        /// Generates the heartbeat message.
        /// </summary>
        /// <returns>Return the heartbeat message</returns>
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

        /// <summary>
        /// Checks if there are applications to me monitored.
        /// </summary>
        /// <returns>Return true if there are no applications to be monitored.</returns>
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

        /// <summary>
        /// Iterates through all the droplet instances.
        /// </summary>
        /// <param name="upgradableReadLock">Set it to true if a write lock on the Droplet Collection.</param>
        /// <param name="doThat">The code to execute on each instance.</param>
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
                    this.Lock.EnterReadLock();
                    foreach (KeyValuePair<int, Droplet> instances in this.Droplets)
                    {
                        foreach (KeyValuePair<string, DropletInstance> instance in instances.Value.DropletInstances)
                        {
                            ephemeralInstances.Add(instance.Value);
                        }
                    }
                }
                finally
                {
                    this.Lock.ExitReadLock();
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
                    this.Lock.EnterReadLock();
                    foreach (KeyValuePair<int, Droplet> instances in this.Droplets)
                    {
                        foreach (KeyValuePair<string, DropletInstance> instance in instances.Value.DropletInstances)
                        {
                            doThat(instance.Value);
                        }
                    }
                }
                finally
                {
                    this.Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// For each with upgradableReadLock set to false.
        /// </summary>
        /// <param name="doThat">The callback for each instance.</param>
        public void ForEach(ForEachCallback doThat)
        {
            this.ForEach(false, doThat);
        }

        /// <summary>
        /// Snapshots the state of the applications.
        /// </summary>
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

            string tmpFilename = Path.Combine(
                Path.GetDirectoryName(this.AppStateFile),
                string.Format(CultureInfo.InvariantCulture, Strings.SnapshotTemplate, new Guid().ToString()));

            File.WriteAllText(tmpFilename, appState);

            if (File.Exists(this.AppStateFile))
            {
                File.Delete(this.AppStateFile);
            }

            File.Move(tmpFilename, this.AppStateFile);

            Logger.Debug(Strings.TookXSecondsToSnapshotApplication, DateTime.Now - start);

            this.snapshotScheduled = false;
        }

        /// <summary>
        /// Schedules snapshot application state.
        /// </summary>
        public void ScheduleSnapshotAppState()
        {
            if (!this.snapshotScheduled)
            {
                this.snapshotScheduled = true;
                ThreadPool.QueueUserWorkItem(delegate(object data)
                {
                    this.SnapshotAppState();
                    this.snapshotScheduled = false;
                });
            }
        }

        /// <summary>
        /// Removes a droplet instance from the collection.
        /// </summary>
        /// <param name="instance">The droplet instance to remove.</param>
        public void RemoveDropletInstance(DropletInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            try
            {
                this.Lock.EnterWriteLock();

                if (this.Droplets.ContainsKey(instance.Properties.DropletId))
                {
                    Droplet droplet = this.Droplets[instance.Properties.DropletId];
                    if (droplet.DropletInstances.ContainsKey(instance.Properties.InstanceId))
                    {
                        droplet.DropletInstances.Remove(instance.Properties.InstanceId);
                        if (droplet.DropletInstances.Count == 0)
                        {
                            this.Droplets.Remove(instance.Properties.DropletId);
                        }
                    }
                }
            }
            finally
            {
                this.Lock.ExitWriteLock();
            }

            this.ScheduleSnapshotAppState();
        }

        /// <summary>
        /// Adds a droplet instance to the collection.
        /// </summary>
        /// <param name="instance">The droplet instance to add.</param>
        public void AddDropletInstance(DropletInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            try
            {
                this.Lock.EnterWriteLock();
                instance.Lock.EnterReadLock();

                if (!this.Droplets.ContainsKey(instance.Properties.DropletId))
                {
                    this.Droplets.Add(instance.Properties.DropletId, new Droplet());
                }

                this.Droplets[instance.Properties.DropletId].DropletInstances[instance.Properties.InstanceId] = instance;
            }
            finally
            {
                instance.Lock.ExitReadLock();
                this.Lock.ExitWriteLock();
            }

            this.ScheduleSnapshotAppState();
        }

        /// <summary>
        /// Creates a new droplet instance.
        /// </summary>
        /// <param name="message">The NATS message</param>
        /// <returns>The DropletInstance generated.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object is properly disposed on failure.")]
        public DropletInstance CreateDropletInstance(DeaStartMessageRequest message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            DropletInstance instance = null;

            instance = new DropletInstance();

            string instanceId = Guid.NewGuid().ToString("N");

            instance.Properties.State = DropletInstanceState.Starting;
            instance.Properties.StateTimestamp = DateTime.Now;
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
            instance.Properties.LoggingId = string.Format(CultureInfo.InvariantCulture, Strings.NameAppIdInstance, message.Name, message.DropletId, instanceId, message.Index);

            this.AddDropletInstance(instance);

            return instance;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.readerWriterLock != null)
                {
                    this.readerWriterLock.Dispose();
                }
            }
        }
    }
}
