// -----------------------------------------------------------------------
// <copyright file="Agent.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;
    using Uhuru.Configuration;
    using Uhuru.NatsClient;
    using Uhuru.Utilities;
    using Uhuru.Utilities.ProcessPerformance;
    using Uhuru.CloudFoundry.Server.DEA.PluginBase;
    
    public delegate void BoolStateBlockCallback(bool state);

    public class Agent : VcapComponent
    {

        private const decimal Version = 0.99m;

        private const string HomeVariable = "HOME";
        private const string VcapApplicationVariable = "VCAP_APPLICATION";
        private const string VcapServicesVariable = "VCAP_SERVICES";
        private const string VcapAppHostVariable = "VCAP_APP_HOST";
        private const string VcapAppPortVariable = "VCAP_APP_PORT";
        private const string VcapAppDebugIpVariable = "VCAP_DEBUG_IP";
        private const string VcapAppDebugPortVariable = "VCAP_DEBUG_PORT";
        private const string VcapPluginStagingInfoVariable = "VCAP_PLUGIN_STAGING_INFO";
        private const string VcapWindowsUserVariable = "VCAP_WINDOWS_USER";
        private const string VcapWindowsUserPasswordVariable = "VCAP_WINDOWS_USER_PASSWORD";
        private const string VcapAppPidVariable = "VCAP_APP_PID";


        private DropletCollection droplets = new DropletCollection();
        private Stager stager = new Stager();

        private FileViewer fileViewer = new FileViewer();
        private Monitoring monitoring = new Monitoring();

        private bool disableDirCleanup;
        private bool enforceUsageLimit;
        private bool multiTenant;
        private bool secure;
        
        private DeaReactor deaReactor;

        private HelloMessage helloMessage = new HelloMessage(); 
        //private Dictionary<string, object> HelloMessage;
        private volatile bool shuttingDown = false;
        private int evacuationDelayMs = 30 * 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class. Loads the configuaration and initializes the members.
        /// </summary>
        public Agent()
        {
            foreach (Configuration.DEA.RuntimeElement deaConf in UhuruSection.GetSection().DEA.Runtimes)
            {
                DeaRuntime dea = new DeaRuntime();

                dea.Executable = deaConf.Executable;
                dea.Version = deaConf.Version;
                dea.VersionFlag = deaConf.VersionArgument;
                dea.AdditionalChecks = deaConf.AdditionalChecks;
                dea.Enabled = true;

                foreach (Configuration.DEA.EnvironmentElement ienv in deaConf.Environment)
                {
                    dea.Environment.Add(ienv.Name, ienv.Value);
                }
                
                foreach (Configuration.DEA.DebugElement debugEnv in deaConf.Debug)
                {
                    dea.DebugEnv.Add(debugEnv.Name, new Dictionary<string, string>());
                    foreach (Configuration.DEA.EnvironmentElement ienv in debugEnv.Environment)
                    {
                        dea.DebugEnv[debugEnv.Name].Add(ienv.Name, ienv.Value);
                    }   
                }

                this.stager.Runtimes.Add(deaConf.Name, dea);
            }

            this.stager.DropletDir = UhuruSection.GetSection().DEA.BaseDir;

            this.enforceUsageLimit = UhuruSection.GetSection().DEA.EnforceUsageLimit;
            this.disableDirCleanup = UhuruSection.GetSection().DEA.DisableDirCleanup;
            this.multiTenant = UhuruSection.GetSection().DEA.Multitenant;
            this.secure = UhuruSection.GetSection().DEA.Secure;

            this.monitoring.MaxMemoryMbytes = UhuruSection.GetSection().DEA.MaxMemory;

            this.fileViewer.Port = UhuruSection.GetSection().DEA.FilerPort;

            this.stager.ForeHttpFileSharing = UhuruSection.GetSection().DEA.ForceHttpSharing;

            this.ComponentType = "DEA";

            //apps_dump_dir = ConfigurationManager.AppSettings["logFile"] ?? Path.GetTempPath();
            this.monitoring.AppsDumpDirectory = Path.GetTempPath();

            //heartbeat_interval = UhuruSection.GetSection().DEA.HeartBeatInterval;

            this.monitoring.MaxClients = this.multiTenant ? Monitoring.DefaultMaxClients : 1;

            this.stager.StagedDir = Path.Combine(this.stager.DropletDir, "staged");
            this.stager.AppsDir = Path.Combine(this.stager.DropletDir, "apps");
            this.stager.DbDir = Path.Combine(this.stager.DropletDir, "db");

            this.droplets.AppStateFile = Path.Combine(stager.DropletDir, "applications.json");

            this.deaReactor.Uuid = Uuid;

            this.helloMessage.Id = this.Uuid;
            this.helloMessage.Host = this.Host;
            this.helloMessage.FileViewerPort = this.fileViewer.Port;
            this.helloMessage.Version = Version;
        }

        /// <summary>
        /// Constructs the needed reactor. In this case a DeaReactor is needed.
        /// </summary>
        protected override void ConstructReactor()
        {
            if (this.deaReactor == null)
            {
                this.deaReactor = new DeaReactor();
                this.VcapReactor = this.deaReactor;
            }
        }



        /// <summary>
        /// Runs the DEA.
        /// It prepares the NATS subscriptions, stats the NATS client, and the required timers.
        /// </summary>
        public override void Run()
        {

            Logger.Info(Strings.StartingVcapDea, Version);

            this.stager.SetupRuntimes();

            Logger.Info(Strings.UsingNetwork, this.Host);
            Logger.Info(Strings.MaxMemorySetTo, this.monitoring.MaxMemoryMbytes);
            Logger.Info(Strings.UtilizingCpuCores, Utils.NumberOfCores());

            if (this.multiTenant)
            {
                Logger.Info(Strings.Allowingmultitenancy);
            }
            else
            {
                Logger.Info(Strings.RestrictingToSingleTenant);
            }

            Logger.Info(Strings.UsingDirectory, this.stager.DropletDir);

            this.stager.CreateDirectories();
            this.droplets.AppStateFile = Path.Combine(this.stager.DbDir, "applications.json");

            //Clean everything in the staged directory
            this.stager.CleanCacheDirectory();


            this.fileViewer.Start(stager.AppsDir);

            this.VcapReactor.OnNatsError += new EventHandler<ReactorErrorEventArgs>(NatsErrorHandler);

            this.deaReactor.OnDeaStatus += new SubscribeCallback(DeaStatusHandler);
            this.deaReactor.OnDropletStatus += new SubscribeCallback(DropletStatusHandler);
            this.deaReactor.OnDeaDiscover += new SubscribeCallback(DeaDiscoverHandler);
            this.deaReactor.OnDeaFindDroplet += new SubscribeCallback(DeaFindDropletHandler);
            this.deaReactor.OnDeaUpdate += new SubscribeCallback(DeaUpdateHandler);

            this.deaReactor.OnDeaStop += new SubscribeCallback(DeaStopHandler);
            this.deaReactor.OnDeaStart += new SubscribeCallback(DeaStartHandler);

            this.deaReactor.OnRouterStart += new SubscribeCallback(RouterStartHandler);
            this.deaReactor.OnHealthManagerStart += new SubscribeCallback(HealthmanagerStartHandler);
            
            base.Run();  // Start the nats client

            this.RecoverExistingDroplets();

            this.DeleteUntrackedInstanceDirs();
            
            TimerHelper.RecurringLongCall(Monitoring.HeartbeatIntervalMilliseconds, delegate
            {
                this.SendHeartbeat();
            });

            TimerHelper.RecurringLongCall(Monitoring.MonitorIntervalMilliseconds, delegate
            {
                this.MonitorApps();
            });

            TimerHelper.RecurringLongCall(Monitoring.CrashesReaperIntervalMilliseconds, delegate
            {
                this.TheReaper();
            });
            
            TimerHelper.RecurringLongCall(Monitoring.VarzUpdateIntervalMilliseconds, delegate
            {
                this.SnapshotVarz();
            });

            this.deaReactor.SendDeaStart(this.helloMessage.SerializeToJson());
        }


        /// <summary>
        /// Loads the saved droplet instances the last dea process has saved using the ShanpShotAppState method. 
        /// </summary>
        public void RecoverExistingDroplets()
        {
            if (!File.Exists(droplets.AppStateFile))
            {
                droplets.RecoveredDroplets = true;
                return;
            }
            
            object[] instances = JsonConvertibleObject.DeserializeFromJsonArray(File.ReadAllText(droplets.AppStateFile));

            foreach (object obj in instances)
            {
                DropletInstance instance = new DropletInstance();
                
                try
                {
                    instance.Properties.FromJsonIntermediateObject(obj);
                    instance.Properties.Orphaned = true;
                    instance.Properties.ResourcesTracked = false;
                    monitoring.AddInstanceResources(instance);
                    instance.Properties.StopProcessed = false;

                    try
                    {
                        instance.LoadPlugin();


                        instance.Properties.EnvironmentVarialbes[VcapAppPidVariable] = instance.Properties.ProcessId.ToString();
                        List<ApplicationVariable> appVariables = new List<ApplicationVariable>();
                        foreach (KeyValuePair<string, string> appEnv in instance.Properties.EnvironmentVarialbes)
                        {
                            ApplicationVariable appVariable = new ApplicationVariable();
                            appVariable.Name = appEnv.Key;
                            appVariable.Value = appEnv.Value;
                            appVariables.Add(appVariable);
                        }

                        instance.Plugin.RecoverApplication(appVariables.ToArray());
                    }
                    catch (Exception ex)
                    {
                        instance.ErrorLog.Error(ex.ToString());
                    }

                    if (instance.Properties.State == DropletInstanceState.Starting)
                    {
                        DetectAppReady(instance);
                    }
                    
                    
                    droplets.AddDropletInstance(instance);
                }
                catch (Exception ex)
                {
                    Logger.Warning("Error recovering droplet {0}. Exception: {1}", instance.Properties.InstanceId, ex.ToString());
                }
            }
            
            droplets.RecoveredDroplets = true;

            if (monitoring.Clients > 0)
            {
                Logger.Info(Strings.DeaRecoveredApplications, monitoring.Clients);
            }

            MonitorApps();
            droplets.ForEach(delegate(DropletInstance instance)
            {
                RegisterInstanceWithRouter(instance);
            });
            SendHeartbeat();
            droplets.ScheduleSnapshotAppState();
        }

        /// <summary>
        /// If there are lingering instance directories in the application directory, delete them. 
        /// </summary>
        private void DeleteUntrackedInstanceDirs()
        {
            HashSet<string> trackedInstanceDirs = new HashSet<string>();

            droplets.ForEach(delegate(DropletInstance instance)
            {
                trackedInstanceDirs.Add(instance.Properties.Directory);
            });
                        
            List<string> allInstanceDirs = Directory.GetDirectories(stager.AppsDir, "*", SearchOption.TopDirectoryOnly).ToList();

            List<string> to_remove = (from dir in allInstanceDirs
                                      where !trackedInstanceDirs.Contains(dir)
                                      select dir).ToList();

            foreach (string dir in to_remove)
            {
                Logger.Warning(Strings.RemovingInstanceDoesn, dir);
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (System.UnauthorizedAccessException e)
                {
                    Logger.Warning(Strings.CloudNotRemoveInstance, dir, e.ToString());
                }
            }
        }


        /// <summary>
        /// Nats the error handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Uhuru.NatsClient.ReactorErrorEventArgs"/> instance containing the error data.</param>
        private void NatsErrorHandler(object sender,ReactorErrorEventArgs args)
        {
            string errorThrown = args.Message == null ? String.Empty : args.Message;
            Logger.Error(Strings.ExitingNatsError, errorThrown);

            // Only snapshot app state if we had a chance to recover saved state. This prevents a connect error
            // that occurs before we can recover state from blowing existing data away.
            if (droplets.RecoveredDroplets)
            {
                droplets.SnapshotAppState();
            }

            throw new Exception(String.Format(CultureInfo.InvariantCulture, Strings.NatsError, errorThrown));
        }



        /// <summary>
        /// First evacuates the Instances and after a delay it's calling the shutdown.
        /// </summary>
        public void EvacuateAppsThenQuit()
        {
            shuttingDown = true;

            Logger.Info(Strings.Evacuatingapplications);

            droplets.ForEach(delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterWriteLock();
                    if (instance.Properties.State != DropletInstanceState.Crashed)
                    {
                        Logger.Debug(Strings.EvacuatingApp, instance.Properties.InstanceId);

                        instance.Properties.ExitReason = DropletExitReason.DeaEvacuation;
                        deaReactor.SendDropletExited(instance.GenerateDropletExitedMessage().SerializeToJson());
                        instance.Properties.Evacuated = true;
                    }
                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }
            });

            Logger.Info(Strings.SchedulingShutdownIn, evacuationDelayMs);

            droplets.ScheduleSnapshotAppState();

            TimerHelper.DelayedCall(evacuationDelayMs, delegate
            {
                Shutdown();
            });

        }

        /// <summary>
        /// Shuts down the DEA. First it stops all the instances and then the Nats client.
        /// </summary>
        public void Shutdown()
        {
            shuttingDown = true;
            Logger.Info(Strings.ShuttingDownMessage);

            droplets.ForEach(true, delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterWriteLock();
                    if (instance.Properties.State != DropletInstanceState.Crashed)
                    {
                        instance.Properties.ExitReason = DropletExitReason.DeaShutdown;
                    }
                    StopDroplet(instance);
                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }
            });

            // Allows messages to get out.
            Thread.Sleep(250);

            droplets.SnapshotAppState();
            fileViewer.Stop();
            deaReactor.NatsClient.Stop();
            Logger.Info(Strings.ByeMessage);

        }


        /// <summary>
        /// Sends the heartbeat of every droplet instnace the DEA is aware of.
        /// </summary>
        private void SendHeartbeat()
        {
            string response = droplets.GenerateHeartbeatMessage().SerializeToJson();
            deaReactor.SendDeaHeartbeat(response);
        }

        /// <summary>
        /// Snapshots the varz with basic resource information.
        /// </summary>
        void SnapshotVarz()
        {
            try
            {
                VarzLock.EnterWriteLock();
                Varz["apps_max_memory"] = monitoring.MaxMemoryMbytes;
                Varz["apps_reserved_memory"] = monitoring.MemoryReservedMbytes;
                Varz["apps_used_memory"] = monitoring.MemoryUsageKbytes / 1024;
                Varz["num_apps"] = monitoring.MaxClients;
                if (shuttingDown)
                    Varz["state"] = "SHUTTING_DOWN";
            }
            finally
            {
                VarzLock.ExitWriteLock();
            }
        }

        void DeaStatusHandler(string message, string reply, string subject)
        {
            Logger.Debug(Strings.DEAreceivedstatusmessage);
            DeaStatusMessageResponse response = new DeaStatusMessageResponse();

            response.Id = Uuid;
            response.Host = Host;
            response.FileViewerPort = this.fileViewer.Port;
            response.Version = Version;
            response.MaxMemoryMbytes = this.monitoring.MaxMemoryMbytes;
            response.MemoryReservedMbytes = this.monitoring.MemoryReservedMbytes; ;
            response.MemoryUsageKbytes = this.monitoring.MemoryUsageKbytes;
            response.NumberOfClients = this.monitoring.Clients;
            if (this.shuttingDown)
                response.State = "SHUTTING_DOWN";

            this.deaReactor.SendReply(reply, response.SerializeToJson());
        }

        void DropletStatusHandler(string message, string reply, string subject)
        {
            if (this.shuttingDown)
            {
                return;
            }

            Logger.Debug(Strings.DeaReceivedRouterStart, message);

            this.droplets.ForEach(delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterReadLock();
                    if (instance.Properties.State == DropletInstanceState.Running || instance.Properties.State == DropletInstanceState.Starting)
                    {
                        DropletStatusMessageResponse response = instance.GenerateDropletStatusMessage();
                        response.Host = Host;
                        this.deaReactor.SendReply(reply, response.SerializeToJson());
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
            Logger.Debug(Strings.DeaReceivedDiscoveryMessage, message);
            if (shuttingDown || this.monitoring.Clients >= monitoring.MaxClients || this.monitoring.MemoryReservedMbytes > this.monitoring.MaxMemoryMbytes)
            {
                Logger.Debug(Strings.IgnoringRequest);
                return;
            }
            
            DeaDiscoverMessageRequest pmessage = new DeaDiscoverMessageRequest();
            pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));

            if (!this.stager.RuntimeSupported(pmessage.Runtime))
            {
                Logger.Debug(Strings.IgnoringRequestRuntime, pmessage.Runtime);
                return;
            }

            if (this.monitoring.MemoryReservedMbytes + pmessage.Limits.MemoryMbytes > this.monitoring.MaxMemoryMbytes)
            {
                Logger.Debug(Strings.IgnoringRequestNotEnoughMemory);
                return;
            }

            double taintMs = 0;

            try
            {
                this.droplets.Lock.EnterReadLock();

                if (this.droplets.Droplets.ContainsKey(pmessage.DropletId))
                {
                    taintMs += this.droplets.Droplets[pmessage.DropletId].DropletInstances.Count * Monitoring.TaintPerAppMilliseconds;
                }
            }
            finally
            {
                this.droplets.Lock.ExitReadLock();
            }

            try
            {
                this.monitoring.Lock.EnterReadLock();
                taintMs += Monitoring.TaintForMemoryMilliseconds * (this.monitoring.MemoryReservedMbytes / this.monitoring.MaxMemoryMbytes);
                taintMs = Math.Min(taintMs, Monitoring.TaintMaxMilliseconds);
            }
            finally
            {
                this.monitoring.Lock.ExitReadLock();
            }

            Logger.Debug(Strings.SendingDeaDiscoverResponse, taintMs);
            TimerHelper.DelayedCall(taintMs, delegate()
            {
                this.deaReactor.SendReply(reply, this.helloMessage.SerializeToJson());
            });
        }


        void DeaFindDropletHandler(string message, string reply, string subject)
        {
            if (this.shuttingDown)
                return;

            DeaFindDropletMessageRequest pmessage = new DeaFindDropletMessageRequest();
            pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));

            Logger.Debug(Strings.DeaReceivedFindDroplet, message);

            this.droplets.ForEach(delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterReadLock();

                    bool droplet_match = instance.Properties.DropletId == pmessage.DropletId;
                    bool version_match = pmessage.Version == null || pmessage.Version == instance.Properties.Version;
                    bool instace_match = pmessage.InstanceIds == null || pmessage.InstanceIds.Contains(instance.Properties.InstanceId);
                    bool index_match = pmessage.Indexes == null || pmessage.Indexes.Contains(instance.Properties.InstanceIndex);
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
                        response.FileUri = String.Format(CultureInfo.InvariantCulture, Strings.HttpDroplets, Host, this.fileViewer.Port);
                        response.FileAuth = this.fileViewer.Credentials;
                        response.Staged = instance.Properties.Staged;
                        response.DebugIP = instance.Properties.DebugIP;
                        response.DebugPort = instance.Properties.DebugPort;

                        if (pmessage.IncludeStates && instance.Properties.State == DropletInstanceState.Running)
                        {
                            response.Stats = instance.GenerateDropletStatusMessage();
                            response.Stats.Host = Host;
                            response.Stats.Cores = Utils.NumberOfCores();
                        }

                        this.deaReactor.SendReply(reply, response.SerializeToJson());
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
            if (this.shuttingDown)
                return;

            DeaUpdateMessageRequest pmessage = new DeaUpdateMessageRequest();
            pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));

            Logger.Debug(Strings.DeaReceivedUpdateMessage, message);

            this.droplets.ForEach(delegate(DropletInstance instance)
            {
                if (instance.Properties.DropletId == pmessage.DropletId)
                {
                    try
                    {
                        instance.Lock.EnterWriteLock();

                        Logger.Debug(Strings.MappingnewURIs);
                        Logger.Debug(Strings.NewCurrent, JsonConvertibleObject.SerializeToJson(pmessage.Uris), JsonConvertibleObject.SerializeToJson(instance.Properties.Uris));

                        List<string> toUnregister = new List<string>(instance.Properties.Uris.Except(pmessage.Uris));
                        List<string> toRegister = new List<string>(pmessage.Uris.Except(instance.Properties.Uris));

                        instance.Properties.Uris = toUnregister.ToArray();
                        this.UnregisterInstanceFromRouter(instance);

                        instance.Properties.Uris = toRegister.ToArray();
                        this.RegisterInstanceWithRouter(instance);

                        instance.Properties.Uris = pmessage.Uris.ToArray();
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
            if (this.shuttingDown)
                return;

            DeaStopMessageRequest pmessage = new DeaStopMessageRequest();
            pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));

            Logger.Debug(Strings.DeaReceivedStopMessage, message);

            this.droplets.ForEach(true, delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterWriteLock();

                    bool droplet_match = instance.Properties.DropletId == pmessage.DropletId;
                    bool version_match = pmessage.Version == null || pmessage.Version == instance.Properties.Version;
                    bool instace_match = pmessage.InstanceIds == null || pmessage.InstanceIds.Contains(instance.Properties.InstanceId);
                    bool index_match = pmessage.Indexes == null || pmessage.Indexes.Contains(instance.Properties.InstanceIndex);
                    bool state_match = pmessage.States == null || pmessage.States.Contains(instance.Properties.State);

                    if (droplet_match && version_match && instace_match && index_match && state_match)
                    {
                        if (instance.Properties.State == DropletInstanceState.Starting || instance.Properties.State == DropletInstanceState.Running)
                        {
                            instance.Properties.ExitReason = DropletExitReason.Stopped;
                        }
                        if (instance.Properties.State == DropletInstanceState.Crashed)
                        {
                            instance.Properties.State = DropletInstanceState.Deleted;
                            instance.Properties.StopProcessed = false;
                        }

                        this.StopDroplet(instance);
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
                    this.UnregisterInstanceFromRouter(instance);

                    if (instance.Properties.ExitReason == null)
                    {
                        instance.Properties.ExitReason = DropletExitReason.Crashed;
                        instance.Properties.State = DropletInstanceState.Crashed;
                        instance.Properties.StateTimestamp = DateTime.Now;
                        if (!instance.IsPidRunning)
                        {
                            instance.Properties.ProcessId = 0;
                        }
                    }

                    this.deaReactor.SendDropletExited(instance.GenerateDropletExitedMessage().SerializeToJson());

                    instance.Properties.NotifiedExited = true;
                }

                Logger.Info(Strings.StoppingInstance, instance.Properties.LoggingId);

                // if system thinks this process is running, make sure to execute stop script

                if (instance.Properties.State == DropletInstanceState.Starting || instance.Properties.State == DropletInstanceState.Running)
                {
                    instance.Properties.State = DropletInstanceState.Stopped;
                    instance.Properties.StateTimestamp = DateTime.Now;
                    if (instance.Plugin != null)
                    {
                        try
                        {
                            instance.Plugin.StopApplication();
                        }
                        catch (Exception ex)
                        {
                            instance.ErrorLog.Error(ex.ToString());
                        }
                    }
                }

                this.monitoring.RemoveInstanceResources(instance);
                instance.Properties.StopProcessed = true;

            }
            catch (Exception ex)
            {
                Logger.Error("Error stopping droplet: {0}, instance: {1}, exception:", instance.Properties.DropletId, instance.Properties.InstanceId, ex.ToString());
            }
            finally
            {
                instance.Lock.ExitWriteLock();
            }
        }

        void DeaStartHandler(string message, string reply, string subject)
        {
            DeaStartMessageRequest pmessage;
            DropletInstance instance;

            try
            {
                this.droplets.Lock.EnterWriteLock();
  
                if (this.shuttingDown) return;
                Logger.Debug(Strings.DeaReceivedStartMessage, message);

                pmessage = new DeaStartMessageRequest();
                pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));

                long MemoryMbytes = pmessage.Limits != null && pmessage.Limits.MemoryMbytes != null ? pmessage.Limits.MemoryMbytes.Value : Monitoring.DefaultAppMemoryMbytes;
                long DiskMbytes = pmessage.Limits != null && pmessage.Limits.DiskMbytes != null ? pmessage.Limits.DiskMbytes.Value : Monitoring.DefaultAppDiskMbytes;
                long Fds = pmessage.Limits != null && pmessage.Limits.FileDescriptors != null ? pmessage.Limits.FileDescriptors.Value : Monitoring.DefaultAppFds;

                if (this.monitoring.MemoryReservedMbytes + MemoryMbytes > this.monitoring.MaxMemoryMbytes || this.monitoring.Clients >= this.monitoring.MaxClients)
                {
                    Logger.Info(Strings.Donothaveroomforthisclient);
                    return;
                }

                if (String.IsNullOrEmpty(pmessage.Sha1) || String.IsNullOrEmpty(pmessage.ExecutableFile) || String.IsNullOrEmpty(pmessage.ExecutableUri) )
                {
                    Logger.Warning(Strings.StartRequestMissingProper, message);
                    return;
                }

                if (!this.stager.RuntimeSupported(pmessage.Runtime))
                {
                    Logger.Warning(Strings.CloudNotStartRuntimeNot, message);
                    return;
                }


                instance = this.droplets.CreateDropletInstance(pmessage);

                instance.Properties.MemoryQuotaBytes = MemoryMbytes * 1024 * 1024;
                instance.Properties.DiskQuotaBytes = DiskMbytes * 1024 * 1024;
                instance.Properties.FdsQuota = Fds;
                instance.Properties.Staged = instance.Properties.Name + "-" + instance.Properties.InstanceIndex + "-" + instance.Properties.InstanceId;
                instance.Properties.Directory = Path.Combine(this.stager.AppsDir, instance.Properties.Staged);

                if (!String.IsNullOrEmpty(instance.Properties.DebugMode))
                {
                    instance.Properties.DebugPort = NetworkInterface.GrabEphemeralPort();
                    instance.Properties.DebugIP = Host;
                }

                instance.Properties.Port = NetworkInterface.GrabEphemeralPort();

                instance.Properties.EnvironmentVarialbes = this.SetupInstanceEnv(instance, pmessage.Environment, pmessage.Services);

                this.monitoring.AddInstanceResources(instance);
            }
            finally
            {
                this.droplets.Lock.ExitWriteLock();    
            }
             
            //toconsider: the pre-starting stage should be able to gracefuly stop when the shutdown flag is set
            ThreadPool.QueueUserWorkItem(delegate(object data)
            {
                this.StartDropletInstance(instance, pmessage.Sha1, pmessage.ExecutableFile, pmessage.ExecutableUri);
            });
        }


        /// <summary>
        /// Starts the droplet instance after the basic initalization is done.
        /// </summary>
        /// <param name="instance">The instance to be started.</param>
        /// <param name="sha1">The sha1 of the droplet file.</param>
        /// <param name="executableFile">The path to the droplet file.</param>
        /// <param name="executableUri">The URI to the droplet file.</param>
        private void StartDropletInstance(DropletInstance instance, string sha1, string executableFile, string executableUri)
        {
            try
            {
                string TgzFile = Path.Combine(this.stager.StagedDir, sha1 + ".tgz");
                this.stager.StageAppDirectory(executableFile, executableUri, sha1, TgzFile, instance);

                Logger.Debug(Strings.Downloadcompleate);

                string starting = string.Format(CultureInfo.InvariantCulture, Strings.StartingUpInstanceOnPort, instance.Properties.LoggingId, instance.Properties.Port);
                
                if (!String.IsNullOrEmpty(instance.Properties.DebugMode))
                    Logger.Info(starting + Strings.WithDebuggerPort, instance.Properties.DebugPort);
                else
                    Logger.Info(starting);

                Logger.Debug(Strings.Clients, this.monitoring.Clients);
                Logger.Debug(Strings.ReservedMemoryUsageMb, this.monitoring.MemoryReservedMbytes, this.monitoring.MaxMemoryMbytes);


                List<ApplicationVariable> appVariables = new List<ApplicationVariable>();
                try
                {
                    instance.Lock.EnterWriteLock();

                    instance.Properties.WindowsPassword = "P4s$" + Credentials.GenerateCredential();
                    instance.Properties.WindowsUsername = WindowsVCAPUsers.CreateUser(instance.Properties.InstanceId, instance.Properties.WindowsPassword);

                    instance.Properties.EnvironmentVarialbes.Add(VcapWindowsUserVariable, instance.Properties.WindowsUsername);
                    instance.Properties.EnvironmentVarialbes.Add(VcapWindowsUserPasswordVariable, instance.Properties.WindowsPassword);
                    instance.Properties.EnvironmentVarialbes.Add(VcapPluginStagingInfoVariable, File.ReadAllText(Path.Combine(instance.Properties.Directory, "startup")));

                    foreach (KeyValuePair<string, string> appEnv in instance.Properties.EnvironmentVarialbes)
                    {
                        ApplicationVariable appVariable = new ApplicationVariable();
                        appVariable.Name = appEnv.Key;
                        appVariable.Value = appEnv.Value;
                        appVariables.Add(appVariable);
                    }

                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }

                DateTime start = DateTime.Now;

                instance.LoadPlugin();
                instance.Plugin.ConfigureApplication(appVariables.ToArray());
                instance.Plugin.StartApplication();

                int pid = instance.Plugin.GetApplicationProcessId();

                Logger.Debug("Took {0} to load the plugin, configure the application, and start it.", (DateTime.Now - start).TotalSeconds);


                try
                {
                    instance.Lock.EnterWriteLock();

                    if (!instance.Properties.StopProcessed)
                    {
                        Logger.Info(Strings.PidAssignedToDroplet, pid, instance.Properties.LoggingId);
                        instance.Properties.ProcessId = pid;
                        this.droplets.ScheduleSnapshotAppState();
                    }
                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }


                this.DetectAppReady(instance);
            }
            catch(Exception ex)
            {
                Logger.Warning(Strings.FailedStagingAppDir, instance.Properties.Directory, instance.Properties.LoggingId, ex.ToString());
                try
                {
                    instance.Lock.EnterWriteLock();

                    instance.Properties.State = DropletInstanceState.Crashed;
                    instance.Properties.ExitReason = DropletExitReason.Crashed;
                    instance.Properties.StateTimestamp = DateTime.Now;
                                        
                    this.StopDroplet(instance);

                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }
            }
        }


        /// <summary>
        /// Detects if an droplet instance is ready, so that it can be set to a Running state and registerd with the router.
        /// </summary>
        /// <param name="instance">The instance do be detected.</param>
        private void DetectAppReady(DropletInstance instance)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                DetectAppReady(instance,
                        delegate(bool detected)
                        {
                            try
                            {
                                instance.Lock.EnterWriteLock();
                                if (detected)
                                {
                                    if (instance.Properties.State == DropletInstanceState.Starting)
                                    {
                                        Logger.Info(Strings.InstanceIsReadyForConnections, instance.Properties.LoggingId);
                                        instance.Properties.State = DropletInstanceState.Running;
                                        instance.Properties.StateTimestamp = DateTime.Now;

                                        this.deaReactor.SendDeaHeartbeat(instance.GenerateHeartbeat().SerializeToJson());
                                        this.RegisterInstanceWithRouter(instance);
                                        this.droplets.ScheduleSnapshotAppState();
                                    }
                                }
                                else
                                {
                                    Logger.Warning(Strings.GivingUpOnConnectingApp);
                                    this.StopDroplet(instance);
                                }
                            }
                            finally
                            {
                                instance.Lock.ExitWriteLock();
                            }
                        }
                    );
            });
        }

        /// <summary>
        /// Detects the if an app is ready and run the callback.
        /// </summary>
        /// <param name="instance">The instance to be checked.</param>
        /// <param name="callBack">The call back.</param>
        private void DetectAppReady(DropletInstance instance, BoolStateBlockCallback callBack)
        {
            DetectPortReady(instance, callBack);
        }


        /// <summary>
        /// Detects if an application has the port ready and then invoke the call back.
        /// </summary>
        /// <param name="instance">The instance to be checked.</param>
        /// <param name="callBack">The call back.</param>
        private static void DetectPortReady(DropletInstance instance, BoolStateBlockCallback callBack)
        {
            int port = instance.Properties.Port;

            int attempts = 0;
            bool keep_going = true;
            while (attempts <= 1000 && instance.Properties.State == DropletInstanceState.Starting && keep_going == true)
            {
                if (instance.IsPortReady)
                {
                    keep_going = false;
                    callBack(true);
                }
                else
                {
                    Thread.Sleep(100);
                    attempts++;
                }
            }

            if (keep_going)
            {
                callBack(false);
            }
        }


        /// <summary>
        /// Setups the instance environment variables to be passed when configuring the plugin of an instance.
        /// </summary>
        /// <param name="instance">The instance for which to generate the variables.</param>
        /// <param name="appVars">The application variables.</param>
        /// <param name="services">The services to be bound to the instance.</param>
        /// <returns>The application variables.</returns>
        private Dictionary<string, string> SetupInstanceEnv(DropletInstance instance, string[] appVars, Dictionary<string, object>[] services)
        {
            Dictionary<string, string> env = new Dictionary<string, string>();

            env.Add(HomeVariable, instance.Properties.Directory);
            env.Add(VcapApplicationVariable, CreateInstanceVariable(instance));
            env.Add(VcapServicesVariable, CreateServicesApplicationVariable(services));
            env.Add(VcapAppHostVariable, Host);
            env.Add(VcapAppPortVariable, instance.Properties.Port.ToString());

            env.Add(VcapAppDebugIpVariable, instance.Properties.DebugIP);
            env.Add(VcapAppDebugPortVariable, instance.Properties.DebugPort != null ? instance.Properties.DebugPort.ToString() : null);

            if (instance.Properties.DebugPort != null && stager.Runtimes[instance.Properties.Runtime].DebugEnv != null)
            {
                if (stager.Runtimes[instance.Properties.Runtime].DebugEnv.ContainsKey(instance.Properties.DebugMode))
                {
                    foreach (KeyValuePair<string, string> debugEnv in stager.Runtimes[instance.Properties.Runtime].DebugEnv[instance.Properties.DebugMode])
                    {
                        env.Add(debugEnv.Key, debugEnv.Value);
                    }
                }
            }
            

            // Do the runtime environment settings
            foreach (KeyValuePair<string, string> runtimeEnv in stager.Runtimes[instance.Properties.Runtime].Environment)
            {
                env.Add(runtimeEnv.Key, runtimeEnv.Value);
            }

            // User's environment settings
            if (appVars != null)
            {
                foreach (string appEnv in appVars)
                {
                    string[] envVar = appEnv.Split(new char[] { '=' }, 2);
                    env.Add(envVar[0], envVar[1]);
                }
            }

            return env;
        }

        /// <summary>
        /// Creates the application variable for an instance. Is is used for the plugin configuration.
        /// </summary>
        /// <param name="instance">The instance for which the application variable is to be generated.</param>
        /// <returns>The application variable.</returns>
        private string CreateInstanceVariable(DropletInstance instance)
        {
            List<string> whitelist = new List<string>() { "instance_id", "instance_index", "name", "uris", "users", "version", "start", "runtime", "state_timestamp", "port" };
            Dictionary<string, object> result = new Dictionary<string, object>();

            Dictionary<string, object> jInstance = instance.Properties.ToJsonIntermediateObject();

            foreach (string key in whitelist)
            {
                if (jInstance[key] != null)
                {
                    //result[key] = JsonConvertibleObject.ObjectToValue<object>(jInstance[key]);
                    result[key] = jInstance[key];
                }
            }


            result["host"] = Host;
            result["limits"] = new Dictionary<string, object>() {
                {"fds", instance.Properties.FdsQuota},
                {"mem", instance.Properties.MemoryQuotaBytes},
                {"disk", instance.Properties.DiskQuotaBytes}
            };

            

            return JsonConvertibleObject.SerializeToJson(result);
        }


        /// <summary>
        /// Creates the services application variable used when configuring the plugin.
        /// </summary>
        /// <param name="services">The services recieved from the Cloud Controller.</param>
        /// <returns>The services application variable</returns>
        private static string CreateServicesApplicationVariable(Dictionary<string, object>[] services = null)
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


        /// <summary>
        /// Handler for router.start Nats messages.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="reply">The reply token.</param>
        /// <param name="subject">The message subject.</param>
        private void RouterStartHandler(string message, string reply, string subject)
        {
            if (shuttingDown)
                return;

            Logger.Debug(Strings.DeaReceivedRouterStart, message);
            
            droplets.ForEach(delegate(DropletInstance instance)
            {
                if (instance.Properties.State == DropletInstanceState.Running)
                {
                    RegisterInstanceWithRouter(instance);
                }
            });
        }



        /// <summary>
        /// Registers the instance with the Vcap router. Called when the application is running and ready.
        /// </summary>
        /// <param name="instance">The instance to be registerd.</param>
        private void RegisterInstanceWithRouter(DropletInstance instance)
        {
            RouterMessage response = new RouterMessage();
            try
            {
                instance.Lock.EnterReadLock();

                if (instance.Properties.Uris == null || instance.Properties.Uris.Length == 0) return;

                response.DeaId = Uuid;
                response.Host = Host;
                response.Port = Port;
                response.Uris = new List<string>(instance.Properties.Uris).ToArray();

                response.Tags = new RouterMessage.TagsObject();
                response.Tags.Framework = instance.Properties.Framework;
                response.Tags.Runtime = instance.Properties.Runtime;
            }
            finally
            {
                instance.Lock.ExitReadLock();
            }

            deaReactor.SendRouterRegister(response.SerializeToJson());
        }


        /// <summary>
        /// Unregisters the instance from the Vcap router. Called when the applicatoin is not in a running state any more.
        /// </summary>
        /// <param name="instance">The instance.</param>
        private void UnregisterInstanceFromRouter(DropletInstance instance)
        {
            RouterMessage response = new RouterMessage();
            try
            {
                instance.Lock.EnterReadLock();

                if (instance.Properties.Uris == null || instance.Properties.Uris.Length == 0) return;

                response.DeaId = Uuid;
                response.Host = Host;
                response.Port = Port;
                response.Uris = instance.Properties.Uris;

                response.Tags = new RouterMessage.TagsObject();
                response.Tags.Framework = instance.Properties.Framework;
                response.Tags.Runtime = instance.Properties.Runtime;
            }
            finally
            {
                instance.Lock.ExitReadLock();
            }

            deaReactor.SendRouterUnregister(response.SerializeToJson());
        }


        /// <summary>
        /// The handler for healthmanager.start Nats message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="replay">The replay token.</param>
        /// <param name="subject">The message subject.</param>
        private void HealthmanagerStartHandler(string message, string replay, string subject)
        {
            if (shuttingDown)
                return;

            Logger.Debug(Strings.DeaReceivedHealthmanagerStart, message);

            SendHeartbeat();
        }

        /// <summary>
        /// Checks the system resource usage (cpu, memeory, disk size) and associate the respective counters to each instance.
        /// Stop an instance if it's usage is above its quota.
        /// Update the varz with resouce usage.
        /// </summary>
        private void MonitorApps()
        {
            //AgentMonitoring.MemoryUsageKbytes = 0;
            long memoryUsageKbytes = 0;
            List<object> runningApps = new List<object>();

            if (droplets.NoMonitorableApps())
            {
                monitoring.MemoryUsageKbytes = 0;
                return;
            }
                        
            DateTime processInfoStart = DateTime.Now;

            ProcessData[] processStatuses = ProcessInformation.GetProcessUsage();

            TimeSpan elapsed = DateTime.Now - processInfoStart;
            if (elapsed.TotalMilliseconds > 800) 
                Logger.Warning(Strings.TookXSecondsToExecutePs, elapsed.TotalSeconds, processStatuses.Length);

            Dictionary<int, ProcessData> pidInfo = new Dictionary<int, ProcessData>();
            foreach (ProcessData processStatus in processStatuses)
            {
                pidInfo[processStatus.ProcessId] = processStatus;
            }

            DateTime diskUsageStart = DateTime.Now;

            DiskUsageEntry[] duAll = DiskUsage.GetDiskUsage(stager.AppsDir, false);
    
            TimeSpan diskUsageElapsed = DateTime.Now - diskUsageStart;

            if (diskUsageElapsed.TotalMilliseconds > 800)
            {
                Logger.Warning(Strings.TookXSecondsToExecuteDu, diskUsageElapsed.TotalSeconds);
                if ((diskUsageElapsed.TotalSeconds > 10) && ((DateTime.Now - monitoring.LastAppDump).TotalSeconds > Monitoring.AppsDumpIntervalMilliseconds))
                {
                    monitoring.DumpAppsDirDiskUsage(stager.AppsDir);
                    monitoring.LastAppDump = DateTime.Now;
                }
            }

            Dictionary<string, long> diskUsageHash = new Dictionary<string, long>();
            foreach (DiskUsageEntry entry in duAll)
            {
                diskUsageHash[entry.Directory] = entry.SizeKB * 1024;
            }

            Dictionary<string, Dictionary<string, Dictionary<string, long>>> metrics = new Dictionary<string, Dictionary<string, Dictionary<string, long>>>() 
            {
                {"framework", new Dictionary<string, Dictionary<string, long>>()}, 
                {"runtime", new Dictionary<string, Dictionary<string, long>>()}
            };
            
            droplets.ForEach(true, delegate(DropletInstance instance)
            {

                if (!instance.Lock.TryEnterWriteLock(10)) return;

                try
                {
                    
                    //todo: consider only checking for starting and running apps

                    try
                    {
                        instance.Properties.ProcessId = instance.Plugin.GetApplicationProcessId();
                    }
                    catch (Exception ex)
                    {
                        if (instance.ErrorLog != null) instance.ErrorLog.Error(ex.ToString());
                    }

                    int pid = instance.Properties.ProcessId;
                    if ((pid != 0 && pidInfo.ContainsKey(pid)) || instance.IsPortReady)
                    {

                        long memBytes = pid != 0 && pidInfo.ContainsKey(pid) ? (long)pidInfo[pid].WorkingSetBytes : 0;
                        long cpu = pid != 0 && pidInfo.ContainsKey(pid) ? (long)pidInfo[pid].Cpu : 0;
                        long diskBytes = diskUsageHash.ContainsKey(instance.Properties.Directory) ? diskUsageHash[instance.Properties.Directory] : 0;

                        instance.AddUsage(memBytes, cpu, diskBytes);

                        if (secure)
                        {
                            CheckUsage(instance);
                        }

                        memoryUsageKbytes += memBytes / 1024;

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

                            metric["used_memory"] += memBytes / 1024;
                            metric["reserved_memory"] += instance.Properties.MemoryQuotaBytes / 1024;
                            metric["used_disk"] += diskBytes;
                            metric["used_cpu"] += cpu;
                        }

                        // Track running apps for varz tracking
                        runningApps.Add(instance.Properties.ToJsonIntermediateObject());
                    }
                    else
                    {

                        instance.Properties.ProcessId = 0;
                        if (instance.Properties.State == DropletInstanceState.Running)
                        {
                            if (!instance.IsPortReady)
                            {
                                StopDroplet(instance);
                            }
                        }

                    }
                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }
            });

            // export running app information to varz
            try
            {
                VarzLock.EnterWriteLock();
                Varz["running_apps"] = runningApps;
                Varz["frameworks"] = metrics["framework"];
                Varz["runtimes"] = metrics["runtime"];
            }
            finally
            {
                VarzLock.ExitWriteLock();
            }

            TimeSpan ttlog = DateTime.Now - processInfoStart;
            if (ttlog.TotalMilliseconds > 1000)
            {
                Logger.Warning(Strings.TookXSecondsToProcessPsAndDu, ttlog.TotalSeconds);
            }
        }



        /// <summary>
        /// Checks the usage of the instance. If it has a usage above the quota and the DEA is in secure mode, the instance will be stopped.
        /// </summary>
        /// <param name="instance">The instance to checks.</param>
        private void CheckUsage(DropletInstance instance)
        {
            DropletInstanceUsage curUsage = instance.Properties.LastUsage;
            if (curUsage == null) return;

            if (instance == null || curUsage == null)
                return;

            // Check Mem
            if (curUsage.MemoryKbytes > (instance.Properties.MemoryQuotaBytes / 1024))
            {
                FileLogger logger = new FileLogger(Path.Combine(instance.Properties.Directory, "logs\\err.log"));

                logger.Fatal(Strings.MemoryLimitOfExceeded, instance.Properties.MemoryQuotaBytes / 1024 / 1024);
                logger.Fatal(Strings.ActualUsageWasProcessTerminated, curUsage.MemoryKbytes / 1024);
                StopDroplet(instance);
            }

            // Check Disk
            if (curUsage.DiskBytes > instance.Properties.DiskQuotaBytes)
            {
                FileLogger logger = new FileLogger(Path.Combine(instance.Properties.Directory, "logs\\err.log"));
                logger.Fatal(Strings.DiskUsageLimitOf, instance.Properties.DiskQuotaBytes / 1024 / 1024);
                logger.Fatal(Strings.ActualUsageWasProcessTerminated, curUsage.DiskBytes / 1024 / 1024);
                StopDroplet(instance);
            }

            // Check CPU
            if (instance.Usage.Count == 0)
            {
                return;
            }

            if (curUsage.Cpu > Monitoring.BeginReniceCpuThreshold)
            {
                int nice = instance.Properties.Nice + 1;
                if (nice < Monitoring.MaxReniceValue)
                {
                    instance.Properties.Nice = nice;
                    ProcessPriorityClass priority = 
                        nice == 0 ? ProcessPriorityClass.RealTime : nice == 1 ? ProcessPriorityClass.High :
                        nice == 2 ? ProcessPriorityClass.AboveNormal : nice == 3 ? ProcessPriorityClass.Normal : 
                        nice == 4 ? ProcessPriorityClass.BelowNormal : ProcessPriorityClass.Idle;

                    Logger.Info(Strings.LoweringPriorityOnCpuBound, instance.Properties.Name, priority);

                    //TODO: vladi: make sure this works on Windows
                    Process.GetProcessById(instance.Properties.ProcessId).PriorityClass = priority;
                }
            }

            // TODO, Check for an attack, or what looks like one, and look at history?
            // pegged_cpus = @num_cores * 100
        }


        /// <summary>
        /// Does all the cleaning that is needed for an instance if stopped gracefully or has crashed
        /// </summary>
        private void TheReaper()
        {

            droplets.ForEach(true, delegate(DropletInstance instance)
            {
                if (!instance.Lock.TryEnterWriteLock(10)) return;

                bool removeDroplet = false;

                try
                {
                    bool isCrashed = instance.Properties.State == DropletInstanceState.Crashed;
                    bool isOldCrash = instance.Properties.State == DropletInstanceState.Crashed && (DateTime.Now - instance.Properties.StateTimestamp).TotalMilliseconds > Monitoring.CrashesReaperTimeoutMilliseconds;
                    bool isStopped = instance.Properties.State == DropletInstanceState.Stopped;
                    bool isDeleted = instance.Properties.State == DropletInstanceState.Deleted;


                    //Remove the instance system resources, except the instance directory
                    if (isCrashed || isOldCrash || isStopped || isDeleted)
                    {
                        Logger.Debug(Strings.CrashesReaperDeleted, instance.Properties.InstanceId);

                        if (instance.Plugin != null)
                        {
                            try
                            {
                                monitoring.RemoveInstanceResources(instance);
                                instance.Plugin.CleanupApplication(instance.Properties.Directory);
                                instance.Plugin = null;
                                WindowsVCAPUsers.DeleteUser(instance.Properties.InstanceId);
                            }
                            catch (Exception ex)
                            {
                                instance.ErrorLog.Error(ex.ToString());
                            }
                        }
                    }

                    //Remove the instance directory, including the logs
                    if (isOldCrash || isStopped || isDeleted)
                    {

                        if (disableDirCleanup) instance.Properties.Directory = null;
                        if (instance.Properties.Directory != null && instance.Plugin == null)
                        {
                            try
                            {
                                Directory.Delete(instance.Properties.Directory, true);
                                instance.Properties.Directory = null;
                            }
                            catch { }
                        }

                        if (instance.Plugin == null && instance.Properties.Directory == null)
                        {
                            removeDroplet = true;
                        }

                    }

                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }

                //If the remove droplet flag was set, delete the instance form the Dea. The removal is made here to avoid dealocks.
                if (removeDroplet)
                {
                    droplets.RemoveDropletInstance(instance);
                }

            });


        }
    }
}
