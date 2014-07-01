// -----------------------------------------------------------------------
// <copyright file="StagingTaskRegistry.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2013 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Uhuru.CloudFoundry.DEA.Messages;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    class StagingRegistry : IDisposable
    {
        private Dictionary<string, StagingInstance> tasks;
        public delegate void ForEachCallback(StagingInstance instance);

        /// <summary>
        /// The collection's lock.
        /// </summary>
        private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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
        /// Gets or sets the staging state file. This is where the recovery is made from.
        /// </summary>
        public string StagingStateFile { get; set; }

        public Dictionary<string, StagingInstance> Tasks
        {
            get
            {
                return this.tasks;
            }
            set
            {
                this.tasks = value;
            }
        }

        private volatile bool snapshotScheduled;

        public StagingRegistry()
        {
            tasks = new Dictionary<string, StagingInstance>();
        }

        public void ForEach(bool upgradableReadLock, ForEachCallback doThat)
        {
            if (doThat == null)
            {
                throw new ArgumentNullException("doThat");
            }

            if (upgradableReadLock)
            {
                List<StagingInstance> ephemeralInstances = new List<StagingInstance>();

                try
                {
                    this.Lock.EnterReadLock();
                    foreach (KeyValuePair<string, StagingInstance> instance in this.Tasks)
                    {
                        ephemeralInstances.Add(instance.Value);
                    }
                }
                finally
                {
                    this.Lock.ExitReadLock();
                }

                foreach (StagingInstance instance in ephemeralInstances)
                {
                    doThat(instance);
                }
            }
            else
            {
                try
                {
                    this.Lock.EnterReadLock();
                    foreach (KeyValuePair<string, StagingInstance> instance in this.Tasks)
                    {
                        doThat(instance.Value);
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

        public void RemoveStagingInstance(StagingInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            try
            {
                this.Lock.EnterWriteLock();

                if (this.Tasks.ContainsKey(instance.Properties.TaskId))
                {
                    this.Tasks.Remove(instance.Properties.TaskId);                    
                }
            }
            finally
            {
                this.Lock.ExitWriteLock();
            }

            this.ScheduleSnapshotStagingState();
        }

        public void SnapshotStagingState()
        {
            List<object> instances = new List<object>();

            ForEach(delegate(StagingInstance instance)
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

            File.WriteAllText(this.StagingStateFile, appState);

            Logger.Debug(Strings.TookXSecondsToSnapshotApplication, DateTime.Now - start);

            this.snapshotScheduled = false;
        }

        public void ScheduleSnapshotStagingState()
        {
            if (!this.snapshotScheduled)
            {
                this.snapshotScheduled = true;
                ThreadPool.QueueUserWorkItem(delegate(object data)
                {
                    this.SnapshotStagingState();
                    this.snapshotScheduled = false;
                });
            }
        }

        public void AddStagingInstance(StagingInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            try
            {
                this.Lock.EnterWriteLock();
                instance.Lock.EnterReadLock();

                if (!this.Tasks.ContainsKey(instance.Properties.TaskId))
                {
                    this.Tasks.Add(instance.Properties.TaskId, instance);
                }
            }
            finally
            {
                instance.Lock.ExitReadLock();
                this.Lock.ExitWriteLock();
            }

            this.ScheduleSnapshotStagingState();
        }

        public StagingInstance CreateStagingInstance(StagingStartMessageRequest message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            StagingInstance instance = new StagingInstance();

            string instanceId = Credentials.GenerateSecureGuid().ToString("N");
            string privateInstanceId = Credentials.GenerateSecureGuid().ToString("N") + Credentials.GenerateSecureGuid().ToString("N");

            instance.StartMessage = message.StartMessage;

            instance.Properties.InstanceId = instanceId;
            instance.Properties.TaskId = message.TaskID;
            instance.Properties.AppId = message.AppID;
            instance.Properties.BuildpackCacheDownloadURI = message.BuildpackCacheDownloadURI;
            instance.Properties.BuildpackCacheUploadURI = message.BuildpackCacheUploadURI;
            instance.Properties.DownloadURI = message.DownloadURI;
            instance.Properties.UploadURI = message.UploadURI;

            if (message.Properties.Meta != null)
            {
                if (message.Properties.Meta.Command != null)
                {
                    instance.Properties.MetaCommand = message.Properties.Meta.Command;
                }
            }

            this.AddStagingInstance(instance);

            return instance;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
