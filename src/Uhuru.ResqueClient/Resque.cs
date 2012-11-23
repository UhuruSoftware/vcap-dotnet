using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookSleeve;
using Newtonsoft.Json;
using System.Globalization;

// -----------------------------------------------------------------------
// <copyright file="$safeitemrootname$.cs" company="$registeredorganization$">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace Uhuru.ResqueClient
{
    public class Resque : IDisposable
    {
        private RedisConnection redisConnection;

        public Resque(RedisConnection connection)
        {
            this.redisConnection = connection;
        }

        public RedisConnection RedisConnection
        {
            get
            {
                if (this.redisConnection.State != RedisConnectionBase.ConnectionState.Open && 
                    this.redisConnection.State != RedisConnectionBase.ConnectionState.Opening)
                {
                    this.redisConnection.Open();
                }
                return this.redisConnection;
            }
        }

        public Dictionary<string, object> Pop(string queue)
        {
            byte[] result = RedisConnection.Lists.RemoveFirst(0, string.Format(Keys.PopQueue, queue)).Result;
            if (result == null)
            {
                return null;
            }
            return DeserializeObject(Encoding.UTF8.GetString(result));
        }

        public static Dictionary<string, object> DeserializeObject(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }

        public void RegisterWorker(string workerId)
        {
            RedisConnection.Sets.Add(0, Keys.RegisterWorker, workerId);
            RedisConnection.Strings.Set(0, string.Format(Keys.WorkerIdStarted, workerId), Encoding.UTF8.GetBytes(DateTime.Now.ToString()), true);
        }

        public void UnregisterWorker(string workerId)
        {
            RedisConnection.Sets.Remove(0, Keys.RegisterWorker, workerId);
            RedisConnection.Keys.Remove(0, string.Format(Keys.WorkerId, workerId));
            RedisConnection.Keys.Remove(0, string.Format(Keys.WorkerIdStarted, workerId));
            ClearStat(string.Format(Keys.StatProcessedWorkerId, workerId));
            ClearStat(string.Format(Keys.StatFailedWorkerId, workerId));
        }

        public void WorkingOn(Job job)
        {
            Dictionary<string, object> workingOn = new Dictionary<string, object>();
            workingOn["queue"] = job.Queue;
            workingOn["run_at"] = DateTime.Now.ToString();
            workingOn["payload"] = job.Payload;

            this.RedisConnection.Strings.Set(0, string.Format(Keys.WorkerId, job.Worker.Id), Encoding.UTF8.GetBytes(SerializeObject(workingOn)));
        }

        public string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public void ClearStat(string stat)
        {
            RedisConnection.Keys.Remove(0, stat);
        }

        public void IncreaseStat(string stat)
        {
            RedisConnection.Strings.Increment(0, stat, 1);
        }

        public void DecreaseStat(string stat)
        {
            RedisConnection.Strings.Decrement(0, stat, 1);
        }

        public void Dispose()
        {
            this.RedisConnection.Close(true);
        }

        public void DoneWorkingOn(Job job)
        {
            this.IncreaseStat(Keys.StatProcessed);
            this.IncreaseStat(string.Format(Keys.StatProcessedWorkerId, job.Worker.Id));
            this.RedisConnection.Keys.Remove(0, string.Format(Keys.WorkerId, job.Worker.Id));
        }

        public void FailJob(Job job, Exception exception)
        {
            Exception failure = exception.InnerException ?? exception;

            Dictionary<string, object> failureResponse = new Dictionary<string, object>();
            failureResponse["failed_at"] = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss zzz");
            failureResponse["payload"] = job.Payload;
            failureResponse["exception"] = failure.GetType().Name;
            failureResponse["error"] = failure.Message;
            failureResponse["backtrace"] = failure.StackTrace;
            failureResponse["worker"] = job.Worker.Id;
            failureResponse["queue"] = job.Queue;

            this.RedisConnection.Lists.AddLast(0,Keys.Failure , this.SerializeObject(failureResponse));

            this.IncreaseStat(Keys.StatFailed);
            this.IncreaseStat(string.Format(Keys.StatFailedWorkerId, job.Worker.Id));
        }
    }
}
