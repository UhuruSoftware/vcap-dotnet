using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using CloudFoundry.Net.Nats;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Uhuru.Utilities;
using System.Collections.Concurrent;
using CloudFoundry.Net.Configuration;
using Uhuru.Utilities.ProcessPerformance;

namespace CloudFoundry.Net.DEA
{


    // VCAP::Commnon.register, VCAP::Commnon.varz and VCAP::Commnon.uuid is used int agent

    public class NonFatalTimeOutError : System.Exception
    {
    }

    public delegate void BoolStateBlockDelegate(bool state);


    public class Agent
    {

        const double VERSION = 0.99;

        // Some sane app resource defaults
        const int DEFAULT_APP_MEM = 512; //512MB
        const int DEFAULT_APP_DISK = 256; //256MB
        const int DEFAULT_APP_NUM_FDS = 1024;

        // Max limits for DEA
        const int DEFAULT_MAX_CLIENTS = 1024;

        const int MONITOR_INTERVAL = 2000;    // 2 secs
        const int MAX_USAGE_SAMPLES = (1 * 60000) / MONITOR_INTERVAL;  // 1 minutes @ 5 sec interval
        const int CRASHES_REAPER_INTERVAL = 30000;   // 30 secs
        const int CRASHES_REAPER_TIMEOUT = 3600;  // delete crashes older than 1 hour

        // CPU Thresholds
        const int BEGIN_RENICE_CPU_THRESHOLD = 50;
        const int MAX_RENICE_VALUE = 20;

        const int VARZ_UPDATE_INTERVAL = 1000;    // 1 secs

        // The state of the droplets
        const string APP_STATE_FILE = "applications.json";

        const int TAINT_MS_PER_APP = 10;
        const int TAINT_MS_FOR_MEM = 100;
        const int TAINT_MAX_DELAY = 250;

        const int DEFAULT_EVACUATION_DELAY = 30000;  // Default time to wait (in secs) for evacuation and restart of apps.

        // How long to wait in between logging the structure of the apps directory in the event that a du takes excessively long
        const int APPS_DUMP_INTERVAL = 30 * 60000;


        const int PID_INDEX = 0;
        const int PPID_INDEX = 1;
        const int CPU_INDEX = 2;
        const int MEM_INDEX = 3;
        const int USER_INDEX = 4;
        const int FD_INDEX = 3;
        const int TYPE_INDEX = 4;
        const int SIZE_INDEX = 6;
        const int DELETED_INDEX = 9;

        volatile private bool snapshot_scheduled = false;
        private bool shutting_down = false;
        private bool recovered_droplets = false;
        

        private int reserved_mem = 0;   //total memory allocead to instances
        private int num_clients = 0;    //number of instances that have resources allocated
        private int mem_usage = 0;      //total memory used by apps

        private int max_memory;
        private int max_clients;

        DropletCollection droplets = new DropletCollection();

        private string staged_dir;  
        private string db_dir;
        private string app_state_file;
            public string GetAppStateFile { get { return app_state_file; } }
        private string apps_dir;  
            public string GetAppsDir{get{return apps_dir;}}
        
        private string local_ip;

        private bool disable_dir_cleanup;
        private bool enforce_ulimit;
        private bool multi_tenant;
        private int num_cores;
        private int file_viewer_port;
        private int filer_start_attempts = 0; ////How many times we've tried to start the filer
        private System.Timers.Timer filer_start_timer = null;

        //refactor: try to get rid of this; save it into natClient
        private string nats_uri;

        private int heartbeat_interval;

        private string droplet_dir;


        private DateTime last_apps_dump = DateTime.MinValue;
        private Dictionary<int, List<Dictionary<string, long>>> usage = new Dictionary<int, List<Dictionary<string, long>>>();
        private Dictionary<string, object> hello_message;

        private CloudFoundry.Net.Nats.Client natsClient;
        private Dictionary<string, DeaRuntime> runtimes = new Dictionary<string,DeaRuntime>();
        private bool force_http_sharing;

        private readonly object downloadsPendingLock = new object();
        private Dictionary<string, ManualResetEvent> downloads_pending = new Dictionary<string, ManualResetEvent>();
        private readonly object unpacksPendingLock = new object();
        private Dictionary<string, int> unpacks_pending = new Dictionary<string, int>();

        
        private int evacuation_delay;
        private System.Timers.Timer evacuation_delay_timer;

        private FileServer file_viewer_server;
        private bool secure;
        private string apps_dump_dir;
        private string[] file_auth;

        private VcapComponent vcapComponent = new VcapComponent();

        public Agent(string ConfigFile = null)
        {


            
            num_cores = Environment.ProcessorCount;

            

            foreach(Configuration.DEA.RuntimeElement deaConf in UhuruSection.GetSection().DEA.Runtimes)
            {
                DeaRuntime dea = new DeaRuntime();

                
                dea.Executable = deaConf.Executable;
                dea.Version = deaConf.Version;
                dea.VersionFlag = deaConf.VersionFlag;
                dea.AdditionalChecks = deaConf.AdditionalChecks;
                dea.Enabled = true;

                //todo: stefi: add environments

                //deaConf.Environment
                foreach(Configuration.DEA.EnvironmentElement ienv in deaConf.Environment)
                {
                    dea.Environment.Add(ienv.Name, ienv.Value);
                }
                
                
                runtimes.Add(deaConf.Name, dea);
            }


            droplet_dir = UhuruSection.GetSection().DEA.BaseDir;

            enforce_ulimit = UhuruSection.GetSection().DEA.EnforceUlimit;
            disable_dir_cleanup = UhuruSection.GetSection().DEA.DisableDirCleanup;
            multi_tenant = UhuruSection.GetSection().DEA.MultiTenant;

            string local_route = UhuruSection.GetSection().DEA.LocalRoute;
            max_memory = UhuruSection.GetSection().DEA.MaxMemory;   //in MB


            file_viewer_port = UhuruSection.GetSection().DEA.FilerPort;
            force_http_sharing = UhuruSection.GetSection().DEA.ForceHttpSharing;

            evacuation_delay = DEFAULT_EVACUATION_DELAY;


            //apps_dump_dir = ConfigurationManager.AppSettings["logFile"] ?? Path.GetTempPath();
            apps_dump_dir = Path.GetTempPath();

            nats_uri = UhuruSection.GetSection().DEA.MessageBus;
            //heartbeat_interval = UhuruSection.GetSection().DEA.HeartBeatInterval;
            heartbeat_interval = 10000;

            local_ip = VcapComponent.GetLocalIpAddress(local_route);
            max_clients = multi_tenant ? DEFAULT_MAX_CLIENTS : 1;

            staged_dir = Path.Combine(droplet_dir, "staged");
            apps_dir = Path.Combine(droplet_dir, "apps");
            db_dir = Path.Combine(droplet_dir, "db");
            app_state_file = Path.Combine(droplet_dir, APP_STATE_FILE);


        }


        //Run the DEA in blocking mode
        public void Run()
        {

            

            Logger.info(String.Format("Starting VCAP DEA {0}", VERSION));
            /*
          @logger.info("Pid file: %s" % (@pid_filename))
          begin
            @pid_file = VCAP::PidFile.new(@pid_filename)
          rescue => e
            @logger.fatal("Can't create DEA pid file: #{e}")
            exit 1
          end
             */

            //todo: Bundler.with_clean_env ?
            setup_runtimes();

            Logger.info(String.Format("Using network {0}", local_ip));
            Logger.info(String.Format("Max memory set to {0}M", max_memory));
            Logger.info(String.Format("Utilizing {0} cpu cores", num_cores));
            if (multi_tenant)
            {
                Logger.info(String.Format("Allowing multi-tenancy"));
            }
            else
            {
                Logger.info(String.Format("Restricting to single tenant"));
            }

            Logger.info(String.Format("Using directory {0}", droplet_dir));

            try
            {
                Directory.CreateDirectory(droplet_dir);
                Directory.CreateDirectory(staged_dir);
                Directory.CreateDirectory(apps_dir);
                Directory.CreateDirectory(db_dir);
            }
            catch (Exception e)
            {
                Logger.fatal(String.Format("Can't create supported directories: {0}", e.ToString()));
                throw e;    
            }


            try
            {
                ensure_writable(apps_dump_dir);
            }
            catch (Exception e)
            {
                Logger.fatal(String.Format("The system is FUBAR. Unable to write to {0}", apps_dump_dir));
                throw e;
            }

            //Clean everything in the staged directory
            if (!disable_dir_cleanup)
            {
                DirectoryInfo directory = new DirectoryInfo(staged_dir);
                foreach(System.IO.FileInfo file in directory.GetFiles()) 
                    file.Delete();
                foreach(System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) 
                    subDirectory.Delete(true);
            }


            ThreadPool.QueueUserWorkItem(delegate(object data)
            {
                filer_start_timer = new System.Timers.Timer(1000);
                filer_start_timer.AutoReset = false;
                filer_start_timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
                {
                    if (start_file_viewer())
                    {
                        filer_start_timer.Enabled = false;
                        filer_start_timer = null;
                    }
                    else
                    {
                        filer_start_timer.Enabled = true;
                    }
                });
                filer_start_timer.Enabled = true;
            });


            natsClient = new CloudFoundry.Net.Nats.Client();
            
            natsClient.OnError += new EventHandler<NatsEventArgs>(delegate(object sender, NatsEventArgs args)
            {
                string errorThrown = args.Message == null ? String.Empty : args.Message;
                Logger.error(String.Format("EXITING! Nats error: {0}", errorThrown));


                // Only snapshot app state if we had a chance to recover saved state. This prevents a connect error
                // that occurs before we can recover state from blowing existing data away.
                if (recovered_droplets)
                {
                    snapshot_app_state();
                }

                throw new Exception(String.Format("Nats error: {0}", errorThrown));
            });

            natsClient.Start(nats_uri);


            Dictionary<string, object> opts = new Dictionary<string,object>(){
                {"type", "DEA"},
                {"host", local_ip},
                {"nats", natsClient},
                {"user", ""},
                {"password", ""},

                //{"config", }
                //{"index", config["index"]}
            };


            vcapComponent.Register(opts);

            string uuid = vcapComponent.Uuid;

