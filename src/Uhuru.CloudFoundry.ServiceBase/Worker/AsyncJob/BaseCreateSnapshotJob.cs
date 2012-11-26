// -----------------------------------------------------------------------
// <copyright file="BaseCreateSnapshotJob.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase.Worker.AsyncJob
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Uhuru.CloudFoundry.ServiceBase.Objects;
    using Uhuru.CloudFoundry.ServiceBase.Worker.Objects;
    using System.IO;
    using Newtonsoft.Json;
    using BookSleeve;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class BaseCreateSnapshotJob : SnapshotJob
    {
        public BaseCreateSnapshotJob()
            : base()
        {
            this.snapshotFiles = new List<string>();
        }
        protected abstract SnapshotObject Execute();
        protected abstract void Cancel();
        protected List<string> snapshotFiles;

        public void Perform(CreateSnapshotRequest request)
        {
            try
            {
                this.SnapshotId = NewSnapshotId();
                this.Name = request.ServiceId;
                List<string> snapshotFiles = new List<string>();

                var redisConn = new RedisConnection(config.Worker.Resque.Host, config.Worker.Resque.Port, password: config.Worker.Resque.Password, syncTimeout: config.Worker.Resque.Timeout);

                // TODO: set the Lock TTL from config file.
                using (Lock lck = new Lock(redisConn, Name))
                {
                    lck.OnTtlExpired += new EventHandler(lck_RaiseLockExpired);
                    lck.OnRefreshError += new EventHandler(lck_RaiseLockExpired);

                    int quota = config.Worker.SnapshotQuota;
                    int current = ServiceSnapshotsCount(Name);
                    if (current > quota)
                    {
                        throw new ServiceException(ServiceException.OverQuota, Name, current.ToString(), quota.ToString());
                    }

                    SnapshotObject response = Execute();
                    response.Manifest.Plan = request.Metadata.Plan;
                    response.Manifest.Provider = request.Metadata.Provider;
                    response.Manifest.ServiceVersion = request.Metadata.ServiceVersion;

                    string dumpPath = GetDumpPath(this.Name, this.SnapshotId);
                    if (!Directory.Exists(dumpPath))
                    {
                        Directory.CreateDirectory(dumpPath);
                    }

                    string packageFile = string.Concat(this.SnapshotId.ToString(), ".zip");
                    Package package = new Package(packageFile);
                    package.Manifest = response.Manifest;
                    if (response.Files.Length == 0)
                    {
                        throw new Exception("No snapshot file to package.");
                    }
                    foreach (string file in response.Files)
                    {
                        string fullPath = Path.Combine(dumpPath, file);
                        package.AddFile(fullPath);
                    }

                    package.Pack(dumpPath);

                    response.Files = null;
                    response.File = packageFile;
                    response.Date = this.FormattedTime();
                    response.Name = string.Format("Snapshot {0}", response.Date);

                    this.SaveSnapshot(this.Name, response);

                    string messages = JsonConvert.SerializeObject(response);
                    this.Completed(messages);
                }

            }
            catch (Exception ex)
            {
                this.Cleanup(this.Name, this.SnapshotId);
                this.HandleError(ex);
            }
            finally
            {
                this.SetStatus(new Dictionary<string, object>() { { "complete_time", DateTime.Now.ToString() } });
                foreach (string file in snapshotFiles)
                {
                    File.Delete(file);
                }
            }
        }

        void lck_RaiseLockExpired(object state, EventArgs e)
        {
            this.Cancel();
        }

    }
}
