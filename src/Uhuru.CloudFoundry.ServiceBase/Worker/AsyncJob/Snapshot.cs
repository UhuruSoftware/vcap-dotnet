// -----------------------------------------------------------------------
// <copyright file="Snapshot.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase.Worker.AsyncJob
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using BookSleeve;
    using Newtonsoft.Json;
    using Uhuru.CloudFoundry.ServiceBase.Objects;
    using Uhuru.Configuration;
    using Uhuru.Utilities.Json;

    public class Snapshot : IDisposable
    {
        protected static RedisConnection redis;


        public RedisConnection Client
        {
            get 
            {
                if (redis.State != RedisConnectionBase.ConnectionState.Open && redis.State != RedisConnectionBase.ConnectionState.Opening)
                {
                    redis.Open();
                }
                return Snapshot.redis; 
            }
        }

        const string SNAPSHOT_KEY_PREFIX = "vcap:snapshot";
        const string SNAPSHOT_ID = "maxid";
        int maxNameLength = 512;

        public static void RedisConnect(ResqueElement options)
        {
            redis = new RedisConnection(options.Host, options.Port, options.Timeout, options.Password);
        }

        /// <summary>
        /// Initialize necessary keys
        /// </summary>
        public static void RedisInit()
        {
            redis.Strings.SetIfNotExists(0, string.Concat(SNAPSHOT_KEY_PREFIX, ":", SNAPSHOT_ID), "1");
        }

        protected void SaveSnapshot(string serviceId, SnapshotObject snapshot)
        {
            long snapshotId = snapshot.SnapshotId;
            string message = JsonConvert.SerializeObject(snapshot);
            this.Client.Hashes.Set(0, this.RedisKey(serviceId), snapshotId.ToString(), Encoding.UTF8.GetBytes(message));
        }

        public string FormattedTime()
        {
            return DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// Get all snapshots related to a service instance
        /// </summary>
        /// <param name="serviceId">The service id.</param>
        /// <returns></returns>
        string ServiceSnapshots(string serviceId)
        {
            if (string.IsNullOrEmpty(serviceId))
            {
                return string.Empty;
            }
            Dictionary<string, byte[]> res = Client.Hashes.GetAll(0, RedisKey(serviceId)).Result;
            return JsonConvert.SerializeObject(res.Values);
        }

        /// <summary>
        /// Return total snapshots count
        /// </summary>
        /// <param name="serviceId">The service id.</param>
        /// <returns></returns>
        protected int ServiceSnapshotsCount(string serviceId)
        {
            if (string.IsNullOrEmpty(serviceId))
            {
                return 0;
            }

            return (int)Client.Hashes.GetLength(0, RedisKey(serviceId)).Result;
        }

        /// <summary>
        /// Get detail information for a single snapshot
        /// </summary>
        /// <param name="serviceId">The service id.</param>
        /// <param name="snaphotId">The snaphot id.</param>
        /// <returns></returns>
        SnapshotObject SnapshotDetails(string serviceId, int snaphotId)
        {
            byte[] res = Client.Hashes.Get(0, RedisKey(serviceId), snaphotId.ToString()).Result;
            return JsonConvert.DeserializeObject<SnapshotObject>(Encoding.UTF8.GetString(res));
        }

        /// <summary>
        /// Generate a new unique id for a snapshot
        /// </summary>
        /// <returns></returns>
        protected long NewSnapshotId()
        {
            return Client.Strings.Increment(0, RedisKey(SNAPSHOT_ID)).Result;
        }

        protected string SnapshotFilePath(string baseDir, string serviceName, string serviceId, long snapshotId)
        {
            return Path.Combine(baseDir, "snapshots", serviceName, serviceId.Substring(0, 2), serviceId.Substring(2, 2), serviceId.Substring(4, 2), serviceId, snapshotId.ToString());
        }

        bool UpdateName(string serviceId, int snapshotId, string name)
        {
            VerifyInputName(name);

            string key = RedisKey(serviceId);

            SnapshotObject snapshot = JsonConvertibleObject.ObjectToValue<SnapshotObject>(Client.Hashes.Get(0, key, snapshotId.ToString()).Result);
            snapshot.Name = name;

            SaveSnapshot(serviceId, snapshot);
            return true;
        }

        string RedisKey(string key)
        {
            return string.Concat(SNAPSHOT_KEY_PREFIX, ":", key);
        }

        void VerifyInputName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (name.Length > maxNameLength)
            {
                throw new ServiceException(ServiceException.InvalidSnapshotName, maxNameLength.ToString());
            }
        }

        protected void DeleteSnapshot(string serviceId, long snapshotId)
        {
            this.Client.Hashes.Remove(0, this.RedisKey(serviceId), snapshotId.ToString());
        }

        public void Dispose()
        {
            redis.Close(true);
        }
    }
}