            hello_message = new Dictionary<string, object>()
            {
                {"id", uuid},
                {"ip", local_ip},
                {"port", file_viewer_port},
                {"version", VERSION}
            };


            natsClient.Subscribe("dea.status", new SubscribeCallback(process_dea_status));
            natsClient.Subscribe("droplet.status", new SubscribeCallback(process_droplet_status));
            natsClient.Subscribe("dea.discover", new SubscribeCallback(process_dea_discover));
            natsClient.Subscribe("dea.find.droplet", new SubscribeCallback(process_dea_find_droplet));
            natsClient.Subscribe("dea.update", new SubscribeCallback(process_dea_update));

            natsClient.Subscribe("dea.stop", new SubscribeCallback(process_dea_stop));
            natsClient.Subscribe(String.Format("dea.{0}.start", uuid), new SubscribeCallback(process_dea_start));

            natsClient.Subscribe("router.start", new SubscribeCallback(process_router_start));
            natsClient.Subscribe("healthmanager.start", new SubscribeCallback(process_healthmanager_start));

            recover_existing_droplets();
            delete_untracked_instance_dirs();


            System.Timers.Timer timer;

            timer = new System.Timers.Timer(heartbeat_interval);
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
            {
                send_heartbeat();
            });
            timer.Enabled = true;

