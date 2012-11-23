// -----------------------------------------------------------------------
// <copyright file="SnapshotJob.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase.Worker.AsyncJob
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Uhuru.Configuration;
    using System.Configuration;
    using Newtonsoft.Json;
    using System.IO;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SnapshotJob : Snapshot
    {
        public string Name { get; set; }
        public long SnapshotId { get; set; }
        public ServiceElement config { get; set; }
        protected string JobId { get; set; }
        protected string status;

        public SnapshotJob()
            : base()
        {
            config = ((UhuruSection)ConfigurationManager.GetSection("uhuru")).Service;
            Config.RedisConfig = config.Worker.Resque;
            Config.TempFolder = config.Worker.TempDir;
            Snapshot.RedisConnect(Config.RedisConfig);
            Snapshot.RedisInit();
        }

        protected void SetStatus(Dictionary<string, object> status)
        {
            this.SetStatus(status, this.status);
        }

        protected void SetStatus(Dictionary<string, object> status, string messages)
        {
            Dictionary<string, object> currentStatus = JsonConvert.DeserializeObject<Dictionary<string, object>>(messages);
            foreach (KeyValuePair<string, object> pair in status)
            {
                currentStatus[pair.Key] = pair.Value;
            }
            currentStatus["name"] = this.Name;
            this.status = JsonConvert.SerializeObject(currentStatus);
            
            this.Client.Strings.Set(0, "resque:status:" + this.JobId, Encoding.UTF8.GetBytes(this.status));
        }

        protected void Completed(string messages)
        {
            this.SetStatus(new Dictionary<string, object>() { { "status", "completed" }, { "message", "Completed at " + DateTime.Now.ToString() } }, messages);
        }

        protected void Failed(string messages)
        {
            this.SetStatus(new Dictionary<string, object>() { { "status", "failed" } }, messages);
        }

        public Lock CreateLock()
        {
            string lockName = "lock:lifecycle:" + this.Name;

            throw new NotImplementedException();
        }

        protected string GetDumpPath(string name, long snapshotId)
        {
            return SnapshotFilePath(config.Worker.SnapshotsBaseDir, config.Worker.ServiceName, name, snapshotId);
        }

        protected void Cleanup(string name, long snapshotId)
        {
            this.DeleteSnapshot(name, snapshotId);
            Directory.Delete(this.GetDumpPath(name, snapshotId), true);
        }

        protected void HandleError(Exception e)
        {
            Exception err = e.GetType().IsAssignableFrom(typeof(ServiceException)) ? e : new ServiceException(ServiceException.InternalError);
            string errorMessage = JsonConvert.SerializeObject(new Dictionary<string, object>() { { "error", err.Message } });
            Failed(errorMessage);
        }
    }
}
