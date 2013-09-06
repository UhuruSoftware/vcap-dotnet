// -----------------------------------------------------------------------
// <copyright file="StagingTaskRegistry.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2013 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Uhuru.Utilities;

    class StagingTaskRegistry
    {
        private Dictionary<string, StagingTask> tasks;

        public ICollection<StagingTask> Tasks
        {
            get
            {
                return this.tasks.Values;
            }
        }

        public StagingTaskRegistry()
        {
            tasks = new Dictionary<string, StagingTask>();
        }

        public void Register(StagingTask task) 
        {
            tasks[task.TaskId] = task;
            Logger.Debug("Registered staging task {0}", task.TaskId);
        }

        public void Unregister(StagingTask task)
        {            
            if (tasks.Keys.Contains(task.TaskId))
            {
                tasks.Remove(task.TaskId);
                Logger.Debug("Unregistered staging task {0}", task.TaskId);
            }
            else
            {
                Logger.Error("Could not unregister task {0}. It does not exist.", task.TaskId);
            }
        }
    }
}