            timer = new System.Timers.Timer(MONITOR_INTERVAL);
            timer.AutoReset = false;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
            {
                monitor_apps();
            });
            timer.Enabled = true;

            timer = new System.Timers.Timer(CRASHES_REAPER_INTERVAL);
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
            {
                crashes_reaper();
            });
            timer.Enabled = true;

            timer = new System.Timers.Timer(VARZ_UPDATE_INTERVAL);
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
            {
                snapshot_varz();
            });
            timer.Enabled = true;


            natsClient.Publish("dea.start", msg: hello_message.ToJson());

        }


        //refactor: stefi: is going to be replaced by Agent.SendHeartbeat
        void send_heartbeat()
        {
            if (shutting_down || droplets.Count==0)
                return;

            Dictionary<string, object> hearbeat = new Dictionary<string,object>() {
                {"droplets", new List<object>()}
            };

            foreach (Droplet droplet in droplets)
            {
                foreach (DropletInstance instance in droplet.Instances)
                {
                    ((List<object>)hearbeat["droplets"]).Add(generate_heartbeat(instance));
                }
            }

            natsClient.Publish("dea.heartbeat", msg: hearbeat.ToJson());
        }


        //refactor: stefi: is going to be replaced by Agent.SendHeartbeat
        void send_single_heartbeat(DropletInstance instance)
        {
            Dictionary<string, object> heartbeat = new Dictionary<string,object>() {
                {"droplets", new List<object>()}
            };

            ((List<object>)heartbeat["droplets"]).Add(generate_heartbeat(instance));
            natsClient.Publish("dea.heartbeat", msg: heartbeat.ToJson());
        }


        //refactor: stefi: goes into DropletInstance.GenerateHearbeat
        object generate_heartbeat(DropletInstance instance) 
        {
            return
                new Dictionary<string, object>()
                {
                    {"droplet", instance.DropletId},
                    {"version", instance.Version},
                    {"instance", instance.InstanceId},
                    {"index", instance.InstanceIndex},
                    {"state", instance.State.ToString()},
                    {"state_timestamp", Utils.DateTimeToEpochSeconds(instance.StateTimestamp)}
                };
        }



        //Nats event handle

        //refactor: stefi: put some code into DropletInstance.GenerateDropletStatusResponse
        void process_droplet_status(string message, string reply, string subject)
        {
            if (shutting_down)
                return;

            foreach (Droplet droplet in droplets)
            {
                foreach (DropletInstance instance in droplet.Instances)
                {
                    if (instance.State == DropletInstanceState.RUNNING || instance.State == DropletInstanceState.STARTING)
                    {
                        
                        Dictionary<string, object> response= new Dictionary<string, object>()
                        {
                            {"name", instance.Name},
                            {"host", local_ip},
                            {"port", instance.Port},
                            {"uris", ((DropletInstance)instance.Clone()).Uris},
                            {"uptime", (DateTime.Now - instance.Start).TotalSeconds},
                            {"mem_quota", instance.MemQuota},
                            {"disk_quota", instance.DiskQuota},
                            {"fds_quota", instance.FdsQuota}
                        };

                        if (usage.ContainsKey(instance.Pid))
                        {
                            response.Add("usage", usage[instance.Pid][usage[instance.Pid].Count - 1]);
                        }

                        natsClient.Publish(reply, msg: response.ToJson());
                        
                    }

                }
            }

        }

        void snapshot_varz()
        {
            vcapComponent.varz["apps_max_memory"] = max_memory;
            vcapComponent.varz["apps_reserved_memory"] = reserved_mem;
            vcapComponent.varz["apps_used_memory"] = (int)(mem_usage / 1024);
            vcapComponent.varz["num_apps"] = num_clients;
            if (shutting_down)
                vcapComponent.varz["state"] = "SHUTTING_SOWN";

        }


        //Nats event handle
        void process_dea_status(string message, string reply, string subject)
        {
            Logger.debug("DEA received status message");
            Dictionary<string, object> response = new Dictionary<string, object>(hello_message);
            response.Add("max_memory", max_memory);
            response.Add("reserved_memory", reserved_mem);
            response.Add("used_memory", mem_usage);
            response.Add("num_clients", num_clients);
            if(shutting_down)
                response.Add("state", "SHUTTING_DOWN");

            natsClient.Publish(reply, msg: response.ToJson());
        }


        //Nats event handle
        void process_dea_discover(string message, string reply, string subject)
        {
            Logger.debug(String.Format("DEA received discovery message: {0}", message));
            if (shutting_down)
            {
                Logger.debug("Ignoring request, shutting down.");
                return;
            }
            if(num_clients >= max_clients || reserved_mem > max_memory)
            {
                Logger.debug(String.Format("Ignoring request, not enough resources."));
                return;
            }

            Dictionary<string, object> pmessage = new Dictionary<string, object>(); pmessage = pmessage.FromJson(message);
            if (!runtime_supported(pmessage["runtime"].ToValue<string>()))
            {
                Logger.debug(String.Format("Ignoring request, {0} runtime not supported",pmessage["runtime"].ToObject<string>() ));
                return;
            }

            Dictionary<string, int> limits = pmessage["limits"].ToObject<Dictionary<string, int>>();
            int mem_needed = limits["mem"];
            int droplet_id = pmessage["droplet"].ToValue<int>();
            if (reserved_mem + mem_needed > max_memory)
            {
                Logger.debug(String.Format("Ignoring request, not enough resources."));
                return;
            }
            double delay = Math.Max(1, calculate_help_taint(droplet_id));
            delay = Math.Min(delay, TAINT_MAX_DELAY);

            System.Timers.Timer timer = new System.Timers.Timer(delay);
            timer.AutoReset = false;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
            {
                natsClient.Publish(reply, msg: hello_message.ToJson());
            });
            timer.Enabled = true;

 
        }

        int calculate_help_taint(int droplet_id)
        {
            int taint_ms = 0;
            try
            {
                Droplet aleardy_running = droplets[droplet_id];
                taint_ms += (aleardy_running.Instances.Count * TAINT_MS_PER_APP);
            }
            catch(KeyNotFoundException){}

            double mem_percent = (double)reserved_mem / (double)max_memory;
            taint_ms += (int)(mem_percent * TAINT_MS_FOR_MEM);

            return taint_ms;
        }


        // Nats event handle
        //refactor: stefi: replace with DropletInstance.GenerateDeaFindDropletResponse
        void process_dea_find_droplet(string message, string reply, string subject)
        {
            if (shutting_down)
                return;

            Dictionary<string, object> pmessage = new Dictionary<string, object>(); pmessage = pmessage.FromJson(message);

            Logger.debug(String.Format("DEA received find droplet message: {0}", message));


            int droplet_id = pmessage["droplet"].ToValue<int>();
            string version = pmessage.ContainsKey("version") ? pmessage["version"].ToValue<string>() : null;

            HashSet<string> instance_ids = pmessage.ContainsKey("instances") ? pmessage["instances"].ToObject<HashSet<string>>() : null;
            HashSet<int> indices = pmessage.ContainsKey("indices") ? pmessage["indices"].ToObject<HashSet<int>>() : null;
            HashSet<string> states = pmessage.ContainsKey("states") ? pmessage["states"].ToObject<HashSet<string>>() : null;
            bool include_stats = pmessage.ContainsKey("include_stats") ? pmessage["include_stats"].ToValue<bool>() : false;

            Droplet droplet;
            try
            {
                droplet = droplets[droplet_id];
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            foreach (DropletInstance instance in droplet.Instances)
            {
                bool version_match = version == null || version == instance.Version;
                bool instace_match = instance_ids == null || instance_ids.Contains(instance.InstanceId);
                bool index_match = indices == null || indices.Contains(instance.InstanceIndex);
                bool state_match = states == null || states.Contains(instance.State.ToString());


                Dictionary<string, object> response = new Dictionary<string,object>();

                if (version_match && instace_match && index_match && state_match)
                {
                    response.Add("dea", vcapComponent.Uuid);
                    response.Add("version", instance.Version);
                    response.Add("droplet", instance.DropletId);
                    response.Add("instance", instance.InstanceId);
                    response.Add("index", instance.InstanceIndex);
                    response.Add("state", instance.State.ToString());
                    response.Add("state_timestamp", Utils.DateTimeToEpochSeconds(instance.StateTimestamp));
                    response.Add("file_uri", String.Format(@"http://{0}:{1}/droplets/", local_ip, file_viewer_port));
                    response.Add("credentials", file_auth);
                    response.Add("staged", instance.Staged);
                    response.Add("debug_ip", instance.DebugIp);
                    response.Add("debug_port", instance.DebugPort);
                }
                if (include_stats && instance.State == DropletInstanceState.RUNNING)
                {
                    Dictionary<string, object> stats = new Dictionary<string, object>()
                    {
                        {"name", instance.Name},
                        {"host", local_ip},
                        {"port", instance.Port},
                        {"uris", ((DropletInstance)instance.Clone()).Uris},
                        {"mem_quota", instance.MemQuota},
                        {"disk_quota", instance.DiskQuota},
                        {"fds_quota", instance.FdsQuota},
                        {"cores", num_cores}

                    };
                    if (usage.ContainsKey(instance.Pid))
                        {
                            stats.Add("usage", usage[instance.Pid][usage[instance.Pid].Count - 1]);
                        }

                    response.Add("stats", stats);
                }

                natsClient.Publish(reply, msg: response.ToJson());
            }

        }

        // Nats event handle
        void process_dea_update(string message, string reply, string subject)
        {
            if (shutting_down)
                return;

            Dictionary<string, object> pmessage = new Dictionary<string, object>(); pmessage = pmessage.FromJson(message);

            Logger.debug(String.Format("DEA received update message: {0}", message));

            int droplet_id = pmessage["droplet"].ToValue<int>();

            Droplet droplet;
            try
            {
                droplet = droplets[droplet_id];
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            HashSet<string> uris = pmessage["uris"].ToObject<HashSet<string>>();

            foreach (DropletInstance instance in droplet.Instances)
            {
                HashSet<string> current_uris = ((DropletInstance)instance.Clone()).Uris;

                Logger.debug(String.Format("Mapping new URIs"));
                Logger.debug(String.Format("New: {0} Current: {1}", uris, current_uris));

                Dictionary<string, object> duris;

                duris = new Dictionary<string,object>();
                duris.Add("uris", new HashSet<string>(uris.Except(current_uris))  );
                register_instance_with_router(instance, duris);

                duris = new Dictionary<string, object>();
                duris.Add("uris", new HashSet<string>(current_uris.Except(uris)) );
                unregister_instance_from_router(instance, duris);

                lock (instance.CollectionsLock)
                {
                    instance.Uris.Clear();

                    foreach (string uri in uris)
                    {
                        instance.Uris.Add(uri);
                    }
                }
            }

        }

        //Nats event handle
        void process_dea_stop(string message, string reply, string subject)
        {
            if (shutting_down)
                return;

            Dictionary<string, object> pmessage = new Dictionary<string, object>(); pmessage = pmessage.FromJson(message);

            Logger.debug(String.Format("DEA received stop message: {0}", message));

            int droplet_id = pmessage["droplet"].ToValue<int>();
            string version = pmessage.ContainsKey("version") ? pmessage["version"].ToValue<string>() : null;

            HashSet<string> instance_ids = pmessage.ContainsKey("instances") ? pmessage["instances"].ToObject<HashSet<string>>() : null;
            HashSet<int> indices = pmessage.ContainsKey("indices") ? pmessage["indices"].ToObject<HashSet<int>>() : null;
            HashSet<string> states = pmessage.ContainsKey("states") ? pmessage["states"].ToObject<HashSet<string>>() : null;
            

            Droplet droplet;
            try
            {
                droplet = droplets[droplet_id];
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            foreach (DropletInstance instance in droplet.Instances)
            {
                bool version_match = version == null || version == instance.Version;
                bool instace_match = instance_ids == null || instance_ids.Contains(instance.InstanceId);
                bool index_match = indices == null || indices.Contains(instance.InstanceIndex);
                bool state_match = states == null || states.Contains(instance.State.ToString());

                if (version_match && instace_match && index_match && state_match)
                {
                    if (instance.State == DropletInstanceState.STARTING || instance.State == DropletInstanceState.RUNNING)
                    {
                        instance.ExitReason = DropletExitReason.STOPPED;
                    }
                    if(instance.State ==  DropletInstanceState.CRASHED)
                    {
                        instance.State = DropletInstanceState.DELETED;
                        instance.StopProcessed = false;
                    }

                    stop_droplet(instance);
                }
            }
 
        }


        //Nats handle
        void process_dea_start(string message, string reply, string subject)
        {

            if (shutting_down)
                return;

            Dictionary<string, object> pmessage = new Dictionary<string, object>(); pmessage = pmessage.FromJson(message);

            Logger.debug(String.Format("DEA received start message: {0}", message));


            string instance_id = Guid.NewGuid().ToString();

            int droplet_id = pmessage["droplet"].ToValue<int>();
            int instance_index = pmessage["index"].ToValue<int>();

            List<Dictionary<string, object>> services = pmessage["services"].ToObject<List<Dictionary<string, object>>>();
            
            string version = pmessage["version"].ToValue<string>();
            string bits_file = pmessage["executableFile"].ToValue<string>();
            string bits_uri = pmessage["executableUri"].ToValue<string>();
            string name = pmessage["name"].ToValue<string>();
            HashSet<string> uris = pmessage["uris"].ToObject< HashSet<string> >();
            string sha1 = pmessage["sha1"].ToValue<string>();
            List<string> app_env = pmessage["env"].ToObject<List<string>>();
            List<string> users = pmessage["users"].ToObject<List<string>>();
            string runtime = pmessage["runtime"].ToValue<string>();
            string framework = pmessage["framework"].ToValue<string>();
            string debug = !pmessage.ContainsKey("debug") ? null : pmessage["debug"].ToValue<string>();
            
            int mem = DEFAULT_APP_MEM;
            int num_fds = DEFAULT_APP_NUM_FDS;
            int disk = DEFAULT_APP_DISK;

            try
            {
                Dictionary<string, object> limits = pmessage["limits"].ToObject<Dictionary<string, object>>();
                if (limits.ContainsKey("mem")) mem = limits["mem"].ToValue<int>();
                if (limits.ContainsKey("num_fds")) num_fds = limits["num_fds"].ToValue<int>();
                if (limits.ContainsKey("disk")) disk = limits["disk"].ToValue<int>();
            }
            catch (Exception) { }

            Logger.debug(String.Format("Requested Limits: mem={0}M, fds={1}, disk={2}M", mem, num_fds, max_clients));

            if (shutting_down)
            {
                Logger.info("Shutting down, ignoring start request");
                return;
            }
            if (reserved_mem + mem > max_memory || num_clients >= max_clients)
            {
                Logger.info("Do not have room for this client application");
                return;
            }

            if (sha1 == "" || bits_file == "" || bits_uri == "")
            {
                Logger.warn(String.Format("Start request missing proper download information, ignoring request. ({0})", message));
                return;
            }

            if (!runtime_supported(runtime))
            {
                return;
            }

            string tgz_file = Path.Combine(staged_dir, sha1 + ".tgz");
            string instance_dir = Path.Combine(apps_dir, name + "-" + instance_index + "-" + instance_id);

            DropletInstance instance = new DropletInstance(instance_id);
            instance.DropletId = droplet_id;
            instance.InstanceIndex = instance_index;
            instance.Name = name;
            instance.Dir = instance_dir;

            lock (instance.CollectionsLock)
            {
                foreach (string uri in uris)
                {
                    instance.Uris.Add(uri);
                }

                foreach (string user in users)
                {
                    instance.Users.Add(user);
                }
            }

            instance.Version = version;
            instance.MemQuota = mem * (1024 * 1024);
            instance.DiskQuota = disk * (1024 * 1024);
            instance.FdsQuota = num_fds;
            instance.State = DropletInstanceState.STARTING;
            instance.Runtime = runtime;
            instance.Framework = framework;
            instance.Start = DateTime.Now;

            //instance.StateTimestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            //:state_timestamp => Time.now.to_i
            instance.StateTimestamp = DateTime.Now;
            instance.LogId = String.Format("name={0} app_id={1} instance={2} index={3}", name, droplet_id, instance_id, instance_index);

            Droplet droplet;

            if (droplets.DropletExists(droplet_id))
            {
                droplet = droplets[droplet_id];
            }
            else
            {
                droplet = new Droplet(droplet_id);
                droplets.Add(droplet);
            }

            droplet.Instances.Add(instance);

            //start_operation proc defined

            add_instance_resources(instance);

            Logger.debug(String.Format("reserved_mem = {0} MB,  max_memory = {1} MB", reserved_mem, max_memory));

            

            ThreadPool.QueueUserWorkItem(delegate(object data)
            {

                bool success = stage_app_dir(bits_file, bits_uri, sha1, tgz_file, instance_dir, runtime);

                if (success)
                {
                    Logger.debug("Completed download");
                   
                    int port = VcapComponent.GetEphemeralPort();

                    instance.Port = port;

                    string starting = string.Format("Starting up instance {0} on port {1}", instance.LogId, instance.Port);

                    //todo: stefi: change the condition
                    if (debug != "")
                    {
                        int debug_port = VcapComponent.GetEphemeralPort();
                        instance.DebugIp = VcapComponent.GetLocalIpAddress();
                        instance.DebugMode = debug;

                        Logger.info(String.Format("{0} with debugger:{1}", starting, debug_port));
                    }
                    else
                    {
                        Logger.info(starting);
                    }

                    Logger.debug(String.Format("Clients: {0}", num_clients));
                    Logger.debug(String.Format("Reserved Memory Usage: {0} MB of {1} MB TOTAL", reserved_mem, max_memory));

                    //todo: stefi: figure out what is the manifest about
                    //manifest_file = File.join(instance[:dir], 'droplet.yaml')
                    //manifest = {}
                    //manifest = File.open(manifest_file) { |f| YAML.load(f) } if File.file?(manifest_file)

                    string prepare_script = Path.Combine(instance_dir, "prepare");

                    //FileUtils.cp(File.expand_path("../../../bin/close_fds", __FILE__), prepare_script)
                    //File.Copy(@"..\..\..\bin\close_fds", prepare_script);

                    app_env = setup_instance_env(instance, app_env, services);


                    int mem_kbytes = (int)((mem * 1024) * 1.125);

                    int ont_gb = 1024 * 1024 * 2;
                    int disk_limit = (disk * 1024) * 2 * 2;
                    if(disk_limit > ont_gb) disk_limit = ont_gb;



                    string execute_cmd = "cmd";

                    StreamWriterDelegate stopOperation = delegate(StreamWriter stdin)
                    {
                        stdin.WriteLine(String.Format("cd /D {0}", instance.Dir));
                        foreach (String env in app_env)
                        {
                            stdin.WriteLine(String.Format("set {0}", env));
                        }


                        string runtime_layer = String.Format("{0}\\netiis.exe",Directory.GetCurrentDirectory());

                        stdin.WriteLine("copy .\\startup .\\startup.ps1");
                        stdin.WriteLine(String.Format("powershell.exe -ExecutionPolicy Bypass -InputFormat None -noninteractive -file .\\startup.ps1 \"{0}\"", runtime_layer));
                        stdin.WriteLine("exit");
                    };

                    ProcessDoneDelegate exitOperation = delegate(string output, int status)
                    {
                        Logger.info(String.Format("{0} completed running with status = {1}.", name, status));
                        Logger.info(String.Format("{0} uptime was {1}.", name, DateTime.Now - instance.StateTimestamp));
                        stop_droplet(instance);
                    };

                    //TODO: vladi: this must be done with clean environment variables
                    Utils.ExecuteCommands(execute_cmd, "", stopOperation, exitOperation);

                    //instance[:staged] = instance_dir.sub("#{@apps_dir}/", '')
                    instance.Staged = instance_dir.Replace(String.Format("{0}/",apps_dir), "");

                    detect_app_ready(instance, new Dictionary<string, string>(), 
                        delegate(bool detected)
                        {
                            if (detected && !instance.StopProcessed)
                            {
                                Logger.info(String.Format("Instance {0} is ready for connections, notifying system of status", instance.LogId));
                                instance.State = DropletInstanceState.RUNNING;
                                instance.StateTimestamp = DateTime.Now;
                                send_single_heartbeat(instance);
                                register_instance_with_router(instance);
                                schedule_snapshot();
                            }
                            else
                            {
                                Logger.warn("Giving up on connecting app.");
                                stop_droplet(instance);
                            }
                        }
                    );


                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object data2)
                    {
                        int pid = detect_app_pid(instance_dir);
                        if (pid != 0 && !instance.StopProcessed)
                        {
                            Logger.info(String.Format("PID:{0} assigned to droplet instance: {1}", pid, instance.LogId));
                            instance.Pid = pid;
                            schedule_snapshot();
                        
                        }
                    }));
                    //stefi: code here



                }
                else
                {
                    Logger.debug(String.Format("Failed staging app dir '{0}', not starting app {1}", instance_dir, instance.LogId));

                    instance.State = DropletInstanceState.CRASHED;
                    instance.ExitReason = DropletExitReason.CRASHED;
                    instance.StateTimestamp = DateTime.Now;

                    stop_droplet(instance);

                    //The first call to cleanup_droplet() via stop_droplet() won't remove
                    //our instance from internal structures because we set the state to
                    //:CRASHED. We do that explicitly here instead of waiting for the
                    //crashes reaper to do so. This code is duplicated from
                    //cleanup_droplet() (gross, I know). The other option would be to set
                    //the state to STOPPED and call cleanup_droplet(). Not sure which is
                    //worse.


                    if (droplets.DropletExists(instance.DropletId))
                    {
                        droplets[instance.DropletId].Instances.Remove(instance);
                        if (droplets[instance.DropletId].Instances.Count == 0)
                        {
                            droplets.Delete(instance.DropletId);
                        }
                        schedule_snapshot();
                    }

                }

            });

            


        }

        //********************************************************************************************************
        //********************************************************************************************************VLAD CODE
        //********************************************************************************************************

        private void process_router_start(string message, string reply, string subject)
        {
            if (shutting_down)
            {
                return;
            }

            Logger.debug(String.Format("DEA received router start message: {0}", message));

            foreach (Droplet droplet in droplets)
            {
                foreach (DropletInstance instance in droplet.Instances)
                {
                    if (instance.State == DropletInstanceState.RUNNING)
                    {
                        register_instance_with_router(instance);
                    }
                }
            }
        }

        private void process_healthmanager_start(string message, string reply, string subject)
        {
            if (shutting_down)
            {
                return;
            }
            Logger.debug(String.Format("DEA received healthmanager start message: {0}", message));

            send_heartbeat();
        }

        private void schedule_snapshot()
        {
            if (snapshot_scheduled)
            {
                return;
            }
            snapshot_scheduled = true;

            ThreadPool.QueueUserWorkItem(delegate(object data)
            {
                snapshot_app_state();
            });
        }

        //refactor: stefi: put this into DropletCollection.SnapshotAppState
        private void snapshot_app_state()
        {
            DateTime start = DateTime.Now;

            string tmpFilename = String.Format("{0}/snap_{1}", db_dir, DateTime.Now.Ticks);

            File.WriteAllText(tmpFilename, droplets.ToJson().ToString());

            if (File.Exists(app_state_file))
            {
                File.Delete(app_state_file);
            }

            File.Move(tmpFilename, app_state_file);

            Logger.debug(String.Format("Took {0} to snapshot application state.", DateTime.Now - start));

            snapshot_scheduled = false;
        }

        private void recover_existing_droplets()
        {
            if (!File.Exists(app_state_file))
            {
                recovered_droplets = true;
                return;
            }

            Dictionary<string, object> appsInfo = new Dictionary<string, object>();
            appsInfo = appsInfo.FromJson(File.ReadAllText(app_state_file));

            // Whip through and reconstruct droplet_ids and instance symbols correctly for droplets, state, etc..
            foreach (string jDropletId in appsInfo.Keys)
            {
                int droplet_id = Convert.ToInt32(jDropletId);
                Droplet droplet = new Droplet(droplet_id);
                droplets.Add(droplet);

                Dictionary<string, object> instances = appsInfo[droplet_id.ToString()].ToObject<Dictionary<string, object>>();

                foreach (string instanceId in instances.Keys)
                {
                    Dictionary<string, object> instance = instances[instanceId].ToObject<Dictionary<string, object>>();
                    DropletInstance dropletInstance = new DropletInstance(instanceId);
                    dropletInstance.FromDictionary(instance);
                    dropletInstance.State = (DropletInstanceState)Enum.Parse(typeof(DropletInstanceState), instance["state"].ToValue<string>(), true);
                    dropletInstance.ExitReason = (DropletExitReason)Enum.Parse(typeof(DropletExitReason), instance["exit_reason"].ToValue<string>(), true);
                    dropletInstance.Orphaned = true;
                    DateTime startTime = DateTime.MinValue;
                    dropletInstance.Start = DateTime.TryParse(instance["start"].ToValue<string>(), out startTime) ? startTime : DateTime.Now;
                    
                    // Assume they are running until we know different..
                    // Accounting is done here so we do not run ahead with the defers.
                    dropletInstance.ResourcesTracked = false;
                    add_instance_resources(dropletInstance);
                    dropletInstance.StopProcessed = false;
                    droplet.Instances.Add(dropletInstance);
                }
            }

            recovered_droplets = true;

            if (num_clients > 0)
            {
                Logger.info(String.Format("DEA recovered {0} applications", num_clients));
            }

            monitor_apps(true);
            send_heartbeat();
            schedule_snapshot();
        }

        // Removes any instance dirs without a corresponding instance entry in @droplets
        // NB: This is run once at startup, so not using EM.system to perform the rm is fine.
        private void delete_untracked_instance_dirs()
        {
            List<string> tracked_instance_dirs = new List<string>();

            foreach (Droplet droplet in droplets)
            {
                foreach (DropletInstance instance in droplet.Instances)
                {
                    tracked_instance_dirs.Add(instance.Dir);
                }
            }

            // todo: vladi: this must be completed with cleaning up IIS sites
            List<string> all_instance_dirs = Directory.GetDirectories(apps_dir, "*", SearchOption.TopDirectoryOnly).ToList();

            List<string> to_remove = (from dir in all_instance_dirs
                                      where !tracked_instance_dirs.Contains(dir)
                                      select dir).ToList();

            foreach (string dir in to_remove)
            {
                Logger.warn(String.Format("Removing instance '{0}', doesn't correspond to any instance entry.", dir));
                try
                {
                    //Clean up the instance, including the IIS Web Site and the Windows User Accoung
                    //Utils.ExecuteCommand(String.Format("netiis -cleanup={0}", dir));
                    Directory.Delete(dir, true);
                }
                catch(Exception e)
                {
                    Logger.warn(String.Format("Cloud not remove instance: {0}, error: {1}", dir, e.ToString()));
                }
            }

        }

        private void add_instance_resources(DropletInstance instance)
        {
            if (instance.ResourcesTracked)
            {
                return;
            }
            instance.ResourcesTracked = true;
            reserved_mem += instance_mem_usage_in_mb(instance);
            num_clients++;
        }

        private void remove_instance_resources(DropletInstance instance)
        {
            if (!instance.ResourcesTracked)
            {
                return;
            }

            instance.ResourcesTracked = false;
            reserved_mem -= instance_mem_usage_in_mb(instance);
            num_clients--;
        }

        private int instance_mem_usage_in_mb(DropletInstance instance)
        {
            return (instance.MemQuota / (1024 * 1024));
        }

        private int grab_ephemeral_port()
        {
            TcpListener socket = new TcpListener(IPAddress.Any, 0);
            socket.Start();
            int port = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();
            return port;
        }

        private void detect_app_ready(DropletInstance instance, Dictionary<string, string> manifest, BoolStateBlockDelegate block)
        {
            string state_file = manifest.ContainsKey("state_file") ? manifest["state_file"] : null;
            if (state_file != null && state_file != String.Empty)
            {
                state_file = Path.Combine(instance.Dir, state_file);
                detect_state_ready(instance, state_file, block);
            }
            else
            {
                detect_port_ready(instance, block);
            }
        }

        private void detect_state_ready(DropletInstance instance, string state_file, BoolStateBlockDelegate block)
        {
            int attempts = 0;
            System.Timers.Timer timer = new System.Timers.Timer(500);
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
                {
                    Dictionary<string, object> state = new Dictionary<string,object>();
                    try
                    {
                        if (File.Exists(state_file))
                        {
                            state = state.FromJson(File.ReadAllText(state_file));
                        }
                    }
                    catch
                    {
                    }
                    if (state != null && state["state"].ToValue<string>() == "RUNNING")
                    {
                        block(true);
                        timer.Enabled = false;
                    }
                    else if (instance.DebugMode != "suspend")
                    {
                        attempts++;
                        if (attempts > 600 || instance.State != DropletInstanceState.STARTING)
                        {
                            block(false);
                            timer.Enabled = false;
                        }
                    }
                });
            timer.Enabled = true;
        }

        private void detect_port_ready(DropletInstance instance, BoolStateBlockDelegate block)
        {
            //todo: vladi: make sure this code does not break anything of what the linux version does
            int port = instance.Port;

            int attempts = 0;
            bool keep_going = true;
            while (attempts <= 120 && instance.State == DropletInstanceState.STARTING && keep_going == true)
            {
                AutoResetEvent connectedEvent = new AutoResetEvent(false);

                TcpClient client = new TcpClient();
                IAsyncResult result = client.BeginConnect(local_ip, port, null, null);
                result.AsyncWaitHandle.WaitOne(250);

                if (client.Connected)
                {
                    client.Close();
                    keep_going = false;
                    block(true);
                }
                else
                {
                    client.Close();
                }
                Thread.Sleep(500);
                attempts++;
            }

            if (keep_going)
            {
                block(false);
            }
        }

        //refactor: replace this with Stager.DownloadAppBits
        private ManualResetEvent download_app_bits(string bits_uri, string sha1, string tgz_file)
        {
            //  todo: vladi: make sure the new method of downloading is ok
            lock (downloadsPendingLock)
            {
                if (downloads_pending.ContainsKey(sha1))
                {
                    return downloads_pending[sha1];
                }

                downloads_pending[sha1] = new ManualResetEvent(false);
            }
            WebClient client = new WebClient();
            string pending_tgz_file = Path.Combine(staged_dir, String.Format("{0}.pending", sha1));
            client.DownloadFile(bits_uri, pending_tgz_file);
            File.Move(pending_tgz_file, tgz_file);
            lock (downloadsPendingLock)
            {
                downloads_pending[sha1].Set();
                downloads_pending.Remove(sha1);
            }
            return null;
        }

        // Be conservative here..
        //refactor: stefi: replace this with Stager.BindLocalRuntime
        private void bind_local_runtime(string instance_dir, string runtime_name)
        {
            if (String.IsNullOrEmpty(instance_dir) || String.IsNullOrEmpty(runtime_name) || !runtime_supported(runtime_name))
            {
                return;
            }

            DeaRuntime runtime = runtimes[runtime_name];

            string startup = Path.GetFullPath(Path.Combine(instance_dir, "startup"));

            if (!File.Exists(startup))
            {
                return;
            }

            string startup_contents = File.ReadAllText(startup);
            string new_startup = startup_contents.Replace("%VCAP_LOCAL_RUNTIME%", runtime.Executable);

            if (String.IsNullOrEmpty(new_startup))
            {
                return;
            }

            File.WriteAllText(startup, new_startup);

            // TODO: vladi: make sure functionality/security is not broken because of this
            //FileUtils.chmod(0600, startup)
            //File.open(startup, 'w') { |f| f.write(new_startup) }
            //FileUtils.chmod(0500, startup)
        }

        //refactor: stefi: put this into Stager.StageAppDirectory
        private bool stage_app_dir(string bits_file, string bits_uri, string sha1, string tgz_file, string instance_dir, string runtime)
        {
            // See if we have bits first..
            // What we do here, in order of preference..
            // 1. Check our own staged directory.
            // 2. Check shared directory from CloudController that could be mounted (bits_file)
            // 3. Pull from http if needed.

            try
            {

                if (File.Exists(tgz_file))
                {
                    Logger.debug("Found staged bits in local cache.");
                }
                else
                {
                    //  If we have a shared volume from the CloudController we can see the bits
                    //  directly, just link into our staged version.

                    if (File.Exists(bits_file) && !force_http_sharing)
                    {
                        //todo: stefi: make this thread safe
                        Logger.debug("Sharing cloud controller's staging directories");
                        DateTime start = DateTime.Now;
                        File.Copy(bits_file, tgz_file);
                        Logger.debug(String.Format("Took {0} to copy from shared directory", DateTime.Now - start));
                    }
                    else
                    {
                        DateTime start = DateTime.Now;
                        Logger.debug(String.Format("Need to download app bits from {0}", bits_uri));

                        // We need to download the bits here, so we need to make sure everyone
                        // else looking for same bits gets in line..

                        ManualResetEvent pending = download_app_bits(bits_uri, sha1, tgz_file);

                        if (pending != null)
                        {
                            Logger.debug("Waiting on another download already in progress");
                            pending.WaitOne();
                        }
                        else
                        {
                            //unzil the file

                        }

                        DateTime download_end = DateTime.Now;
                        Logger.debug(String.Format("Took {0} to download and write file", download_end - start));
                    }
                }

                DateTime startStage = DateTime.Now;

                // Explode the app into its directory and optionally bind its
                // local runtime.
                Directory.CreateDirectory(instance_dir);
                // TODO: vladi: make sure this unpacks correctly


                try
                {
                    lock (unpacksPendingLock)
                    {
                        if (unpacks_pending.ContainsKey(sha1))
                        {
                            unpacks_pending[sha1]++;

                        }
                        else
                        {
                            unpacks_pending[sha1] = 1;
                        }
                    }

                    string tarFileName = Path.GetFileName(tgz_file);
                    tarFileName = Path.ChangeExtension(tarFileName, ".tar");
                    //first unzip
                    Utils.UnZipFile(instance_dir, tgz_file);

                    //then untar
                    Utils.UnZipFile(instance_dir, Path.Combine(instance_dir, tarFileName));

                    //delete the tar
                    File.Delete(Path.Combine(instance_dir, tarFileName));

                }
                catch (Exception ex)
                {
                    Logger.warn(String.Format("Problems staging file {0}, {1}", tgz_file, ex.ToString()));
                }

                //Only remove tgz file if there's no one using it.
                lock (unpacksPendingLock)
                {
                    if (unpacks_pending.ContainsKey(sha1))
                    {
                        unpacks_pending[sha1]--;
                    }
                    else
                    {
                        unpacks_pending[sha1] = 0;
                    }

                    if (unpacks_pending[sha1] == 0)
                    {
                        try
                        {
                            // Removed the staged bits
                            if (!disable_dir_cleanup)
                            {
                                File.Delete(tgz_file);

                            }
                        }
                        finally
                        {
                            unpacks_pending.Remove(sha1);
                        }
                    }
                }

                bind_local_runtime(instance_dir, runtime);

                Logger.debug(String.Format("Took {0} to stage the app directory", DateTime.Now - startStage));

            }
            catch(Exception e)
            {
                Logger.warn(String.Format("Unable to stage app", e.ToString()));
                return false;
            }

            return true;
        }

        // The format used by VCAP_SERVICES
        //refactor: replace with DropletInstance.CreateServicesForEnvironment
        private string create_services_for_env(List<Dictionary<string, object>> services = null)
        {
            List<string> whitelist = new List<string>() {"name", "label", "plan", "tags", "plan_option", "credentials"};
            Dictionary<string, List<Dictionary<string, object>>> svcs_hash = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (Dictionary<string, object> service in services)
            {
                string label = service["label"].ToString();
                if (!svcs_hash.ContainsKey(label))
                {
                    svcs_hash[label] = new List<Dictionary<string, object>>();
                }
                Dictionary<string, object> svc_hash = new Dictionary<string, object>();

                foreach (string key in whitelist)
                {
                    if (service[key] != null)
                    {
                        svc_hash[key] = service[key];
                    }
                }

                svcs_hash[label].Add(svc_hash);
            }

            return svcs_hash.ToJson();
        }

        // The format used by VMC_SERVICES
        //refactor: replace with DropletInstance.CreateLegacyServicesForEnvironment
        private string create_legacy_services_for_env(List<Dictionary<string, object>> services = null)
        {
            List<string> whitelist = new List<string>() { "name", "type", "vendor", "version" };

            List<Dictionary<string, object>> as_legacy = new List<Dictionary<string, object>>();

            foreach (Dictionary<string, object> svc in services)
            {
                Dictionary<string, object> leg_svc = new Dictionary<string, object>();
                foreach (string key in whitelist)
                {
                    if (svc.ContainsKey(key))
                    {
                        leg_svc[key] = svc[key];
                    }
                }
                leg_svc["tier"] = svc["plan"];
                leg_svc["options"] = svc["credentials"];

                as_legacy.Add(leg_svc);
            }
            return as_legacy.ToJson();
        }

        // The format used by VCAP_APP_INSTANCE
        //refactor: replace with DropletInstance.CreateInstanceForEnvironment
        private string create_instance_for_env(DropletInstance instance)
        {
            List<string> whitelist = new List<string>() { "instance_id", "instance_index", "name", "uris", "users", "version", "start", "runtime", "state_timestamp", "port" };
            Dictionary<string, object> env_hash = new Dictionary<string, object>();

            Dictionary<string, object> jInstance = instance.ToDictionary();

            foreach (string key in whitelist)
            {
                if (jInstance[key] != null)
                {
                    env_hash[key] = jInstance[key].ToObject<object>();
                }
            }

            env_hash["limits"] = new Dictionary<string, object>() {
                {"fds", instance.FdsQuota},
                {"mem", instance.MemQuota},
                {"disk", instance.DiskQuota}
            };

            env_hash["host"] = local_ip;

            return env_hash.ToJson();
        }

        //refactor: stefi: replace with DropletInstance.CreateDebugForEnvironment
        private List<string> debug_env(DropletInstance instance)
        {
            if (instance.DebugPort == 0)
            {
                return new List<string>();
            }
            if (runtimes[instance.Runtime].DebugEnv == null)
            {
                return new List<string>();
            }

            return runtimes[instance.Runtime].DebugEnv[instance.DebugMode];
        }


        //refactor: stefi: put this into DropletInstance.SetupInstanceEnvironment
        private List<string> setup_instance_env(DropletInstance instance, List<string> app_env, List<Dictionary<string, object>> services)
        {
            List<string> env = new List<string>();            

            env.Add(String.Format("HOME={0}", instance.Dir));
            env.Add(String.Format("VCAP_APPLICATION='{0}'", create_instance_for_env(instance)));
            env.Add(String.Format("VCAP_SERVICES='{0}'", create_services_for_env(services)));
            env.Add(String.Format("VCAP_APP_HOST='{0}'", local_ip));
            env.Add(String.Format("VCAP_APP_PORT='{0}'", instance.Port));
            env.Add(String.Format("VCAP_DEBUG_IP='{0}'", instance.DebugIp));
            env.Add(String.Format("VCAP_DEBUG_PORT='{0}'", instance.DebugPort));

            List<string> vars;
            if ((vars = debug_env(instance)).Count > 0)
            {
                Logger.info(String.Format("Debugger environment variables: {0}", String.Join("\r\n", vars.ToArray())));
                env.AddRange(vars);
            }

            // LEGACY STUFF
            env.Add(String.Format("VMC_WARNING_WARNING='All VMC_* environment variables are deprecated, please use VCAP_* versions.'"));
            env.Add(String.Format("VMC_SERVICES='{0}'", create_legacy_services_for_env(services)));
            env.Add(String.Format("VMC_APP_INSTANCE='{0}'", instance.ToJson()));
            env.Add(String.Format("VMC_APP_NAME='{0}'", instance.Name));
            env.Add(String.Format("VMC_APP_ID='{0}'", instance.InstanceId));
            env.Add(String.Format("VMC_APP_VERSION='{0}'", instance.Version));
            env.Add(String.Format("VMC_APP_HOST='{0}'", local_ip));
            env.Add(String.Format("VMC_APP_PORT='{0}'", instance.Port));

            foreach (Dictionary<string, object> service in services)
            {
                string hostname = string.Empty;

                Dictionary<string, object> serviceCredentials = service["credentials"].ToObject<Dictionary<string, object>>();

                if (serviceCredentials.ContainsKey("hostname"))
                {
                    hostname = serviceCredentials["hostname"].ToValue<string>();
                }
                else if (serviceCredentials.ContainsKey("host"))
                {
                    hostname = serviceCredentials["host"].ToValue<string>();
                }

                string port = serviceCredentials["port"].ToValue<string>();

                if (!String.IsNullOrEmpty(hostname) && !String.IsNullOrEmpty(port))
                {
                    env.Add(String.Format("VMC_{0}={1}:{2}", service["vendor"].ToString().ToUpper(), hostname, port));
                }
            }

            // Do the runtime environment settings
            foreach (string runtimeEnv in runtime_env(instance.Runtime))
            {
                env.Add(runtimeEnv);
            }

            // User's environment settings
            // Make sure user's env variables are in double quotes.
            if (app_env != null)
            {
                foreach (string appEnv in app_env)
                {
                    string[] pieces = appEnv.Split(new char[] { '=' }, 2);
                    if (!pieces[1].StartsWith("'"))
                    {
                        pieces[1] = String.Format("\"{0}\"", pieces[1]);
                    }
                    env.Add(String.Format("{0}={1}", pieces[0], pieces[1]));
                }
            }

            return env;
        }

        private void evacuate_apps_then_quit()
        {
            shutting_down = true;

            Logger.info("Evacuating applications..");

            foreach (Droplet droplet in droplets)
            {
                Logger.debug(String.Format("Evacuating app {0}", droplet.DropletId));
                foreach (DropletInstance instance in droplet.Instances)
                {
                    if (instance.State == DropletInstanceState.CRASHED)
                    {
                        continue;
                    }
                    instance.ExitReason = DropletExitReason.DEA_EVACUATION;
                    send_exited_notification(instance);
                    instance.Evacuated = true;
                }
            }

            Logger.info(String.Format("Scheduling shutdown in {0} seconds..", evacuation_delay));

            evacuation_delay_timer = new System.Timers.Timer(evacuation_delay);
            evacuation_delay_timer.AutoReset = false;
            
            evacuation_delay_timer.Elapsed +=new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
                {
                    Shutdown();
                });

            evacuation_delay_timer.Enabled = true;

            schedule_snapshot();
        }

        public void Shutdown()
        {
            shutting_down = true;
            Logger.info("Shutting down..");

            foreach (Droplet droplet in droplets)
            {
                Logger.debug(String.Format("Stopping app {0}", droplet.DropletId));
                foreach (DropletInstance instance in droplet.Instances)
                {
                    // skip any crashed instances
                    if (instance.State != DropletInstanceState.CRASHED)
                    {
                        instance.ExitReason = DropletExitReason.DEA_SHUTDOWN;
                    }
                    stop_droplet(instance);
                }
            }

            // Allows messages to get out.
            System.Timers.Timer timer = new System.Timers.Timer(250);
            timer.AutoReset = false;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
                {
                    snapshot_app_state();
                    file_viewer_server.Stop();
                    Logger.info("Bye..");
                });
        }

        //refactor: stefi: replace with DropletInstance.IsRunning
        private bool instance_running(DropletInstance instance)
        {
            if (instance == null || instance.Pid == 0)
            {
                return false;
            }

            //todo: vladi:make sure this is ok`ps -o rss= -p #{instance[:pid]}`.length > 0
            return ProcessInformation.GetProcessInformation(instance.Pid).Length == 1;
        }
            

        //refactor: stefi: replace with DropletInstance.StopDroplet
        private void stop_droplet(DropletInstance instance)
        {
            // On stop from cloud controller, this can get called twice. Just make sure we are re-entrant..
            if (instance.StopProcessed)
            {
                return;
            }

            // Unplug us from the system immediately, both the routers and health managers.
            send_exited_message(instance);

            Logger.info(String.Format("Stopping instance {0}", instance.LogId));

            // if system thinks this process is running, make sure to execute stop script

            if (instance.Pid != 0 || instance.State == DropletInstanceState.STARTING || instance.State == DropletInstanceState.RUNNING)
            {
                if (instance.State != DropletInstanceState.CRASHED)
                {
                    instance.State = DropletInstanceState.STOPPED;
                }
                instance.StateTimestamp = DateTime.Now;

                // todo: vladi: make sure the new windows code doesn't break any functionality

                string execute_cmd = "cmd";

                StreamWriterDelegate stopOperation = delegate(StreamWriter stdin)
                {
                    stdin.WriteLine(String.Format("cd /D {0}", instance.Dir));
                    stdin.WriteLine("copy .\\stop .\\stop.ps1");
                    stdin.WriteLine("powershell.exe -ExecutionPolicy Bypass -InputFormat None -noninteractive -file .\\stop.ps1");
                    stdin.WriteLine("exit");
                };

                ProcessDoneDelegate exitOperation = delegate(string output, int status)
                {
                    Logger.info(String.Format("Stop operation completed running with status = {0}.", status));
                    Logger.info(String.Format("Stop operation std output is: {0}", output));

                    // Cleanup resource usage and files..
                    cleanup_droplet(instance);
                };

                //TODO: vladi: this must be done with clean environment variables
                Utils.ExecuteCommands(execute_cmd, "", stopOperation, exitOperation);
            }

            // Mark that we have processed the stop command.
            instance.StopProcessed = true;
   
        }

        private void cleanup_droplet(DropletInstance instance)
        {
            // Drop usage and resource tracking regardless of state
            remove_instance_resources(instance);

            if (instance.Pid != 0)
            {
                usage.Remove(instance.Pid);
            }

            // clean up the in memory instance and directory only if the instance didn't crash
            if (instance.State != DropletInstanceState.CRASHED)
            {
                if (droplets.DropletExists(instance.DropletId))
                {
                    droplets[instance.DropletId].Instances.Remove(instance);
                    if (droplets[instance.DropletId].Instances.Count == 0)
                    {
                        droplets.Delete(instance.DropletId);
                    }
                    schedule_snapshot();
                }

                if (!disable_dir_cleanup)
                {
                    //Logger.debug(String.Format("{0}: Cleaning up dir {1}", instance.Name, instance.Dir));

                    //Do some attemts then so that the file and directory handels are released
                    

                    for(int retryAttempts = 5; retryAttempts > 0 ; retryAttempts--)
                    {
                        try
                        {
                            Directory.Delete(instance.Dir, true);
                            Logger.debug(String.Format("{0}: Cleand up dir {1}", instance.Name, instance.Dir));
                            break;
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Logger.warn(String.Format("Unable to delete direcotry {0}, UnauthorizedAccessException: {1}", instance.Dir, e.ToString()));
                            Thread.Sleep(100);
                        }
                        catch (Exception e)
                        {
                            Logger.warn(String.Format("Unable to delete direcotry {0}, Exception: {1}", instance.Dir, e.ToString()));
                            break;
                        }
                    }
                    


                    
                }
            }
        }

        //refactor: stefi: replace with DropletInstance.GenerateRouterRegisterMessage
        private void register_instance_with_router(DropletInstance instance, Dictionary<string, object> options = null)
        {
            if (instance == null)
            {
                return;
            }
            lock (instance.CollectionsLock)
            {
                if (instance.Uris.Count == 0)
                {
                    return;
                }
            }

            natsClient.Publish("router.register", msg: new Dictionary<string, object>()
                {
                           {"dea", vcapComponent.Uuid},
                           {"host", local_ip},
                           {"port", instance.Port},
                           {"uris", options != null && options.ContainsKey("uris") ? options["uris"] : ((DropletInstance)instance.Clone()).Uris},
                           {"tags", new Dictionary<string, object>() {
                                        {"framework", instance.Framework},
                                        {"runtime", instance.Runtime}
                           }}
                }.ToJson());
        }

        //refactor: stefi: replace with DropletInstance.GenerateRouterUnregisterMessage and DeaPublisher.SendRouterUnregister
        private void unregister_instance_from_router(DropletInstance instance, Dictionary<string, object> options = null)
        {
            if (instance == null)
            {
                return;
            }
            lock (instance.CollectionsLock)
            {
                if (instance.Uris.Count == 0)
                {
                    return;
                }
            }

            natsClient.Publish("router.unregister", msg: new Dictionary<string, object>()
                {
                           {"dea", vcapComponent.Uuid},
                           {"host", local_ip},
                           {"port", instance.Port},
                           {"uris", options != null && options.ContainsKey("uris") ? options["uris"] : ((DropletInstance)instance.Clone()).Uris}                           
                }.ToJson());
        }

        
        //refactor: stefi: replace with DropletInstance.GenerateDropletExitedMessage and DeaPublisher.SendDropletExited
        private void send_exited_notification(DropletInstance instance)
        {
            if (instance.Evacuated)
            {
                return;
            }

            Dictionary<string, object> exit_message = new Dictionary<string, object>()
            {
                {"droplet", instance.DropletId},
                {"version", instance.Version},
                {"instance", instance.InstanceId},
                {"index", instance.InstanceIndex},
                {"reason", instance.ExitReason.ToString()},
            };

            if (instance.State == DropletInstanceState.CRASHED)
            {
                exit_message["crash_timestamp"] = Utils.DateTimeToEpochSeconds(instance.StateTimestamp);
            }

            natsClient.Publish("droplet.exited", msg: exit_message.ToJson());

            Logger.debug(String.Format("Sent droplet.exited {0}", exit_message.ToJson()));
        }

        //refactor: put into Agent.StopDroplet code
        private void send_exited_message(DropletInstance instance)
        {
            if (instance.Notified)
            {
                return;
            }

            unregister_instance_from_router(instance);

            if (instance.ExitReason == null)
            {
                instance.ExitReason = DropletExitReason.CRASHED;
                instance.State = DropletInstanceState.CRASHED;
                instance.StateTimestamp = DateTime.Now;
                if (!instance_running(instance))
                {
                    instance.Pid = 0;
                }
            }

            send_exited_notification(instance);
            instance.Notified = true;
        }

        //refactor: stefi: put into DropletInstance.DetectAppPid
        private int detect_app_pid(string dir)
        {
            int detect_attempts = 0;
            int pid = 0;

            AutoResetEvent resetEvent = new AutoResetEvent(false);

            System.Timers.Timer detect_pid_timer = new System.Timers.Timer(1000);
            detect_pid_timer.AutoReset = true;
            detect_pid_timer.Elapsed +=new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
                {
                    string pid_file = Path.Combine(dir, "run.pid");
                    if (File.Exists(pid_file))
                    {
                        detect_pid_timer.Enabled = false;
                        pid = Convert.ToInt32(File.ReadAllText(pid_file));
                        resetEvent.Set();
                    }
                    else
                    {
                        detect_attempts++;
                        if (detect_attempts > 300)
                        {
                            Logger.debug("Giving up detecting stop file");
                            detect_pid_timer.Enabled = false;
                            resetEvent.Set();
                        }
                    }
                });
            detect_pid_timer.Enabled = true;

            resetEvent.WaitOne();
            return pid;
        }


        //refactor: stefi: replace to DropletCollection.HasMonitorableApps
        private bool no_monitorable_apps()
        {
            if (droplets.Count == 0)
            {
                return true;
            }

            // If we are here we have droplets, but we need to make sure that we have ones we feel are starting or running.
            foreach (Droplet droplet in droplets)
            {
                foreach (DropletInstance instance in droplet.Instances)
                {
                    if (instance.State == DropletInstanceState.STARTING || instance.State == DropletInstanceState.RUNNING)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // This is only called when in secure mode, cur_usage is in kb, quota is in bytes.
        private void check_usage(DropletInstance instance, Dictionary<string, long> usage, object history)
        {
            if (instance == null || usage == null)
            {
                return;
            }

            // Check Mem
            if (usage["mem"] > (instance.MemQuota / 1024))
            {
                
                Logger logger = new Logger(Path.Combine(instance.Dir, "logs\\err.log"));

                logger.ffatal(String.Format("Memory limit of {0}M exceeded.", instance.MemQuota / 1024 / 1024));
                logger.ffatal(String.Format("Actual usage was {0}M, process terminated.", usage["mem"] / 1024));
                stop_droplet(instance);
            }

            // Check Disk
            if (usage["disk"] > instance.DiskQuota)
            {
                Logger logger = new Logger(Path.Combine(instance.Dir, "logs\\err.log"));
                logger.ffatal(String.Format("Disk usage limit of {0}M exceeded.", instance.DiskQuota / 1024 / 1024));
                logger.ffatal(String.Format("Actual usage was {0}M, process terminated.", usage["disk"] / 1024 / 1024));
                stop_droplet(instance);
            }

            // Check CPU
            if (history == null)
            {
                return;
            }

            if (usage["cpu"] > BEGIN_RENICE_CPU_THRESHOLD)
            {
                int nice = instance.Nice + 1;
                if (nice < MAX_RENICE_VALUE)
                {
                    instance.Nice = nice;
                    ProcessPriorityClass priority = nice == 0 ? ProcessPriorityClass.RealTime : nice == 1 ? ProcessPriorityClass.High :
                        nice == 2 ? ProcessPriorityClass.AboveNormal : nice == 3 ? ProcessPriorityClass.Normal : nice == 4 ? ProcessPriorityClass.BelowNormal : ProcessPriorityClass.Idle;

                    Logger.info(String.Format("Lowering priority on CPU bound process({0}), new value:{1}", instance.Name, priority));

                    //TODO: vladi: make sure this works on Windows
                    Process.GetProcessById(instance.Pid).PriorityClass = priority;
                }
            }
            // TODO, Check for an attack, or what looks like one, and look at history?
            // pegged_cpus = @num_cores * 100
        }

        private void grab_deleted_file_usage(string username)
        {
            //todo: vladi: this has not be migrated
            //user = find_secure_user(username)
            //return unless @secure && user
            //# Disabled for now on MacOS, where uid is set to -1 in secure mode
            //uid = user[:uid]
            //if uid && uid.to_i >= 0
            //  files = %x[lsof -nwu #{uid} -s -l].split("\n")
            //else
            //  files = []
            //end
            //disk = 0
            //files.each do |file|
            //  parts = file.split(/\s+/)
            //  next unless parts[DELETED_INDEX] && parts[DELETED_INDEX] =~ /deleted/i
            //  next unless (parts[TYPE_INDEX] =~ /REG/ && parts[FD_INDEX] =~ /\d+[rwu]?/)
            //  disk += parts[SIZE_INDEX].to_i
            //end
            //return disk
        }

        //refactor: put thist into DropletCollectio.CrashesReaper
        private void crashes_reaper()
        {
            foreach (Droplet droplet in droplets)
            {
                // delete all crashed instances that are older than an hour
                List<DropletInstance> toDelete = (from instance in droplet.Instances
                                                  where instance.State == DropletInstanceState.CRASHED && (DateTime.Now - instance.StateTimestamp).TotalSeconds > CRASHES_REAPER_TIMEOUT
                                                  select instance).ToList();
                foreach (DropletInstance instance in toDelete)
                {
                    Logger.debug(String.Format("Crashes reaper deleted: {0}", instance.InstanceId));
                    if (!disable_dir_cleanup)
                    {
                        Directory.Delete(instance.Dir, true);
                    }
                }
            }

            foreach (int dropletId in (from droplet in droplets where droplet.Count == 0 select droplet.DropletId))
            {
                droplets.Delete(dropletId);
            }
        }

        // monitor the running applications
        //refactor: stefi: put this into DropletCollection.MonitorApps
        private void monitor_apps(bool startup_check = false)
        {
            // Always reset
            mem_usage = 0;
            List<object> runningApps = null;
            vcapComponent.varz["running_apps"] = runningApps;

            if (no_monitorable_apps() && !startup_check)
            {
                System.Timers.Timer monitorTimer = new System.Timers.Timer(MONITOR_INTERVAL);
                monitorTimer.AutoReset = false;
                monitorTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
                    {
                        monitor_apps(false);
                    });
                monitorTimer.Enabled = true;
                return;
            }

            Dictionary<int, ProcessInformationEntry> pid_info = new Dictionary<int, ProcessInformationEntry>();
            Dictionary<object, object> user_info = new Dictionary<object, object>();

            DateTime start = DateTime.Now;

            // BSD style ps invocation
            DateTime ps_start = DateTime.Now;

            ProcessInformationEntry[] processStatuses = ProcessInformation.GetProcessInformation(0);

            TimeSpan ps_elapsed = DateTime.Now - ps_start;
            if (ps_elapsed.TotalMilliseconds > 800)
            {
                Logger.warn(String.Format("Took {0}s to execute ps. ({1} entries returned)", ps_elapsed.TotalSeconds, processStatuses.Length));
            }

            foreach (ProcessInformationEntry processStatus in processStatuses)
            {
                int pid = processStatus.ProcessId;
                pid_info[pid] = processStatus;

                // TODO: vladi: security is handled by netiis, figure out if that needs to change
                // (user_info[parts[USER_INDEX]] ||= []) << parts if (@secure && parts[USER_INDEX] =~ SECURE_USER)
            }

            // This really, really needs refactoring, but seems like the least intrusive/failure-prone way
            // of making the du non-blocking in all but the startup case...
            DateTime du_start = DateTime.Now;
            if (startup_check)
            {
                DiskUsageEntry[] entries = DiskUsage.GetDiskUsage(apps_dir, "*", true);
                monitor_apps_helper(startup_check, start, du_start, entries, pid_info, user_info);
            }
            else
            {
                //todo: vladi: make this asynchronous
                DiskUsageEntry[] entries = DiskUsage.GetDiskUsage(apps_dir, "*", true);
                monitor_apps_helper(startup_check, start, du_start, entries, pid_info, user_info);
            }
        }

        private void monitor_apps_helper(bool startup_check, DateTime ma_start, DateTime du_start, DiskUsageEntry[] du_all_out, Dictionary<int, ProcessInformationEntry> pid_info, Dictionary<object, object> user_info)
        {
            List<DropletInstance> running_apps = new List<DropletInstance>();

            // Do disk summary
            Dictionary<string, int> du_hash = new Dictionary<string,int>();
            TimeSpan du_elapsed = DateTime.Now - du_start;

            if (du_elapsed.TotalMilliseconds > 800)
            {
                Logger.warn(String.Format("Took {0}s to execute du.", du_elapsed.TotalSeconds));
                if ((du_elapsed.TotalSeconds > 10) && ((DateTime.Now - last_apps_dump).TotalSeconds > APPS_DUMP_INTERVAL))
                {
                    dump_apps_dir();
                    last_apps_dump = DateTime.Now;
                }
            }

            foreach (DiskUsageEntry du_entry in du_all_out)
            {
                int size = Convert.ToInt32(du_entry.Size) * 1024;
                du_hash[du_entry.Directory] = size;
            }

            Dictionary<string, Dictionary<string, Dictionary<string, int>>> metrics = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>() 
            {
                {"framework", new Dictionary<string, Dictionary<string, int>>()}, 
                {"runtime", new Dictionary<string, Dictionary<string, int>>()}
            };

            foreach (Droplet droplet in droplets)
            {
                foreach (DropletInstance instance in droplet.Instances)
                {
                    if (instance.Pid != 0 && pid_info.ContainsKey(instance.Pid))
                    {
                        int pid = instance.Pid;
                        int mem = 0;
                        int cpu = 0;

                        int disk = du_hash.ContainsKey(Path.GetDirectoryName(instance.Dir)) ? du_hash[Path.GetDirectoryName(instance.Dir)] : 0;

                        // TODO: vladi: make sure this can work properly with netiis
                        // For secure mode, gather all stats for secure_user so we can process forks, etc.
                        if (secure && user_info.ContainsKey(instance.SecureUser))
                        {
                            //  user_info[instance[:secure_user]].each do |part|
                            //  mem += part[MEM_INDEX].to_f
                            //  cpu += part[CPU_INDEX].to_f
                            // disabled for now, LSOF is too slow to run per app/user
                            //  deleted_disk = grab_deleted_file_usage(instance[:secure_user])
                            //  disk += deleted_disk
                        }
                        else
                        {
                            mem = Convert.ToInt32(pid_info[pid].WorkingSet);
                            cpu = Convert.ToInt32(pid_info[pid].Cpu);
                        }


                        List<Dictionary<string, long>> lusage = usage.ContainsKey(pid) ? usage[pid] : usage[pid] = new List<Dictionary<string, long>>();

                        Dictionary<string, long> cur_usage = new Dictionary<string, long>() { 
                            {"time", (long)Utils.DateTimeToEpochSeconds(DateTime.Now)},
                            {"cpu", cpu}, 
                            {"mem", mem}, 
                            {"disk", disk}
                        };

                        lusage.Add(cur_usage);
                        if (lusage.Count > MAX_USAGE_SAMPLES)
                        {
                            lusage.RemoveAt(0);
                        }
                        if (secure)
                        {
                            check_usage(instance, cur_usage, lusage);
                        }

                        mem_usage += mem;

                        
                        
                        foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, int>>> kvp in metrics)
                        {
                            Dictionary<string, int> metric = new Dictionary<string, int>();

                            if (kvp.Key == "framework")
                            {
                                if (!metrics.ContainsKey(instance.Framework))
                                {
                                    kvp.Value[instance.Framework] = new Dictionary<string, int>() 
                                    {
                                        {"used_memory", 0},
                                        {"reserved_memory", 0},
                                        {"used_disk", 0},
                                        {"used_cpu", 0}
                                    };
                                }
                                metric = kvp.Value[instance.Framework];
                            }
                            if (kvp.Key == "runtime")
                            {
                                if (!metrics.ContainsKey(instance.Runtime))
                                {
                                    kvp.Value[instance.Runtime] = new Dictionary<string, int>() 
                                    {
                                        {"used_memory", 0},
                                        {"reserved_memory", 0},
                                        {"used_disk", 0},
                                        {"used_cpu", 0}
                                    };
                                }
                                metric = kvp.Value[instance.Runtime];
                            }

                            metric["used_memory"] += mem;
                            metric["reserved_memory"] += instance.MemQuota / 1024;
                            metric["used_disk"] += disk;
                            metric["used_cpu"] += cpu;
                        }

                        // Track running apps for varz tracking
                        DropletInstance instance2 = (DropletInstance)instance.Clone();

                        lock (instance2.CollectionsLock)
                        {
                            instance2.Usage.Clear();

                            foreach (string key in cur_usage.Keys)
                            {
                                instance2.Usage[key] = cur_usage[key];
                            }
                        }

                        running_apps.Add(instance2);

                        // Re-register with router on startup since these are orphaned and may have been dropped.
                        if (startup_check)
                        {
                            //refactoring: stefi: put this after monitor apps
                            register_instance_with_router(instance);
                        }
                    }
                    else
                    {
                        // App *should* no longer be running if we are here
                        instance.Pid = 0;
                        // Check to see if this is an orphan that is no longer running, clean up here if needed
                        // since there will not be a cleanup proc or stop call associated with the instance..
                        if (instance.Orphaned && !instance.StopProcessed)
                        {
                            stop_droplet(instance);
                        }
                    }
                }
            }

            // export running app information to varz
            vcapComponent.varz["running_apps"] = running_apps;
            vcapComponent.varz["frameworks"] = metrics["framework"];
            vcapComponent.varz["runtimes"] = metrics["runtime"];
            TimeSpan ttlog = DateTime.Now - ma_start;
            if (ttlog.TotalMilliseconds > 1000)
            {
                Logger.warn(String.Format("Took {0}s to process ps and du stats", ttlog.TotalSeconds));   
            }
            if (!startup_check)
            {
                System.Timers.Timer monitorTimer = new System.Timers.Timer(MONITOR_INTERVAL);
                monitorTimer.AutoReset = false;
                monitorTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
                {
                    monitor_apps(false);
                });
                monitorTimer.Enabled = true;
            }
        }

        // This is for general access to the file system for the staged droplets.
        //refactor: stefi: go into FileViewer
        private bool start_file_viewer()
        {
            bool success = false;

            try
            {
                string apps_dir = this.apps_dir;
                file_auth = new string[] { Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N") };
                string[] auth = file_auth;

                file_viewer_server = new FileServer(file_viewer_port, apps_dir, "/droplets", file_auth[0], file_auth[1]);
                file_viewer_server.Start();

                Logger.info(String.Format("File service started on port: {0}", file_viewer_port));
                filer_start_attempts += 1;
                return success = true;
            }
            catch (Exception ex)
            {
                Logger.fatal(String.Format("Filer service failed to start: {0} already in use?: {1}", file_viewer_port, ex.ToString()));
                filer_start_attempts += 1;
                if (filer_start_attempts >= 5)
                {
                    Logger.fatal("Giving up on trying to start filer, exiting...");
                    throw new ApplicationException();
                }
            }
            return success;
        }

        //refactor: stefi: Replace With Stager.RuntimeSupportes
        private bool runtime_supported(string runtime_name)
        {
            if (String.IsNullOrEmpty(runtime_name) || !runtimes.ContainsKey(runtime_name))
            {
                Logger.debug(String.Format("Ignoring request, no suitable runtimes available for '{0}'",runtime_name));
                return false;
            }

            if (!runtimes[runtime_name].Enabled)
            {
                Logger.debug(String.Format("Ignoring request, runtime not enabled for '{0}'", runtime_name));
                return false;
            }

            return true;
        }

        //refactor: stefi: replace with Stager.GetRuntimeEnvironment
        private List<string> runtime_env(string runtime_name)
        {
            List<string> env = new List<string>();

            if (!String.IsNullOrEmpty(runtime_name) && runtimes.ContainsKey(runtime_name))
            {
                if (runtimes[runtime_name].Environment != null)
                {
                    foreach (KeyValuePair<string, string> kvp in runtimes[runtime_name].Environment)
                    {
                        env.Add(String.Format("{0}={1}", kvp.Key, kvp.Value));
                    }
                }
            }
            return env;
        }

        // This determines out runtime support.
        //refactor: stefi: Replace with Stager.SetupRuntimes
        private void setup_runtimes()
        {
            if (runtimes == null || runtimes.Count == 0)
            {
                Logger.fatal("Can't determine application runtimes, exiting");
                throw new ApplicationException();
            }
            Logger.info("Checking runtimes:");

            foreach (KeyValuePair<string, DeaRuntime> kvp in runtimes)
            {
                DeaRuntime runtime = kvp.Value;
                string name = kvp.Key;

                //  Only enable when we succeed
                runtime.Enabled = false;

                // Check that we can get a version from the executable
                string version_flag = String.IsNullOrEmpty(runtime.VersionFlag) ? "-v" : runtime.VersionFlag;

                string expanded_exec = Utils.RunCommandAndGetOutput("where", runtime.Executable).Trim();

                expanded_exec = expanded_exec.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    
                if (!File.Exists(expanded_exec))
                {
                    Logger.info(String.Format("{0} FAILED, executable '{1}' not found \r\n Current directory: {2} \r\n Full executable path: {3}", name, runtime.Executable, Directory.GetCurrentDirectory(), expanded_exec));
                    continue;
                }

                // java prints to stderr, so munch them both..
                string version_check = Utils.RunCommandAndGetOutput(expanded_exec, String.Format("{0} 2>&1", expanded_exec, version_flag)).Trim();

                runtime.Executable = expanded_exec;

                if (String.IsNullOrEmpty(runtime.Version))
                {
                    continue;
                }

                // Check the version for a match
                if (new Regex(runtime.Version).IsMatch(version_check))
                {
                    // Additional checks should return true
                    if (!String.IsNullOrEmpty(runtime.AdditionalChecks))
                    {
                        string additional_check = Utils.RunCommandAndGetOutput(runtime.Executable, String.Format("{0} 2>&1", runtime.AdditionalChecks));
                        if (!(new Regex("true").IsMatch(additional_check)))
                        {
                            Logger.info(String.Format("{0} FAILED, additional checks failed", name));
                        }
                    }
                    runtime.Enabled = true;
                    Logger.info(String.Format("{0} OK", name));
                }
                else
                {
                    Logger.info(String.Format("{0} FAILED, version mismatch ({1})", name, version_check));
                }
            }
        }

        // Logs out the directory structure of the apps dir. This produces both a summary
        // (top level view) of the directory, as well as a detailed view.
        //refactor: stefi: go into Monitoring
        private void dump_apps_dir()
        {
            DateTime now = DateTime.Now;
            string tsig = now.ToString("yyyyMMdd_hhmm");
            string summary_file = Path.Combine(apps_dump_dir, String.Format("apps.du.{0}.summary", tsig));
            string details_file = Path.Combine(apps_dump_dir, String.Format("apps.du.{0}.details", tsig));
            // todo: vladi: I removed max depth level (6) from call, because netdu does not support it
            DiskUsage.WriteDiskUsageToFile(summary_file, true, apps_dir, "*", true);
            DiskUsage.WriteDiskUsageToFile(details_file, true, apps_dir, "*", false);
        }

        //refactor: stefi: Put Into Utils
        private void ensure_writable(string dir)
        {
            string test_file = Path.Combine(dir, String.Format("dea.{0}.sentinel", Process.GetCurrentProcess().Id));
            File.WriteAllText(test_file, "");
            File.Delete(test_file);
        }
    }
}
