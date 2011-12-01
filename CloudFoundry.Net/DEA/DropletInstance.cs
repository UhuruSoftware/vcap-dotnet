using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using Uhuru.Utilities;

namespace CloudFoundry.Net.DEA
{
    enum DropletInstanceState
    {
        RUNNING,
        STARTING,
        CRASHED,
        STOPPED,
        DELETED
    }

    enum DropletExitReason
    {
        DEA_EVACUATION,
        DEA_SHUTDOWN,
        CRASHED,
        STOPPED,
    }

    class DropletInstance : ICloneable
    {
        public readonly object CollectionsLock = new object();

        private readonly object startLock = new object();
        DateTime start;
        private readonly object stateTimestampLock = new object();
        DateTime stateTimestamp;

        volatile DropletInstanceState state;
        volatile DropletExitReason exitReason;
        volatile bool orphaned;
        volatile bool resourcesTracked;
        volatile bool stopProcessed;
        volatile string debugMode;
        volatile int port;
        volatile int debugPort;
        volatile string debugIp;
        volatile string runtime;
        volatile int fdsQuota;
        volatile int diskQuota;
        volatile string name;
        volatile string instanceId;
        volatile string version;
        volatile int dropletId;
        volatile int instanceIndex;
        volatile string dir;
        HashSet<string> uris = new HashSet<string>();
        List<string> users = new List<string>();
        volatile int memQuota;
        volatile string framework;
        volatile string logId;
        volatile bool evacuated;
        volatile int pid;
        volatile bool notified;
        volatile int nice;
        volatile string secureUser;
        volatile string key;
        Dictionary<string, long> usage;
        volatile string staged;

        internal DropletInstanceState State
        {
            get { return state; }
            set { state = value; }
        }

        internal DropletExitReason ExitReason
        {
            get { return exitReason; }
            set { exitReason = value; }
        }

        public bool Orphaned
        {
            get { return orphaned; }
            set { orphaned = value; }
        }

        public DateTime Start
        {
            get 
            {
                lock (startLock)
                {
                    return start;
                }
            }
            set
            {
                lock (startLock)
                {
                    start = value;
                }
            }
        }

        public DateTime StateTimestamp
        {
            get
            {
                lock (stateTimestampLock)
                {
                    return stateTimestamp;
                }
            }
            set
            {
                lock (stateTimestampLock)
                {
                    stateTimestamp = value;
                }
            }
        }

        public bool ResourcesTracked
        {
            get { return resourcesTracked; }
            set { resourcesTracked = value; }
        }

        public bool StopProcessed
        {
            get { return stopProcessed; }
            set { stopProcessed = value; }
        }

