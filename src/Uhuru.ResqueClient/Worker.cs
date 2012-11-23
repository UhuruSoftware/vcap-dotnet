using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookSleeve;
using System.Net;
using System.Diagnostics;
using System.Threading;

// -----------------------------------------------------------------------
// <copyright file="$safeitemrootname$.cs" company="$registeredorganization$">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace Uhuru.ResqueClient
{
    public class Worker: IDisposable
    {
        private Resque resque;
        private string[] queues;
        private string id;

        public Worker(string queue)
        {
            this.queues = new string[] { queue };
        }

        public Worker(string[] queues)
        {
            this.queues = queues;
        }

        public Resque Resque
        {
            get
            {
                
                return this.resque;
            }
            set
            {
                this.resque = value;
            }
        }

        public string Id
        {
            get
            {
                if (id == null)
                {
                    string hostname = Dns.GetHostName();
                    int pid = Process.GetCurrentProcess().Id;
                    id = string.Format("{0}:{1}:{2}", hostname, pid.ToString(), string.Join(",", queues));
                }
                return id;
            }
        }

        public bool Shutdown
        {
            get;
            set;
        }

        public Dictionary<string, string> JobClasses
        {
            get;
            set;
        }

        public void Work(int interval)
        {
            this.Register();
            try
            {
                while (true)
                {
                    if (this.Shutdown)
                    {
                        break;
                    }
                    Job job = GetJob();
                    if (job != null)
                    {
                        try
                        {
                            job.Worker = this;
                            this.Resque.WorkingOn(job);
                            job.Perform();
                        }
                        catch (Exception ex)
                        {
                            this.Resque.FailJob(job, ex);
                        }
                        finally
                        {
                            this.Resque.DoneWorkingOn(job);
                        }
                    }
                    else
                    {
                        Thread.Sleep(interval * 1000);
                    }
                }
            }
            finally
            {
                Unregister();
            }
        }

        private void Register()
        {
            resque.RegisterWorker(this.Id);
        }

        private void Unregister()
        {
            resque.UnregisterWorker(this.Id);
        }

        private Job GetJob()
        {
            foreach (string queue in queues)
            {
                Dictionary<string, object> dict = this.resque.Pop(queue);
                if (dict != null)
                {
                    return new Job(queue, dict);
                }
            }
            return null;
        }

        public void Dispose()
        {
            this.Unregister();
        }
    }
}
