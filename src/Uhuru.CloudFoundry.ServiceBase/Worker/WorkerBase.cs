// -----------------------------------------------------------------------
// <copyright file="Worker.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase.Worker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Uhuru.Configuration;
    using Uhuru.ResqueClient;
    using BookSleeve;
using System.Reflection;
    using System.Threading;


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class WorkerBase
    {
        private Worker worker;

        public virtual void Start(ServiceElement options)
        {
            dynamic queues = options.NodeId;
            if (!string.IsNullOrEmpty(options.Worker.Queues))
            {
                queues = options.Worker.Queues.Split(new char[] { ',' });
            }

            int interval = options.Worker.Interval;
            worker = new Worker(queues);
            RedisConnection connection = new RedisConnection(
                options.Worker.Resque.Host, 
                options.Worker.Resque.Port, 
                options.Worker.Resque.Timeout, 
                options.Worker.Resque.Password);
            worker.Resque = new Resque(connection);

            Dictionary<string, string> jobs = new Dictionary<string, string>();
            jobs.Add("VCAP::Services::Mssql::Snapshot::CreateSnapshotJob", "Uhuru.CloudFoundry.MSSqlService.Job.CreateSnapshotJob, Uhuru.CloudFoundry.MSSqlService, Version=0.9.0.0, Culture=neutral, PublicKeyToken=ea50a53aba7aa798");
            worker.JobClasses = jobs;
            new Thread(delegate()
                {
                    worker.Work(interval);
                }).Start();
        }

        public virtual void Stop()
        {
            worker.Shutdown = true;
        }
    }
}