        public string DebugMode
        {
            get { return debugMode; }
            set { debugMode = value; }
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public int DebugPort
        {
            get { return debugPort; }
            set { debugPort = value; }
        }

        public string DebugIp
        {
            get { return debugIp; }
            set { debugIp = value; }
        }

        public string Runtime
        {
            get { return runtime; }
            set { runtime = value; }
        }

        public int FdsQuota
        {
            get { return fdsQuota; }
            set { fdsQuota = value; }
        }

        public int DiskQuota
        {
            get { return diskQuota; }
            set { diskQuota = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string InstanceId
        {
            get { return instanceId; }
            set { instanceId = value; }
        }

        public string Version
        {
            get { return version; }
            set { version = value; }
        }

        public int DropletId
        {
            get { return dropletId; }
            set { dropletId = value; }
        }

        public int InstanceIndex
        {
            get { return instanceIndex; }
            set { instanceIndex = value; }
        }

        public string Dir
        {
            get { return dir; }
            set { dir = value; }
        }

        public HashSet<string> Uris
        {
            get { return uris; }
        }

        public List<string> Users
        {
            get { return users; }
        }

        public int MemQuota
        {
            get { return memQuota; }
            set { memQuota = value; }
        }

        public string Framework
        {
            get { return framework; }
            set { framework = value; }
        }

        public string LogId
        {
            get { return logId; }
            set { logId = value; }
        }

        public bool Evacuated
        {
            get { return evacuated; }
            set { evacuated = value; }
        }

        public int Pid
        {
            get { return pid; }
            set { pid = value; }
        }

        public bool Notified
        {
            get { return notified; }
            set { notified = value; }
        }

        public int Nice
        {
            get { return nice; }
            set { nice = value; }
        }

        public string SecureUser
        {
            get { return secureUser; }
            set { secureUser = value; }
        }

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        public Dictionary<string, long> Usage
        {
            get { return usage; }
        }

        public string Staged
        {
            get { return staged; }
            set { staged = value; }
        }

        public DropletInstance(string instanceId)
        {
            this.instanceId = instanceId;
            this.usage = new Dictionary<string, long>()
                { 
                    {"time", (long)Utils.DateTimeToEpochSeconds(DateTime.Now)},
                    {"cpu", 0}, 
                    {"mem", 0}, 
                    {"disk", 0}
                };
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            dict.Add("state", state.ToString());
            dict.Add("exit_reason", exitReason.ToString());
            dict.Add("orphaned", orphaned);
            dict.Add("start", Utils.DateTimeToRubyString(Start));
            dict.Add("state_timestamp", Utils.DateTimeToEpochSeconds(StateTimestamp));
            dict.Add("resources_tracked", resourcesTracked);
            dict.Add("stop_processed", stopProcessed);
            dict.Add("debug_mode", debugMode);
            dict.Add("port", port);
            dict.Add("debug_port", debugPort);
            dict.Add("debug_ip", debugIp);
            dict.Add("runtime", runtime);
            dict.Add("fds_quota", fdsQuota);
            dict.Add("disk_quota", diskQuota);
            dict.Add("name", name);
            dict.Add("instance_id", instanceId);
            dict.Add("version", version);
            dict.Add("droplet_id", dropletId);
            dict.Add("instance_index", instanceIndex);
            dict.Add("dir", dir);
            dict.Add("uris", uris);
            dict.Add("users", users);
            dict.Add("mem_quota", memQuota);
            dict.Add("framework", framework);
            dict.Add("log_id", logId);
            dict.Add("evacuated", evacuated);
            dict.Add("pid", pid);
            dict.Add("notified", notified);
            dict.Add("nice", nice);
            dict.Add("secure_user", secureUser);
            dict.Add("key", key);
            dict.Add("staged", staged);
            dict.Add("usage", usage);

            lock (CollectionsLock)
            {
                return dict;
            }
        }

        public string ToJson()
        {
            return ToDictionary().ToJson();
        }

        public void FromDictionary(Dictionary<string, object> dict)
        {
            lock (CollectionsLock)
            {

                this.state = (DropletInstanceState)Enum.Parse(typeof(DropletInstanceState), (string)dict["state"]);
                this.exitReason = (DropletExitReason)Enum.Parse(typeof(DropletExitReason), (string)dict["exit_reason"]);

                this.orphaned = dict["orphaned"].ToValue<bool>();
                this.resourcesTracked = dict["resources_tracked"].ToValue<bool>();
                this.stopProcessed = dict["stop_processed"].ToValue<bool>();
                this.debugMode = dict["debug_mode"].ToValue<string>();
                this.port = dict["port"].ToValue<int>();
                this.debugPort = dict["debug_port"].ToValue<int>();
                this.debugIp = dict["debug_ip"].ToValue<string>();
                this.runtime = dict["runtime"].ToValue<string>();
                this.fdsQuota = dict["fds_quota"].ToValue<int>();
                this.diskQuota = dict["disk_quota"].ToValue<int>();
                this.name = dict["name"].ToValue<string>();
                this.instanceId = dict["instance_id"].ToValue<string>();
                this.version = dict["version"].ToValue<string>();
                this.dropletId = dict["droplet_id"].ToValue<int>();
                this.instanceIndex = dict["instance_index"].ToValue<int>();
                this.dir = dict["dir"].ToValue<string>();
                this.uris = dict["uris"].ToObject<HashSet<string>>();
                this.users = dict["users"].ToObject<List<string>>();
                this.memQuota = dict["mem_quota"].ToValue<int>();
                this.framework = dict["framework"].ToValue<string>();
                this.logId = dict["log_id"].ToValue<string>();
                this.evacuated = dict["evacuated"].ToValue<bool>();
                this.pid = dict["pid"].ToValue<int>();
                this.notified = dict["notified"].ToValue<bool>();
                this.nice = dict["nice"].ToValue<int>();
                this.secureUser = dict["secure_user"].ToValue<string>();
                this.key = dict["key"].ToValue<string>();
                this.staged = dict["staged"].ToValue<string>();
                this.usage = dict["usage"].ToObject<Dictionary<string, long>>();

                this.Start = Utils.DateTimeFromRubyString(dict["start"].ToValue<string>());
                this.StateTimestamp = Utils.DateTimeFromEpochSeconds(dict["state_timestamp"].ToValue<int>());

            }
        }

        public object Clone()
        {
            DropletInstance instance = new DropletInstance(this.InstanceId);
            instance.state = this.State;
            instance.exitReason = this.ExitReason;
            instance.orphaned = this.Orphaned;
            instance.Start = this.Start;
            instance.StateTimestamp = this.StateTimestamp;
            instance.resourcesTracked = this.ResourcesTracked;
            instance.stopProcessed = this.StopProcessed;
            instance.debugMode = this.DebugMode;
            instance.port = this.Port;
            instance.debugPort = this.DebugPort;
            instance.debugIp = this.DebugIp;
            instance.runtime = this.Runtime;
            instance.fdsQuota = this.FdsQuota;
            instance.diskQuota = this.DiskQuota;
            instance.name = this.Name;
            instance.version = this.Version;
            instance.dropletId = this.DropletId;
            instance.instanceIndex = this.InstanceIndex;
            instance.dir = this.Dir;
            instance.uris = new HashSet<string>();
            instance.users = new List<string>();
            instance.memQuota = this.MemQuota;
            instance.framework = this.Framework;
            instance.logId = this.LogId;
            instance.evacuated = this.Evacuated;
            instance.pid = this.Pid;
            instance.notified = this.Notified;
            instance.nice = this.Nice;
            instance.secureUser = this.SecureUser;
            instance.key = this.Key;
            instance.usage = new Dictionary<string, long>();
            lock (CollectionsLock)
            {
                foreach (string key in this.usage.Keys)
                {
                    instance.usage[key] = this.usage[key];
                }

                foreach (string user in this.users)
                {
                    instance.users.Add(user);
                }

                foreach (string uri in this.uris)
                {
                    instance.uris.Add(uri);
                }
            }

            return instance;
        }
    }
}