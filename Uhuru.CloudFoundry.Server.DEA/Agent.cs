using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.CloudFoundry.DEA.Configuration;
using System.IO;
using Uhuru.Utilities;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using Uhuru.Utilities.ProcessPerformance;
using Uhuru.NatsClient;

namespace Uhuru.CloudFoundry.DEA
{
    public class Agent : VcapComponent
    {
        private const decimal Version = 0.99m;


        private DropletCollection Droplets = new DropletCollection();
        private Stager AgentStager = new Stager();

        private FileViewer AgentFileViewer = new FileViewer();
        private Monitoring AgentMonitoring = new Monitoring();

        private bool DisableDirCleanup;
        private bool EnforceUlimit;
        private bool MultiTenant;
        private bool Secure;


        private DeaReactor deaReactor;

        private HelloMessage HelloMessage = new HelloMessage(); 
        //private Dictionary<string, object> HelloMessage;
        private volatile bool ShuttingDown = false;
        private int EvacuationDelayMs = 30 * 1000;

        public Agent()
        {
            

            foreach (Configuration.DEA.RuntimeElement deaConf in UhuruSection.GetSection().DEA.Runtimes)
            {
                DeaRuntime dea = new DeaRuntime();


                dea.Executable = deaConf.Executable;
                dea.Version = deaConf.Version;
                dea.VersionFlag = deaConf.VersionFlag;
                dea.AdditionalChecks = deaConf.AdditionalChecks;
                dea.Enabled = true;

                

                foreach (Configuration.DEA.EnvironmentElement ienv in deaConf.Environment)
                {
                    dea.Environment.Add(ienv.Name, ienv.Value);
                }
                
                foreach (Configuration.DEA.DebugElement debugEnv in deaConf.Debug)
                {
                    dea.DebugEnv.Add(debugEnv.Name, new List<string>());
                    foreach (Configuration.DEA.EnvironmentElement ienv in debugEnv.Environment)
                    {
                        dea.DebugEnv[debugEnv.Name].Add(ienv.Name + "=" + ienv.Value);
                    }   
                    

                }

                
                AgentStager.Runtimes.Add(deaConf.Name, dea);
            }

            AgentStager.DropletDir = UhuruSection.GetSection().DEA.BaseDir;

            EnforceUlimit = UhuruSection.GetSection().DEA.EnforceUlimit;
            DisableDirCleanup = UhuruSection.GetSection().DEA.DisableDirCleanup;
            MultiTenant = UhuruSection.GetSection().DEA.MultiTenant;
            Secure = UhuruSection.GetSection().DEA.Secure;

            AgentMonitoring.MaxMemoryMbytes = UhuruSection.GetSection().DEA.MaxMemory;

            AgentFileViewer.Port = UhuruSection.GetSection().DEA.FilerPort;
            
            AgentStager.ForeHttpFileSharing = UhuruSection.GetSection().DEA.ForceHttpSharing;

            
            Type = "DEA";


            //apps_dump_dir = ConfigurationManager.AppSettings["logFile"] ?? Path.GetTempPath();
            AgentMonitoring.AppsDumpDirectory = Path.GetTempPath();

            
            //heartbeat_interval = UhuruSection.GetSection().DEA.HeartBeatInterval;

            AgentMonitoring.MaxClients = MultiTenant ? Monitoring.DefaultMaxClients : 1;

            AgentStager.StagedDir = Path.Combine(AgentStager.DropletDir, "staged");
            AgentStager.AppsDir = Path.Combine(AgentStager.DropletDir, "apps");
            AgentStager.DbDir = Path.Combine(AgentStager.DropletDir, "db");

            Droplets.AppStateFile = Path.Combine(AgentStager.DropletDir, "applications.json");


            deaReactor.Uuid = Uuid;

            HelloMessage.Id = Uuid;
            HelloMessage.Host = Host;
            HelloMessage.FileViewerPort = AgentFileViewer.Port;
            HelloMessage.Version = Version;
            

        }

        protected override void ConstructReactor()
        {
            if (deaReactor == null)
            {
                deaReactor = new DeaReactor();
                vcapReactor = deaReactor;
            }
        }



        public override void Run()
        {

            Logger.info(String.Format("Starting VCAP DEA {0}", Version));




            AgentStager.SetupRuntimes();


            Logger.info(String.Format("Using network {0}", Host));
            Logger.info(String.Format("Max memory set to {0}M", AgentMonitoring.MaxMemoryMbytes));
            Logger.info(String.Format("Utilizing {0} cpu cores", Utils.NumberOfCores()));

            if (MultiTenant)
            {
                Logger.info(String.Format("Allowing multi-tenancy"));
            }
            else
            {
                Logger.info(String.Format("Restricting to single tenant"));
            }

            Logger.info(String.Format("Using directory {0}", AgentStager.DropletDir));


            AgentStager.CreateDirectories();
            Droplets.AppStateFile = Path.Combine(AgentStager.DbDir, "applications.json");

            //Clean everything in the staged directory
            AgentStager.CleanCacheDirectory();


            AgentFileViewer.Start(AgentStager.DropletDir);

            vcapReactor.OnNatsError += new EventHandler<ReactorErrorEventArgs>(NatsErrorHandler);

            deaReactor.OnDeaStatus += new SubscribeCallback(DeaStatusHandler);
            deaReactor.OnDropletStatus += new SubscribeCallback(DropletStatusHandler);
            deaReactor.OnDeaDiscover += new SubscribeCallback(DeaDiscoverHandler);
            deaReactor.OnDeaFindDroplet += new SubscribeCallback(DeaFindDropletHandler);
            deaReactor.OnDeaUpdate += new SubscribeCallback(DeaUpdateHandler);

            deaReactor.OnDeaStop += new SubscribeCallback(DeaStopHandler);
            deaReactor.OnDeaStart += new SubscribeCallback(DeaStartHandler);

            deaReactor.OnRouterStart += new SubscribeCallback(RouterStartHandler);
            deaReactor.OnHealthManagerStart += new SubscribeCallback(HealthmanagerStartHandler);


            base.Run();  // Start the nats client

            
            

            RecoverExistingDroplets();
            DeleteUntrackedInstanceDirs();


            TimerHelper.RecurringCall(Monitoring.HeartbeatIntervalMs, delegate
            {
                SendHeartbeat();
            });

            TimerHelper.RecurringLongCall(Monitoring.MonitorIntervalMs, delegate
            {
                MonitorApps();
            });

            TimerHelper.RecurringCall(Monitoring.CrashesReaperIntervalMs, delegate
            {
                CrashesReaper();
            });


            TimerHelper.RecurringCall(Monitoring.VarzUpdateIntervalMs, delegate
            {
                SnapshotVarz();
            });


            deaReactor.SendDeaStart(HelloMessage.SerializeToJson());
        }

