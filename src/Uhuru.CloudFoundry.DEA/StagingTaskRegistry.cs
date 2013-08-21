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
        }

        public void Unregister(StagingTask task)
        {
            tasks.Remove(task.TaskId);
        }
    }
}