        public void RecoverExistingDroplets()
        {
            if (!File.Exists(Droplets.AppStateFile))
            {
                Droplets.RecoverdDroplets = true;
                return;
            }


            object[] instances = JsonConvertibleObject.DeserializeFromJsonArray(File.ReadAllText(Droplets.AppStateFile));

            foreach (object obj in instances)
            {
                DropletInstance instance = new DropletInstance();
                instance.Properties.FromJsonIntermediateObject(obj);

                instance.Properties.Orphaned = true;
                instance.Properties.ResourcesTracked = false;
                AgentMonitoring.AddInstanceResources(instance);
                instance.Properties.StopProcessed = false;

                Droplets.AddDropletInstance(instance);
            }


            Droplets.RecoverdDroplets = true;

            if (AgentMonitoring.Clients > 0)
            {
                Logger.info(String.Format("DEA recovered {0} applications", AgentMonitoring.Clients));
            }

            MonitorApps();
            Droplets.ForEach(delegate(DropletInstance instance)
            {
                RegisterInstanceWithRouter(instance);
            });
            SendHeartbeat();
            Droplets.ScheduleSnapshotAppState();
        }

        private void DeleteUntrackedInstanceDirs()
        {


            HashSet<string> trackedInstanceDirs = new HashSet<string>();

            Droplets.ForEach(delegate(DropletInstance instance)
            {
                trackedInstanceDirs.Add(instance.Properties.Directory);
            });

            
            List<string> allInstanceDirs = Directory.GetDirectories(AgentStager.AppsDir, "*", SearchOption.TopDirectoryOnly).ToList();

            List<string> to_remove = (from dir in allInstanceDirs
                                      where !trackedInstanceDirs.Contains(dir)
                                      select dir).ToList();

            foreach (string dir in to_remove)
            {
                Logger.warn(String.Format("Removing instance '{0}', doesn't correspond to any instance entry.", dir));
                try
                {
                    // todo: vladi: this must be completed with cleaning up IIS sites
                    //Clean up the instance, including the IIS Web Site and the Windows User Accoung
                    //netiis is slow on cleanup
                    //Utils.ExecuteCommand(String.Format("netiis -cleanup={0}", dir));
                    Directory.Delete(dir, true);
                }
                catch (Exception e)
                {
                    Logger.warn(String.Format("Cloud not remove instance: {0}, error: {1}", dir, e.ToString()));
                }
            }

        }


        private void NatsErrorHandler(object sender,ReactorErrorEventArgs args)
        {
            string errorThrown = args.Message == null ? String.Empty : args.Message;
            Logger.error(String.Format("EXITING! Nats error: {0}", errorThrown));

            // Only snapshot app state if we had a chance to recover saved state. This prevents a connect error
            // that occurs before we can recover state from blowing existing data away.
            if (Droplets.RecoverdDroplets)
            {
                Droplets.SnapshotAppState();
            }

            throw new Exception(String.Format("Nats error: {0}", errorThrown));
        }

        public void EvacuateAppsThenQuit()
        {
            ShuttingDown = true;

            Logger.info("Evacuating applications..");

            Droplets.ForEach(delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterWriteLock();
                    if (instance.Properties.State != DropletInstanceState.CRASHED)
                    {
                        Logger.debug(String.Format("Evacuating app {0}", instance.Properties.InstanceId));

                        instance.Properties.ExitReason = DropletExitReason.DEA_EVACUATION;
                        deaReactor.SendDopletExited(instance.GenerateDropletExitedMessage().SerializeToJson());
                        instance.Properties.Evacuated = true;
                    }
                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }
            });

            Logger.info(String.Format("Scheduling shutdown in {0} seconds..", EvacuationDelayMs));

            Droplets.ScheduleSnapshotAppState();

            TimerHelper.DelayedCall(EvacuationDelayMs, delegate
            {
                Shutdown();
            });

        }

        public void Shutdown()
        {
            ShuttingDown = true;
            Logger.info("Shutting down..");

            Droplets.ForEach(true, delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterWriteLock();
                    if (instance.Properties.State != DropletInstanceState.CRASHED)
                    {
                        instance.Properties.ExitReason = DropletExitReason.DEA_SHUTDOWN;
                    }
                    StopDroplet(instance);
                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }
            });

            // Allows messages to get out.
            TimerHelper.DelayedCall(250, delegate
            {
                Droplets.SnapshotAppState();
                AgentFileViewer.Stop();
                deaReactor.NatsClient.Stop();
                Logger.info("Bye..");
            });

        }

        //todo: do this the right way
        public void WaitForExit()
        {
            while (deaReactor.NatsClient.Status == ConnectionStatus.Open)
            {
                Thread.Sleep(100);
            }
        }

        private void SendHeartbeat()
        {
            string response = Droplets.GenerateHearbeatMessage().SerializeToJson();
            deaReactor.SendDeaHeartbeat(response);
        }

        void SnapshotVarz()
        {
            try
            {
                VarzLock.EnterWriteLock();
                Varz["apps_max_memory"] = AgentMonitoring.MaxMemoryMbytes;
                Varz["apps_reserved_memory"] = AgentMonitoring.MemoryReservedMbytes;
                Varz["apps_used_memory"] = AgentMonitoring.MemoryUsageKbytes / 1024;
                Varz["num_apps"] = AgentMonitoring.MaxClients;
                if (ShuttingDown)
                    Varz["state"] = "SHUTTING_SOWN";
            }
            finally
            {
                VarzLock.ExitWriteLock();
            }
        }


        void DeaStatusHandler(string message, string reply, string subject)
        {

            
            Logger.debug("DEA received status message");
            DeaStatusMessageResponse response = new DeaStatusMessageResponse();

            response.Id = Uuid;
            response.Host = Host;
            response.FileViewerPort = AgentFileViewer.Port;
            response.Version = Version;
            response.MaxMemoryMbytes = AgentMonitoring.MaxMemoryMbytes;
            response.MemoryReservedMbytes = AgentMonitoring.MemoryReservedMbytes; ;
            response.MermoryUsageKbytes = AgentMonitoring.MemoryUsageKbytes;
            response.NumberOfClients = AgentMonitoring.Clients;
            if (ShuttingDown)
                response.State = "SHUTTING_DOWN";

            deaReactor.SendReply(reply, response.SerializeToJson());
        }


        void DropletStatusHandler(string message, string reply, string subject)
        {
            if (ShuttingDown)
                return;

            Logger.debug(String.Format("DEA received router start message: {0}", message));

            Droplets.ForEach(delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterReadLock();
                    if (instance.Properties.State == DropletInstanceState.RUNNING || instance.Properties.State == DropletInstanceState.STARTING)
                    {

                        DropletStatusMessageResponse response = instance.GenerateDropletStatusMessage();
                        response.Host = Host;
                        deaReactor.SendReply(reply, response.SerializeToJson());
                    }
                }
                finally
                {
                    instance.Lock.ExitReadLock();
                }

            });

        }



        void DeaDiscoverHandler(string message, string reply, string subject)
        {
            Logger.debug(String.Format("DEA received discovery message: {0}", message));
            if (ShuttingDown || AgentMonitoring.Clients >= AgentMonitoring.MaxClients || AgentMonitoring.MemoryReservedMbytes > AgentMonitoring.MaxMemoryMbytes)
            {
                Logger.debug("Ignoring request.");
                return;
            }
            
            DeaDiscoverMessageRequest pmessage = new DeaDiscoverMessageRequest();
            pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));

            if (!AgentStager.RuntimeSupported(pmessage.Runtime))
            {
                Logger.debug(String.Format("Ignoring request, {0} runtime not supported", pmessage.Runtime));
                return;
            }

            if (AgentMonitoring.MemoryReservedMbytes + pmessage.Limits.MemoryMbytes > AgentMonitoring.MaxMemoryMbytes)
            {
                Logger.debug(String.Format("Ignoring request, not enough memory."));
                return;
            }


            double taintMs = 0;

            try
            {
                Droplets.Lock.EnterReadLock();

                if(Droplets.Droplets.ContainsKey(pmessage.DropletId))
                {
                    taintMs += Droplets.Droplets[pmessage.DropletId].DropletInstances.Count * Monitoring.TaintPerAppMs;
                }

            }
            finally
            {
                Droplets.Lock.ExitReadLock();
            }

            try
            {
                AgentMonitoring.Lock.EnterReadLock();
                taintMs += Monitoring.TaintForMemoryMs * (AgentMonitoring.MemoryReservedMbytes / AgentMonitoring.MaxMemoryMbytes);
                taintMs = Math.Min(taintMs, Monitoring.TaintMaxMs);

            }
            finally
            {
                AgentMonitoring.Lock.ExitReadLock();
            }


            Logger.debug(String.Format("Sending dea.discover response message with a taint delay of: {0} ms", taintMs));
            TimerHelper.DelayedCall(taintMs, delegate()
            {
                deaReactor.SendReply(reply, HelloMessage.SerializeToJson());
            });
            

        }


        void DeaFindDropletHandler(string message, string reply, string subject)
        {
            if (ShuttingDown)
                return;

            DeaFindDropletMessageRequest pmessage = new DeaFindDropletMessageRequest();
            pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));


            Logger.debug(String.Format("DEA received find droplet message: {0}", message));


            Droplets.ForEach(delegate(DropletInstance instance)
            {

                try
                {
                    instance.Lock.EnterReadLock();

                    bool droplet_match = instance.Properties.DropletId == pmessage.DropletId;
                    bool version_match = pmessage.Version == null || pmessage.Version == instance.Properties.Version;
                    bool instace_match = pmessage.InstanceIds == null || pmessage.InstanceIds.Contains(instance.Properties.InstanceId);
                    bool index_match = pmessage.Indices == null || pmessage.Indices.Contains(instance.Properties.InstanceIndex);
                    bool state_match = pmessage.States == null || pmessage.States.Contains(instance.Properties.State);

                    DeaFindDropletMessageResponse response = new DeaFindDropletMessageResponse();

                    if (droplet_match && version_match && instace_match && index_match && state_match)
                    {
                        response.DeaId = Uuid;
                        response.Version = instance.Properties.Version;
                        response.DropletId = instance.Properties.DropletId;
                        response.InstanceId = instance.Properties.InstanceId;
                        response.Index = instance.Properties.InstanceIndex;
                        response.State = instance.Properties.State;
                        response.StateTimestamp = instance.Properties.StateTimestamp;
                        response.FileUri = String.Format(@"http://{0}:{1}/droplets/", Host, AgentFileViewer.Port);
                        response.FileAuth = AgentFileViewer.Credentials;
                        response.Staged = instance.Properties.Staged;
                        response.DebugIp = instance.Properties.DebugIp;
                        response.DebugPort = instance.Properties.DebugPort;


                        if (pmessage.IncludeStates && instance.Properties.State == DropletInstanceState.RUNNING)
                        {
                            response.Stats = instance.GenerateDropletStatusMessage();
                            response.Stats.Host = Host;
                            response.Stats.Cores = Utils.NumberOfCores();
                        }

                        deaReactor.SendReply(reply, response.SerializeToJson());
                    }

                }
                finally
                {
                    instance.Lock.ExitReadLock();
                }
                
            });

            
        }


        void DeaUpdateHandler(string message, string replay, string subject)
        {
            if (ShuttingDown)
                return;


            DeaUpdateMessageRequest pmessage = new DeaUpdateMessageRequest();
            pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));

            Logger.debug(String.Format("DEA received update message: {0}", message));

            Droplets.ForEach(delegate(DropletInstance instance)
            {
                if (instance.Properties.DropletId == pmessage.DropletId)
                {
                    try
                    {
                        instance.Lock.EnterWriteLock();

                        Logger.debug(String.Format("Mapping new URIs"));
                        Logger.debug(String.Format("New: {0} Current: {1}", JsonConvertibleObject.SerializeToJson(pmessage.Uris), JsonConvertibleObject.SerializeToJson(instance.Properties.Uris)));

                        List<string> toUnregister = new List<string>(instance.Properties.Uris.Except(pmessage.Uris));
                        List<string> toRegister = new List<string>(pmessage.Uris.Except(instance.Properties.Uris));

                        instance.Properties.Uris = toUnregister;
                        UnregisterInstanceFromRouter(instance);

                        instance.Properties.Uris = toRegister;
                        RegisterInstanceWithRouter(instance);

                        instance.Properties.Uris = pmessage.Uris;

                    }
                    finally
                    {
                        instance.Lock.ExitWriteLock();
                    }
                }
            });

        }



        void DeaStopHandler(string message, string replay, string subject)
        {

            if (ShuttingDown)
                return;

            DeaStopMessageRequest pmessage = new DeaStopMessageRequest();
            pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));


            Logger.debug(String.Format("DEA received stop message: {0}", message));


            Droplets.ForEach(true, delegate(DropletInstance instance)
            {

                try
                {
                    instance.Lock.EnterWriteLock();

                    bool droplet_match = instance.Properties.DropletId == pmessage.DropletId;
                    bool version_match = pmessage.Version == null || pmessage.Version == instance.Properties.Version;
                    bool instace_match = pmessage.InstanceIds == null || pmessage.InstanceIds.Contains(instance.Properties.InstanceId);
                    bool index_match = pmessage.Indices == null || pmessage.Indices.Contains(instance.Properties.InstanceIndex);
                    bool state_match = pmessage.States == null || pmessage.States.Contains(instance.Properties.State);


                    if (droplet_match && version_match && instace_match && index_match && state_match)
                    {
                        if (instance.Properties.State == DropletInstanceState.STARTING || instance.Properties.State == DropletInstanceState.RUNNING)
                        {
                            instance.Properties.ExitReason = DropletExitReason.STOPPED;
                        }
                        if (instance.Properties.State == DropletInstanceState.CRASHED)
                        {
                            instance.Properties.State = DropletInstanceState.DELETED;
                            instance.Properties.StopProcessed = false;
                        }

                        StopDroplet(instance);
                    }

                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }

            });

        }

        //onply stops the droplet instance from running, no cleanup or resource untracking
        void StopDroplet(DropletInstance instance)
        {
            try
            {
                instance.Lock.EnterWriteLock();

                if (instance.Properties.StopProcessed)
                    return;

                // Unplug us from the system immediately, both the routers and health managers.
                if (!instance.Properties.NotifiedExited)
                {
                    UnregisterInstanceFromRouter(instance);

                    if (instance.Properties.ExitReason == null)
                    {
                        instance.Properties.ExitReason = DropletExitReason.CRASHED;
                        instance.Properties.State = DropletInstanceState.CRASHED;
                        instance.Properties.StateTimestamp = DateTime.Now;
                        if (!instance.IsRunning)
                        {
                            instance.Properties.Pid = 0;
                        }
                    }

                    deaReactor.SendDopletExited(instance.GenerateDropletExitedMessage().SerializeToJson());

                    instance.Properties.NotifiedExited = true;
                }



                Logger.info(String.Format("Stopping instance {0}", instance.Properties.LoggingId));

                // if system thinks this process is running, make sure to execute stop script

                if (instance.Properties.Pid != 0 || instance.Properties.State == DropletInstanceState.STARTING || instance.Properties.State == DropletInstanceState.RUNNING)
                {
                    if (instance.Properties.State != DropletInstanceState.CRASHED)
                    {
                        instance.Properties.State = DropletInstanceState.STOPPED;
                        instance.Properties.StateTimestamp = DateTime.Now;
                    }


                    StreamWriterDelegate stopOperation = delegate(StreamWriter stdin)
                    {
                        stdin.WriteLine(String.Format("cd /D {0}", instance.Properties.Directory));
                        stdin.WriteLine("copy .\\stop .\\stop.ps1");
                        stdin.WriteLine("powershell.exe -ExecutionPolicy Bypass -InputFormat None -noninteractive -file .\\stop.ps1");
                        stdin.WriteLine("exit");
                    };

                    ProcessDoneDelegate exitOperation = delegate(string output, int status)
                    {
                        Logger.info(String.Format("Stop operation completed running with status = {0}.", status));
                        Logger.info(String.Format("Stop operation std output is: {0}", output));
                        
                    };

                    //TODO: vladi: this must be done with clean environment variables
                    Utils.ExecuteCommands("cmd", "", stopOperation, exitOperation);
                }

                CleanupDroplet(instance);

                instance.Properties.StopProcessed = true;
                
            }
            finally
            {
                instance.Lock.ExitWriteLock();
            }
        }

        private void CleanupDroplet(DropletInstance instance)
        {
            // Drop usage and resource tracking regardless of state
            AgentMonitoring.RemoveInstanceResources(instance);


            // clean up the in memory instance and directory only if the instance didn't crash
            if (instance.Properties.State != DropletInstanceState.CRASHED)
            {
                Droplets.RemoveDropletInstance(instance);
                Droplets.ScheduleSnapshotAppState();

                if (!DisableDirCleanup)
                {

                    for (int retryAttempts = 5; retryAttempts > 0; retryAttempts--)
                    {
                        try
                        {
                            Directory.Delete(instance.Properties.Directory, true);
                            Logger.debug(String.Format("{0}: Cleand up dir {1}", instance.Properties.Name, instance.Properties.Directory));
                            break;
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Logger.warn(String.Format("Unable to delete direcotry {0}, UnauthorizedAccessException: {1}", instance.Properties.Directory, e.ToString()));
                            Thread.Sleep(300);
                        }
                        catch (Exception e)
                        {
                            Logger.warn(String.Format("Unable to delete direcotry {0}, Exception: {1}", instance.Properties.Directory, e.ToString()));
                            break;
                        }
                    }

                }
            }
        }

        void DeaStartHandler(string message, string reply, string subject)
        {
            DeaStartMessageRequest pmessage;
            DropletInstance instance;
            List<string> AppEnv;
            try
            {
                Droplets.Lock.EnterWriteLock();
                

                if (ShuttingDown) return;
                Logger.debug(String.Format("DEA received start message: {0}", message));

                pmessage = new DeaStartMessageRequest();
                pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));

                long MemoryMbytes = pmessage.Limits != null && pmessage.Limits.MemoryMbytes != null ? pmessage.Limits.MemoryMbytes.Value : Monitoring.DefaultAppMemMbytes;
                long DiskMbytes = pmessage.Limits != null && pmessage.Limits.DiskMbytes != null ? pmessage.Limits.DiskMbytes.Value : Monitoring.DefaultAppDiskMbytes;
                long Fds = pmessage.Limits != null && pmessage.Limits.Fds != null ? pmessage.Limits.Fds.Value : Monitoring.DefaultAppFds;

                if (AgentMonitoring.MemoryReservedMbytes + MemoryMbytes > AgentMonitoring.MaxMemoryMbytes || AgentMonitoring.Clients >= AgentMonitoring.MaxClients)
                {
                    Logger.info("Do not have room for this client application");
                    return;
                }

                if (String.IsNullOrEmpty(pmessage.Sha1) || String.IsNullOrEmpty(pmessage.ExecutableFile) || String.IsNullOrEmpty(pmessage.ExecutableUri) )
                {
                    Logger.warn(String.Format("Start request missing proper download information, ignoring request. ({0})", message));
                    return;
                }

                if (!AgentStager.RuntimeSupported(pmessage.Runtime))
                {
                    Logger.warn(String.Format("Cloud not start, runtime not supported. ({0})", message));
                }

                instance = Droplets.CreateDropletInstance(pmessage);

                instance.Properties.MemoryQuotaBytes = MemoryMbytes * 1024 * 1024;
                instance.Properties.DiskQuotaBytes = DiskMbytes * 1024 * 1024;
                instance.Properties.FdsQuota = Fds;
                instance.Properties.Staged = instance.Properties.Name + "-" + instance.Properties.InstanceIndex + "-" + instance.Properties.InstanceId;
                instance.Properties.Directory = Path.Combine(AgentStager.AppsDir, instance.Properties.Staged);

                if (!String.IsNullOrEmpty(instance.Properties.DebugMode))
                {
                    instance.Properties.DebugPort = Utils.GetEphemeralPort();
                    instance.Properties.DebugIp = Host;
                }

                instance.Properties.Port = Utils.GetEphemeralPort();

                AppEnv = SetupInstanceEnv(instance, pmessage.Environment, pmessage.Services);


                AgentMonitoring.AddInstanceResources(instance);

            }
            finally
            {
                Droplets.Lock.ExitWriteLock();    
            }
           
            
            //toconsider: the pre-starting stage should be able to gracefuly stop when the shutdown flag is set
            ThreadPool.QueueUserWorkItem(delegate(object data)
            {
                StartDropletInstance(instance, AppEnv, pmessage.Sha1, pmessage.ExecutableFile, pmessage.ExecutableUri);
            });


        }


        private void StartDropletInstance(DropletInstance instance, List<string> AppEnv, string sha1, string executableFile, string executableUri)
        {
            try
            {

                string TgzFile = Path.Combine(AgentStager.StagedDir, sha1 + ".tgz");
                AgentStager.StageAppDirectory(executableFile, executableUri, sha1, TgzFile, instance);

                Logger.debug("Download compleate");


                string starting = string.Format("Starting up instance {0} on port {1} ", instance.Properties.LoggingId, instance.Properties.Port);
                
                if (!String.IsNullOrEmpty(instance.Properties.DebugMode))
                    Logger.info(starting + String.Format("with debugger port {0}", instance.Properties.DebugPort));
                else
                    Logger.info(starting);

                Logger.debug(String.Format("Clients: {0}", AgentMonitoring.Clients));
                Logger.debug(String.Format("Reserved Memory Usage: {0} MB of {1} MB TOTAL", AgentMonitoring.MemoryReservedMbytes, AgentMonitoring.MaxMemoryMbytes));


                StreamWriterDelegate startOperation = delegate(StreamWriter stdin)
                {
                    stdin.WriteLine(String.Format("cd /D {0}", instance.Properties.Directory));
                    foreach (String env in AppEnv)
                    {
                        stdin.WriteLine(String.Format("set {0}", env));
                    }


                    string runtimeLayer = String.Format("{0}\\netiis.exe", Directory.GetCurrentDirectory());

                    stdin.WriteLine("copy .\\startup .\\startup.ps1");
                    stdin.WriteLine(String.Format("powershell.exe -ExecutionPolicy Bypass -InputFormat None -noninteractive -file .\\startup.ps1 \"{0}\"", runtimeLayer));
                    stdin.WriteLine("exit");
                };

                ProcessDoneDelegate exitOperation = delegate(string output, int status)
                {
                    Logger.info(String.Format("{0} completed running with status = {1}.", instance.Properties.Name, status));
                    Logger.info(String.Format("{0} uptime was {1}.", instance.Properties.Name, DateTime.Now - instance.Properties.StateTimestamp));

                    StopDroplet(instance);
                };

                //TODO: vladi: this must be done with clean environment variables
                Utils.ExecuteCommands("cmd", "", startOperation, exitOperation);



                DetectAppReady(instance,
                    delegate(bool detected)
                    {
                        try
                        {
                            instance.Lock.EnterWriteLock();
                            if (detected && !instance.Properties.StopProcessed)
                            {
                                Logger.info(String.Format("Instance {0} is ready for connections, notifying system of status", instance.Properties.LoggingId));
                                instance.Properties.State = DropletInstanceState.RUNNING;
                                instance.Properties.StateTimestamp = DateTime.Now;

                                deaReactor.SendDeaHeartbeat(instance.GenerateHeartbeat().SerializeToJson());
                                RegisterInstanceWithRouter(instance);
                                Droplets.ScheduleSnapshotAppState();

                            }
                            else
                            {
                                Logger.warn("Giving up on connecting app.");
                                StopDroplet(instance);
                            }
                        }
                        finally
                        {
                            instance.Lock.ExitWriteLock();
                        }
                    }
                );


                int pid = DetectAppPid(instance);

                try
                {
                    instance.Lock.EnterWriteLock();

                    if (pid != 0 && !instance.Properties.StopProcessed)
                    {
                        Logger.info(String.Format("PID:{0} assigned to droplet instance: {1}", pid, instance.Properties.LoggingId));
                        instance.Properties.Pid = pid;
                        Droplets.ScheduleSnapshotAppState();
                    }
                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }




            }
            catch(Exception ex)
            {
                Logger.warn(String.Format("Failed staging app dir '{0}', not starting app {1}, Exception: {2}", instance.Properties.Directory, instance.Properties.LoggingId, ex.ToString()));
                try
                {
                    instance.Lock.EnterWriteLock();

                    instance.Properties.State = DropletInstanceState.CRASHED;
                    instance.Properties.ExitReason = DropletExitReason.CRASHED;
                    instance.Properties.StateTimestamp = DateTime.Now;

                    
                    StopDroplet(instance);

                    Droplets.RemoveDropletInstance(instance);
                    Droplets.ScheduleSnapshotAppState();

                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }

            }

        }

        public delegate void BoolStateBlockDelegate(bool state);

        private void DetectAppReady(DropletInstance instance, BoolStateBlockDelegate callBack)
        {
            //string state_file = manifest.ContainsKey("state_file") ? manifest["state_file"] : null;
            //if (state_file != null && state_file != String.Empty)
            //{
            //    state_file = Path.Combine(instance.Dir, state_file);
            //    detect_state_ready(instance, state_file, block);
            //}
            //else
            //{
            DetectPortReady(instance, callBack);
            //}
        }

        private void DetectPortReady(DropletInstance instance, BoolStateBlockDelegate callBack)
        {
            int port = instance.Properties.Port;

            int attempts = 0;
            bool keep_going = true;
            while (attempts <= 1000 && instance.Properties.State == DropletInstanceState.STARTING && keep_going == true)
            {
                AutoResetEvent connectedEvent = new AutoResetEvent(false);

                TcpClient client = new TcpClient();
                IAsyncResult result = client.BeginConnect(Host, port, null, null);
                result.AsyncWaitHandle.WaitOne(100);
                
                if (client.Connected)
                {
                    client.Close();
                    keep_going = false;
                    callBack(true);
                }
                else
                {
                    client.Close();
                }
                Thread.Sleep(100);
                attempts++;
            }

            if (keep_going)
            {
                callBack(false);
            }
        }


        private int DetectAppPid(DropletInstance instance)
        {
            int detect_attempts = 0;
            int pid = 0;

            while(true)
            {
                try
                {
                    string pid_file = Path.Combine(instance.Properties.Directory, "run.pid");
                    if (File.Exists(pid_file))
                    {
                        pid = Convert.ToInt32(File.ReadAllText(pid_file));
                        break;
                    }
                    else
                    {
                        detect_attempts++;
                        if (detect_attempts > 300 || !(instance.Properties.State == DropletInstanceState.STARTING || instance.Properties.State == DropletInstanceState.RUNNING))
                        {
                            Logger.warn("Giving up detecting stop file");
                            break;
                        }
                    }
                }
                catch 
                {
                }
                Thread.Sleep(500);
            }

            
            return pid;
        }


        private List<string> SetupInstanceEnv(DropletInstance instance, List<string> app_env, List<Dictionary<string, object>> services)
        {
            List<string> env = new List<string>();

            env.Add(String.Format("HOME={0}", instance.Properties.Directory));
            env.Add(String.Format("VCAP_APPLICATION='{0}'", create_instance_for_env(instance)));
            env.Add(String.Format("VCAP_SERVICES='{0}'", create_services_for_env(services)));
            env.Add(String.Format("VCAP_APP_HOST='{0}'", Host));
            env.Add(String.Format("VCAP_APP_PORT='{0}'", instance.Properties.Port));
            env.Add(String.Format("VCAP_DEBUG_IP='{0}'", instance.Properties.DebugIp));
            env.Add(String.Format("VCAP_DEBUG_PORT='{0}'", instance.Properties.DebugPort));


            if (instance.Properties.DebugPort != 0 && AgentStager.Runtimes[instance.Properties.Runtime].DebugEnv != null)
            {
                env.AddRange( AgentStager.Runtimes[instance.Properties.Runtime].DebugEnv[instance.Properties.DebugMode] );
            }



            // LEGACY STUFF
            env.Add(String.Format("VMC_WARNING_WARNING='All VMC_* environment variables are deprecated, please use VCAP_* versions.'"));
            env.Add(String.Format("VMC_SERVICES='{0}'", create_legacy_services_for_env(services)));
            env.Add(String.Format("VMC_APP_INSTANCE='{0}'", instance.Properties.SerializeToJson()));
            env.Add(String.Format("VMC_APP_NAME='{0}'", instance.Properties.Name));
            env.Add(String.Format("VMC_APP_ID='{0}'", instance.Properties.InstanceId));
            env.Add(String.Format("VMC_APP_VERSION='{0}'", instance.Properties.Version));
            env.Add(String.Format("VMC_APP_HOST='{0}'", Host));
            env.Add(String.Format("VMC_APP_PORT='{0}'", instance.Properties.Port));

            foreach (Dictionary<string, object> service in services)
            {
                string hostname = string.Empty;

                Dictionary<string, object> serviceCredentials =  JsonConvertibleObject.ObjectToValue<Dictionary<string, object>>(service["credentials"]);

                if (serviceCredentials.ContainsKey("hostname"))
                {
                    hostname = JsonConvertibleObject.ObjectToValue<string>(serviceCredentials["hostname"]);
                }
                else if (serviceCredentials.ContainsKey("host"))
                {
                    hostname = JsonConvertibleObject.ObjectToValue<string>(serviceCredentials["host"]);
                }

                string port = JsonConvertibleObject.ObjectToValue<string>(serviceCredentials["port"]);

                if (!String.IsNullOrEmpty(hostname) && !String.IsNullOrEmpty(port))
                {
                    env.Add(String.Format("VMC_{0}={1}:{2}", service["vendor"].ToString().ToUpper(), hostname, port));
                }
            }

            // Do the runtime environment settings

            foreach (KeyValuePair<string, string> runtimeEnv in AgentStager.Runtimes[instance.Properties.Runtime].Environment)
            {
                env.Add(runtimeEnv.Key + "=" + runtimeEnv.Value);
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

        private string create_instance_for_env(DropletInstance instance)
        {
            List<string> whitelist = new List<string>() { "instance_id", "instance_index", "name", "uris", "users", "version", "start", "runtime", "state_timestamp", "port" };
            Dictionary<string, object> env_hash = new Dictionary<string, object>();

            Dictionary<string, object> jInstance = instance.Properties.ToJsonIntermediateObject();

            foreach (string key in whitelist)
            {
                if (jInstance[key] != null)
                {
                    env_hash[key] = JsonConvertibleObject.ObjectToValue<object>(jInstance[key]);
                }
            }

            env_hash["limits"] = new Dictionary<string, object>() {
                {"fds", instance.Properties.FdsQuota},
                {"mem", instance.Properties.MemoryQuotaBytes},
                {"disk", instance.Properties.DiskQuotaBytes}
            };

            env_hash["host"] = Host;

            return JsonConvertibleObject.SerializeToJson(env_hash);
        }


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
            return JsonConvertibleObject.SerializeToJson(as_legacy);
        }

        private string create_services_for_env(List<Dictionary<string, object>> services = null)
        {
            List<string> whitelist = new List<string>() { "name", "label", "plan", "tags", "plan_option", "credentials" };
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

            return JsonConvertibleObject.SerializeToJson(svcs_hash);
        }


        

        void RouterStartHandler(string message, string reply, string subject)
        {
            if (ShuttingDown)
                return;

            Logger.debug(String.Format("DEA received router start message: {0}", message));


            Droplets.ForEach(delegate(DropletInstance instance)
            {
                if (instance.Properties.State == DropletInstanceState.RUNNING)
                {
                    RegisterInstanceWithRouter(instance);
                }
            });

        }


        void RegisterInstanceWithRouter(DropletInstance instance)
        {
            RouterMessage response = new RouterMessage();
            try
            {
                instance.Lock.EnterReadLock();

                if (instance.Properties.Uris == null || instance.Properties.Uris.Count == 0) return;

                response.DeaId = Uuid;
                response.Host = Host;
                response.Port = Port;
                response.Uris = new List<string>(instance.Properties.Uris);

                response.Tags = new RouterMessage.TagsObject();
                response.Tags.Framwork = instance.Properties.Framework;
                response.Tags.Runtime = instance.Properties.Runtime;

            }
            finally
            {
                instance.Lock.ExitReadLock();
            }
            deaReactor.SendRouterRegister(response.SerializeToJson());
        }

        void UnregisterInstanceFromRouter(DropletInstance instance)
        {
            RouterMessage response = new RouterMessage();
            try
            {
                instance.Lock.EnterReadLock();

                if (instance.Properties.Uris == null || instance.Properties.Uris.Count == 0) return;

                response.DeaId = Uuid;
                response.Host = Host;
                response.Port = Port;
                response.Uris = instance.Properties.Uris;

                response.Tags = new RouterMessage.TagsObject();
                response.Tags.Framwork = instance.Properties.Framework;
                response.Tags.Runtime = instance.Properties.Runtime;

            }
            finally
            {
                instance.Lock.ExitReadLock();
            }

            deaReactor.SendRouterUnregister(response.SerializeToJson());
        }


        void HealthmanagerStartHandler(string message, string replay, string subject)
        {
            if (ShuttingDown)
                return;

            Logger.debug(String.Format("DEA received healthmanager start message: {0}", message));

            SendHeartbeat();
        }

        void MonitorApps()
        {
            //AgentMonitoring.MemoryUsageKbytes = 0;

            long memoryUsageKbytes = 0;
            List<object> runningApps = new List<object>();

            if (Droplets.NoMonitorableApps())
            {
                AgentMonitoring.MemoryUsageKbytes = 0;
                return;
            }

            
            DateTime start = DateTime.Now;

            ProcessData[] processStatuses = ProcessInformation.GetProcessUsage();

            TimeSpan elapsed = DateTime.Now - start;
            if (elapsed.TotalMilliseconds > 800) 
                Logger.warn(String.Format("Took {0}s to execute ps. ({1} entries returned)", elapsed.TotalSeconds, processStatuses.Length));

            Dictionary<int, ProcessData> pidInfo = new Dictionary<int, ProcessData>();
            foreach (ProcessData processStatus in processStatuses)
            {
                pidInfo[processStatus.ProcessId] = processStatus;
            }



            DateTime duStart = DateTime.Now;

            DiskUsageEntry[] duAll = DiskUsage.GetDiskUsage(AgentStager.AppsDir, "*", true);
    
            TimeSpan duElapsed = DateTime.Now - duStart;

            if (duElapsed.TotalMilliseconds > 800)
            {
                Logger.warn(String.Format("Took {0}s to execute du.", duElapsed.TotalSeconds));
                if ((duElapsed.TotalSeconds > 10) && ((DateTime.Now - AgentMonitoring.LastAppDump).TotalSeconds > Monitoring.AppsDumpIntervalMs))
                {
                    AgentMonitoring.DumpAppsDirDiskUsage(AgentStager.AppsDir);
                    AgentMonitoring.LastAppDump = DateTime.Now;
                }
            }

            Dictionary<string, long> duHash = new Dictionary<string, long>();
            foreach (DiskUsageEntry entry in duAll)
            {
                duHash[entry.Directory] = entry.Size * 1024;
            }


            Dictionary<string, Dictionary<string, Dictionary<string, long>>> metrics = new Dictionary<string, Dictionary<string, Dictionary<string, long>>>() 
            {
                {"framework", new Dictionary<string, Dictionary<string, long>>()}, 
                {"runtime", new Dictionary<string, Dictionary<string, long>>()}
            };

            
            Droplets.ForEach(true, delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterWriteLock();

                    if (instance.Properties.Pid != 0 && pidInfo.ContainsKey(instance.Properties.Pid))
                    {
                        int pid = instance.Properties.Pid;

                        long mem = (long)pidInfo[pid].WorkingSet;
                        long cpu = (long)pidInfo[pid].Cpu;
                        long disk = duHash.ContainsKey(instance.Properties.Directory) ? duHash[instance.Properties.Directory] : 0;


                        DropletInstanceUsage curUsage = new DropletInstanceUsage();
                        curUsage.Time = DateTime.Now;
                        curUsage.Cpu = cpu;
                        curUsage.MemoryKbytes = mem;
                        curUsage.DiskBytes = disk;


                        instance.Usage.Add(curUsage);
                        if (instance.Usage.Count > DropletInstance.MaxUsageSamples)
                        {
                            instance.Usage.RemoveAt(0);
                        }

                        if (Secure)
                        {
                            //CheckUsage(instance, curUsage);
                        }

                        memoryUsageKbytes += mem;



                        foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, long>>> kvp in metrics)
                        {
                            Dictionary<string, long> metric = new Dictionary<string, long>() 
                                    {
                                        {"used_memory", 0},
                                        {"reserved_memory", 0},
                                        {"used_disk", 0},
                                        {"used_cpu", 0}
                                    };

                            if (kvp.Key == "framework")
                            {
                                if (!metrics.ContainsKey(instance.Properties.Framework))
                                    kvp.Value[instance.Properties.Framework] = metric;
                                
                                metric = kvp.Value[instance.Properties.Framework];
                            }
                            if (kvp.Key == "runtime")
                            {
                                if (!metrics.ContainsKey(instance.Properties.Runtime))
                                    kvp.Value[instance.Properties.Runtime] = metric;
                                
                                metric = kvp.Value[instance.Properties.Runtime];
                            }

                            metric["used_memory"] += mem;
                            metric["reserved_memory"] += instance.Properties.MemoryQuotaBytes / 1024;
                            metric["used_disk"] += disk;
                            metric["used_cpu"] += cpu;
                        }

                        // Track running apps for varz tracking

                        instance.Properties.UsageRecent = curUsage;
                        runningApps.Add(instance.Properties.ToJsonIntermediateObject());

                    }
                    else
                    {
                        // App *should* no longer be running if we are here
                        // Check to see if this is an orphan that is no longer running, clean up here if needed 
                        // since there will not be a cleanup proc or stop call associated with the instance..
                        instance.Properties.Pid = 0;
                        if (instance.Properties.Orphaned && !instance.Properties.StopProcessed)
                        {
                            StopDroplet(instance);
                        }
                    }
                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }

            });

            // export running app information to varz
            Varz["running_apps"] = runningApps;
            Varz["frameworks"] = metrics["framework"];
            Varz["runtimes"] = metrics["runtime"];

            TimeSpan ttlog = DateTime.Now - start;
            if (ttlog.TotalMilliseconds > 1000)
            {
                Logger.warn(String.Format("Took {0}s to process ps and du stats", ttlog.TotalSeconds));
            }

            
        }

        // This is only called when in secure mode, cur_usage is in kb, quota is in bytes.
        private void CheckUsage(DropletInstance instance, DropletInstanceUsage usage)
        {
            if (instance == null || usage == null)
                return;

            // Check Mem
            if (usage.MemoryKbytes > (instance.Properties.MemoryQuotaBytes / 1024))
            {

                Logger logger = new Logger(Path.Combine(instance.Properties.Directory, "logs\\err.log"));

                logger.ffatal(String.Format("Memory limit of {0}M exceeded.", instance.Properties.MemoryQuotaBytes / 1024 / 1024));
                logger.ffatal(String.Format("Actual usage was {0}M, process terminated.", usage.MemoryKbytes / 1024));
                StopDroplet(instance);
            }

            // Check Disk
            if (usage.DiskBytes > instance.Properties.DiskQuotaBytes)
            {
                Logger logger = new Logger(Path.Combine(instance.Properties.Directory, "logs\\err.log"));
                logger.ffatal(String.Format("Disk usage limit of {0}M exceeded.", instance.Properties.DiskQuotaBytes / 1024 / 1024));
                logger.ffatal(String.Format("Actual usage was {0}M, process terminated.", usage.DiskBytes / 1024 / 1024));
                StopDroplet(instance);
            }

            // Check CPU
            if (instance.Usage.Count == 0)
            {
                return;
            }

            if (usage.Cpu > Monitoring.BeginReniceCpuThreshold)
            {
                int nice = instance.Properties.Nice + 1;
                if (nice < Monitoring.MaxReniceValue)
                {
                    instance.Properties.Nice = nice;
                    ProcessPriorityClass priority = 
                        nice == 0 ? ProcessPriorityClass.RealTime : nice == 1 ? ProcessPriorityClass.High :
                        nice == 2 ? ProcessPriorityClass.AboveNormal : nice == 3 ? ProcessPriorityClass.Normal : 
                        nice == 4 ? ProcessPriorityClass.BelowNormal : ProcessPriorityClass.Idle;

                    Logger.info(String.Format("Lowering priority on CPU bound process({0}), new value:{1}", instance.Properties.Name, priority));

                    //TODO: vladi: make sure this works on Windows
                    Process.GetProcessById(instance.Properties.Pid).PriorityClass = priority;
                }
            }

            // TODO, Check for an attack, or what looks like one, and look at history?
            // pegged_cpus = @num_cores * 100
        }

        private void CrashesReaper()
        {

            List<DropletInstance> toDelete = new List<DropletInstance>();

            Droplets.ForEach(delegate(DropletInstance instance)
            {
                if (instance.Properties.State == DropletInstanceState.CRASHED && (DateTime.Now - instance.Properties.StateTimestamp).TotalMilliseconds > Monitoring.CrashesReaperTimeoutMs)
                    toDelete.Add(instance);

            });

            foreach (DropletInstance instance in toDelete)
            {
                
                Logger.debug(String.Format("Crashes reaper deleted: {0}", instance.Properties.InstanceId));
                if (!DisableDirCleanup)
                {
                    try
                    {
                        Directory.Delete(instance.Properties.Directory, true);
                    }
                    catch
                    {
                    }
                }

                Droplets.RemoveDropletInstance(instance);
            }


        }



    }
}
