// -----------------------------------------------------------------------
// <copyright file="Agent.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using DiskQuotaTypeLibrary;
    using Uhuru.CloudFoundry.DEA.Messages;
    using Uhuru.CloudFoundry.DEA.PluginBase;
    using Uhuru.Configuration;
    using Uhuru.NatsClient;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// Callback with a Boolean parameter.
    /// </summary>
    /// <param name="state">if set to <c>true</c> [state].</param>
    public delegate void BoolStateBlockCallback(bool state);

    /// <summary>
    /// The Agent class is the DEA engine. It handles all the messages it receives on the message bus and send appropriate messages when it is requested to do so,
    /// or some external event happened.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Trying to keep similarity to Ruby version.")]
    public sealed class Agent : VCAPComponent
    {
        /// <summary>
        /// The DEA version.
        /// </summary>
        private const decimal Version = 0.99m;

        /// <summary>
        /// Home variable.
        /// </summary>
        private const string HomeVariable = "HOME";

        /// <summary>
        /// Application variable.
        /// </summary>
        private const string VcapApplicationVariable = "VCAP_APPLICATION";

        /// <summary>
        /// Services variable.
        /// </summary>
        private const string VcapServicesVariable = "VCAP_SERVICES";

        /// <summary>
        /// Vcap Application Host Variable.
        /// </summary>
        private const string VcapAppHostVariable = "VCAP_APP_HOST";

        /// <summary>
        /// Vcap Application Port.
        /// </summary>
        private const string VcapAppPortVariable = "VCAP_APP_PORT";

        /// <summary>
        /// Vcap Debug Ip.
        /// </summary>
        private const string VcapAppDebugIpVariable = "VCAP_DEBUG_IP";

        /// <summary>
        /// Vcap Debug Port.
        /// </summary>
        private const string VcapAppDebugPortVariable = "VCAP_DEBUG_PORT";

        /// <summary>
        /// Vcap Plugin Stating Info.
        /// </summary>
        private const string VcapPluginStagingInfoVariable = "VCAP_PLUGIN_STAGING_INFO";

        /// <summary>
        /// Vcap Windows User.
        /// </summary>
        private const string VcapWindowsUserVariable = "VCAP_WINDOWS_USER";

        /// <summary>
        /// Vcap Windows User Password.
        /// </summary>
        private const string VcapWindowsUserPasswordVariable = "VCAP_WINDOWS_USER_PASSWORD";

        /// <summary>
        /// Vcap Application Pid.
        /// </summary>
        private const string VcapAppPidVariable = "VCAP_APP_PID";

        /// <summary>
        /// The the droplets the DEA manages.
        /// </summary>
        private DropletCollection droplets = new DropletCollection();

        /// <summary>
        /// The application stager.
        /// </summary>
        private Stager stager = new Stager();

        /// <summary>
        /// The DEA's HTTP droplet file viewer. Helps receive the logs.
        /// </summary>
        private FileViewer fileViewer = new FileViewer();

        /// <summary>
        /// The monitoring resource.
        /// </summary>
        private Monitoring monitoring = new Monitoring();

        /// <summary>
        /// Set to true when more applications are allowed to be hosted on the DEA.
        /// </summary>
        private bool multiTenant;

        /// <summary>
        /// If secure mode is enabled.
        /// </summary>
        private bool secure;

        /// <summary>
        /// If the enforcement of usage limit is enabled.
        /// </summary>
        private bool enforceUlimit;

        /// <summary>
        /// If the enforcement of usage limit is enabled.
        /// </summary>
        private bool useDiskQuota;

        /// <summary>
        /// When a DEA has the "prod" flag set to true, only the apps that have
        /// "prod" flag will be run on that DEA.  When the DEA's prod flag is
        /// set to false (default value), the prod flag of the application is
        /// ignored.
        /// </summary>
        private bool onlyProductionApps;

        /// <summary>
        /// The DEA reactor. Is is the middleware to the message bus. 
        /// </summary>
        private DeaReactor deaReactor;

        /// <summary>
        /// The hello message send to the message bus.
        /// </summary>
        private HelloMessage helloMessage = new HelloMessage();

        /// <summary>
        /// Flag set to true when the system is shutting down. It used to avoiding processing some routines when the DEA is preparing to shut down.
        /// </summary>
        private volatile bool shuttingDown = false;

        /// <summary>
        /// The delay
        /// </summary>
        private int evacuationDelayMs = 30 * 1000;

        /// <summary>
        /// Maximum number of droplets that can be in a starting state. After that dea.discover messages are ingored.
        /// </summary>
        private int maxConcurrentStarts;

        /// <summary>
        /// The windows disk quota manager.
        /// </summary>
        private DIDiskQuotaControl diskQuotaControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class. Loads the configuration and initializes the members.
        /// </summary>
        public Agent()
        {
            UhuruSection uhuruSection = (UhuruSection)ConfigurationManager.GetSection("uhuru");

            foreach (RuntimeElement deaConf in uhuruSection.DEA.Runtimes)
            {
                DeaRuntime dea = new DeaRuntime();

                dea.Executable = deaConf.Executable;
                dea.Version = deaConf.Version;
                dea.VersionArgument = deaConf.VersionArgument;
                dea.AdditionalChecks = deaConf.AdditionalChecks;
                dea.Enabled = true;

                foreach (EnvironmentElement ienv in deaConf.Environment)
                {
                    dea.Environment.Add(ienv.Name, ienv.Value);
                }

                foreach (DebugElement debugEnv in deaConf.Debug)
                {
                    dea.DebugEnvironmentVariables.Add(debugEnv.Name, new Dictionary<string, string>());
                    foreach (EnvironmentElement ienv in debugEnv.Environment)
                    {
                        dea.DebugEnvironmentVariables[debugEnv.Name].Add(ienv.Name, ienv.Value);
                    }
                }

                this.stager.Runtimes.Add(deaConf.Name, dea);
            }

            string baseDir = uhuruSection.DEA.BaseDir;
            this.stager.DropletDir = new DirectoryInfo(baseDir).FullName;

            this.stager.DisableDirCleanup = uhuruSection.DEA.DisableDirCleanup;
            this.multiTenant = uhuruSection.DEA.Multitenant;
            this.secure = uhuruSection.DEA.Secure;
            this.enforceUlimit = uhuruSection.DEA.EnforceUsageLimit;

            this.monitoring.MaxMemoryMbytes = uhuruSection.DEA.MaxMemoryMB;

            this.fileViewer.Port = uhuruSection.DEA.FilerPort;

            this.useDiskQuota = uhuruSection.DEA.UseDiskQuota;
            this.onlyProductionApps = uhuruSection.DEA.OnlyProductionApps;

            this.maxConcurrentStarts = uhuruSection.DEA.MaxConcurrentStarts;

            // Replace the ephemeral monitoring port with the configured one
            if (uhuruSection.DEA.StatusPort > 0)
            {
                this.Port = uhuruSection.DEA.StatusPort;
            }

            this.stager.ForceHttpFileSharing = false;

            this.ComponentType = "DEA";
            if (uhuruSection.DEA.Index >= 0)
            {
                this.Index = uhuruSection.DEA.Index;
            }
            
            if (this.Index != null)
            {
                this.UUID = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", this.Index, this.UUID);
            }

            // apps_dump_dir = ConfigurationManager.AppSettings["logFile"] ?? Path.GetTempPath();
            this.monitoring.AppsDumpDirectory = Path.GetTempPath();

            this.monitoring.MonitorIntervalMilliseconds = uhuruSection.DEA.HeartbeatIntervalMs;
            this.monitoring.AdvertiseIntervalMilliseconds = uhuruSection.DEA.AdvertiseIntervalMs;
                
            this.monitoring.MaxClients = this.multiTenant ? Monitoring.DefaultMaxClients : 1;

            this.stager.StagedDir = Path.Combine(this.stager.DropletDir, "staged");
            this.stager.AppsDir = Path.Combine(this.stager.DropletDir, "apps");
            this.stager.DBDir = Path.Combine(this.stager.DropletDir, "db");

            this.droplets.AppStateFile = Path.Combine(this.stager.DropletDir, "applications.json");

            this.deaReactor.UUID = this.UUID;

            this.helloMessage.Id = this.UUID;
            this.helloMessage.Host = this.Host;
            this.helloMessage.FileViewerPort = this.fileViewer.Port;
            this.helloMessage.Version = Version;
        }

        /// <summary>
        /// Runs the DEA.
        /// It prepares the NATS subscriptions, stats the NATS client, and the required timers.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Info(System.String)", Justification = "More clear"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "It is needed to capture all exceptions.")]
        public override void Run()
        {
            Logger.Info(Strings.StartingVcapDea, Version);

            this.stager.SetupRuntimes();

            Logger.Info(Strings.UsingNetwork, this.Host);
            Logger.Info(Strings.MaxMemorySetTo, this.monitoring.MaxMemoryMbytes);
            Logger.Info(Strings.UtilizingCpuCores, DEAUtilities.NumberOfCores());

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
            this.droplets.AppStateFile = Path.Combine(this.stager.DBDir, "applications.json");

            // Clean everything in the staged directory
            this.stager.CleanCacheDirectory();

            if (this.useDiskQuota)
            {
                // Initialize disk quota
                // TODO: stefi: add a disk quota control for every drive that an app can use
                Logger.Info("Initializing disk quota...");
                this.diskQuotaControl = new DiskQuotaControlClass();
                this.diskQuotaControl.Initialize(Path.GetPathRoot(this.stager.AppsDir), true);
                this.diskQuotaControl.QuotaState = QuotaStateConstants.dqStateEnforce;

                // Set to ResolveNone to prevent blocking when using account names.
                this.diskQuotaControl.UserNameResolution = UserNameResolutionConstants.dqResolveNone;
                this.diskQuotaControl.LogQuotaThreshold = true;
                this.diskQuotaControl.LogQuotaLimit = true;

                // Disable default quota limit and threshold
                this.diskQuotaControl.DefaultQuotaThreshold = -1;
                this.diskQuotaControl.DefaultQuotaLimit = -1;

                // Wait until the volume diskquota is initialized
                while (this.diskQuotaControl.QuotaFileIncomplete || this.diskQuotaControl.QuotaFileRebuilding)
                {
                    Thread.Sleep(200);
                }
            }

            this.fileViewer.Start(this.stager.AppsDir);

            this.VCAPReactor.OnNatsError += new EventHandler<ReactorErrorEventArgs>(this.NatsErrorHandler);

            this.deaReactor.OnDeaStatus += new SubscribeCallback(this.DeaStatusHandler);
            this.deaReactor.OnDropletStatus += new SubscribeCallback(this.DropletStatusHandler);
            this.deaReactor.OnDeaDiscover += new SubscribeCallback(this.DeaDiscoverHandler);
            this.deaReactor.OnDeaFindDroplet += new SubscribeCallback(this.DeaFindDropletHandler);
            this.deaReactor.OnDeaUpdate += new SubscribeCallback(this.DeaUpdateHandler);
            this.deaReactor.OnDeaLocate += new SubscribeCallback(this.DeaLocateHandler);

            this.deaReactor.OnDeaStop += new SubscribeCallback(this.DeaStopHandler);
            this.deaReactor.OnDeaStart += new SubscribeCallback(this.DeaStartHandler);

            this.deaReactor.OnRouterStart += new SubscribeCallback(this.RouterStartHandler);
            this.deaReactor.OnHealthManagerStart += new SubscribeCallback(this.HealthmanagerStartHandler);

            base.Run();  // Start the nats client

            this.RecoverExistingDroplets();

            this.DeleteUntrackedInstanceDirs();

            TimerHelper.RecurringLongCall(
                Monitoring.HeartbeatIntervalMilliseconds,
                delegate
                {
                    this.SendHeartbeat();
                });

            TimerHelper.RecurringLongCall(
                this.monitoring.AdvertiseIntervalMilliseconds,
                delegate
                {
                    this.SendAdvertise();
                });

            TimerHelper.RecurringLongCall(
                this.monitoring.MonitorIntervalMilliseconds,
                delegate
                {
                    this.MonitorApps();
                });

            TimerHelper.RecurringLongCall(
                500,
                delegate
                {
                    this.InstanceProcessMonitor();
                });

            TimerHelper.RecurringLongCall(
                Monitoring.CrashesReaperIntervalMilliseconds,
                delegate
                {
                    this.TheReaper();
                });

            TimerHelper.RecurringLongCall(
                Monitoring.VarzUpdateIntervalMilliseconds,
                delegate
                {
                    this.SnapshotVarz();
                });

            this.deaReactor.SendDeaStart(this.helloMessage.SerializeToJson());
            this.SendAdvertise();
        }

        /// <summary>
        /// Loads the saved droplet instances the last dea process has saved using the ShanpShotAppState method. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object is properly disposed on failure."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged, and error must not bubble up.")]
        public void RecoverExistingDroplets()
        {
            if (!File.Exists(this.droplets.AppStateFile))
            {
                this.droplets.RecoveredDroplets = true;
                return;
            }

            object[] instances = JsonConvertibleObject.DeserializeFromJsonArray(File.ReadAllText(this.droplets.AppStateFile));

            foreach (object obj in instances)
            {
                DropletInstance instance = null;

                try
                {
                    instance = new DropletInstance();
                    instance.Properties.FromJsonIntermediateObject(obj);
                    instance.Properties.Orphaned = true;
                    instance.Properties.ResourcesTracked = false;
                    this.monitoring.AddInstanceResources(instance);
                    instance.Properties.StopProcessed = false;
                    instance.JobObject.JobMemoryLimit = instance.Properties.MemoryQuotaBytes;

                    if (this.useDiskQuota)
                    {
                        instance.UserDiskQuota = this.diskQuotaControl.FindUser(instance.Properties.WindowsUserName);
                    }

                    try
                    {
                        instance.LoadPlugin();

                        instance.Properties.EnvironmentVariables[VcapAppPidVariable] = instance.Properties.ProcessId.ToString(CultureInfo.InvariantCulture);
                        List<ApplicationVariable> appVariables = new List<ApplicationVariable>();
                        foreach (KeyValuePair<string, string> appEnv in instance.Properties.EnvironmentVariables)
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
                        this.DetectAppReady(instance);
                    }

                    this.droplets.AddDropletInstance(instance);
                    instance = null;
                }
                catch (Exception ex)
                {
                    Logger.Warning(Strings.ErrorRecoveringDropletWarningMessage, instance.Properties.InstanceId, ex.ToString());
                }
                finally
                {
                    if (instance != null)
                    {
                        instance.Dispose();
                    }
                }
            }

            this.droplets.RecoveredDroplets = true;

            if (this.monitoring.Clients > 0)
            {
                Logger.Info(Strings.DeaRecoveredApplications, this.monitoring.Clients);
            }

            this.MonitorApps();
            this.droplets.ForEach(delegate(DropletInstance instance)
            {
                this.RegisterInstanceWithRouter(instance);
            });
            this.SendHeartbeat();
            this.droplets.ScheduleSnapshotAppState();
        }

        /// <summary>
        /// First evacuates the Instances and after a delay it's calling the shutdown.
        /// </summary>
        public void EvacuateAppsThenQuit()
        {
            this.shuttingDown = true;

            Logger.Info(Strings.Evacuatingapplications);

            this.droplets.ForEach(delegate(DropletInstance instance)
            {
                try
                {
                    instance.Lock.EnterWriteLock();
                    if (instance.Properties.State != DropletInstanceState.Crashed)
                    {
                        Logger.Debug(Strings.EvacuatingApp, instance.Properties.InstanceId);

                        instance.Properties.ExitReason = DropletExitReason.DeaEvacuation;
                        this.deaReactor.SendDropletExited(instance.GenerateDropletExitedMessage().SerializeToJson());
                        instance.Properties.Evacuated = true;
                    }
                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }
            });

            Logger.Info(Strings.SchedulingShutdownIn, this.evacuationDelayMs);

            this.droplets.ScheduleSnapshotAppState();

            TimerHelper.DelayedCall(
                this.evacuationDelayMs,
                delegate
                {
                    this.Shutdown();
                });
        }

        /// <summary>
        /// Shuts down the DEA. First it stops all the instances and then the Nats client.
        /// </summary>
        public void Shutdown()
        {
            this.shuttingDown = true;
            Logger.Info(Strings.ShuttingDownMessage);

            this.droplets.ForEach(
                true,
                delegate(DropletInstance instance)
                {
                    try
                    {
                        instance.Lock.EnterWriteLock();
                        if (instance.Properties.State != DropletInstanceState.Crashed)
                        {
                            instance.Properties.ExitReason = DropletExitReason.DeaShutdown;
                        }

                        this.StopDroplet(instance);
                    }
                    finally
                    {
                        instance.Lock.ExitWriteLock();
                    }
                });

            // Allows messages to get out.
            Thread.Sleep(500);

            this.fileViewer.Stop();
            this.deaReactor.NatsClient.Close();
            this.TheReaper();

            // Execute twice because of TryEnterWriteLock(10)
            this.TheReaper();
            this.TheReaper();

            this.droplets.ScheduleSnapshotAppState();
            Logger.Info(Strings.ByeMessage);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this.droplets != null)
                    {
                        this.droplets.Dispose();
                    }

                    if (this.fileViewer != null)
                    {
                        this.fileViewer.Dispose();
                    }

                    if (this.monitoring != null)
                    {
                        this.monitoring.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Constructs the needed reactor. In this case a DeaReactor is needed.
        /// </summary>
        protected override void ConstructReactor()
        {
            if (this.deaReactor == null)
            {
                this.deaReactor = new DeaReactor();
                this.VCAPReactor = this.deaReactor;
            }
        }

        /// <summary>
        /// Creates the services application variable used when configuring the plugin.
        /// </summary>
        /// <param name="services">The services received from the Cloud Controller.</param>
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
        /// Detects the if an app is ready and run the callback.
        /// </summary>
        /// <param name="instance">The instance to be checked.</param>
        /// <param name="callBack">The call back.</param>
        private static void DetectAppReady(DropletInstance instance, BoolStateBlockCallback callBack)
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
            int attempts = 0;
            bool keep_going = true;
            while (attempts <= 1000 && instance.Properties.State == DropletInstanceState.Starting && keep_going == true)
            {
                if (instance.IsPortReady(150))
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
        /// If there are lingering instance directories in the application directory, delete them. 
        /// </summary>
        private void DeleteUntrackedInstanceDirs()
        {
            HashSet<string> trackedInstanceDirs = new HashSet<string>();

            this.droplets.ForEach(delegate(DropletInstance instance)
            {
                trackedInstanceDirs.Add(instance.Properties.Directory);
            });

            List<string> allInstanceDirs = Directory.GetDirectories(this.stager.AppsDir, "*", SearchOption.TopDirectoryOnly).ToList();

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
                catch (IOException e)
                {
                    Logger.Warning(Strings.CloudNotRemoveInstance, dir, e.ToString());
                }
            }
        }

        /// <summary>
        /// NATS the error handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Uhuru.NatsClient.ReactorErrorEventArgs"/> instance containing the error data.</param>
        private void NatsErrorHandler(object sender, ReactorErrorEventArgs args)
        {
            // Only snapshot app state if we had a chance to recover saved state. This prevents a connect error
            // that occurs before we can recover state from blowing existing data away.
            if (this.droplets.RecoveredDroplets)
            {
                this.droplets.SnapshotAppState();
            }

            string errorThrown = args.Message == null ? string.Empty : args.Message;
            Logger.Fatal(Strings.ExitingNatsError, errorThrown);
            Environment.FailFast(string.Format(CultureInfo.InvariantCulture, Strings.NatsError, errorThrown), args.Exception);
        }

        /// <summary>
        /// Sends the heartbeat of every droplet instnace the DEA is aware of.
        /// </summary>
        private void SendHeartbeat()
        {
            string response = this.droplets.GenerateHeartbeatMessage(this.UUID, this.onlyProductionApps).SerializeToJson();
            this.deaReactor.SendDeaHeartbeat(response);
        }

        /// <summary>
        /// Sends the heartbeat of every droplet instnace the DEA is aware of.
        /// </summary>
        private void SendAdvertise()
        {
            if (this.shuttingDown || this.monitoring.Clients >= this.monitoring.MaxClients || this.monitoring.MemoryReservedMbytes >= this.monitoring.MaxMemoryMbytes)
            {
                return;
            }

            DeaAdvertiseMessage response = new DeaAdvertiseMessage();
            response.Id = this.UUID;
            response.Runtimes = this.stager.Runtimes.Select((pair) => pair.Key).ToList();
            response.AvailableMemory = this.monitoring.MaxMemoryMbytes - this.monitoring.MemoryReservedMbytes;
            response.Prod = this.onlyProductionApps;

            this.deaReactor.SendDeaAdvertise(response.SerializeToJson());
        }

        /// <summary>
        /// Snapshots the varz with basic resource information.
        /// </summary>
        private new void SnapshotVarz()
        {
            try
            {
                VarzLock.EnterWriteLock();

                base.SnapshotVarz();

                Varz["apps_max_memory"] = this.monitoring.MaxMemoryMbytes;
                Varz["apps_reserved_memory"] = this.monitoring.MemoryReservedMbytes;
                Varz["apps_used_memory"] = this.monitoring.MemoryUsageKbytes / 1024;
                Varz["num_apps"] = this.monitoring.Clients;
                if (this.shuttingDown)
                {
                    Varz["state"] = "SHUTTING_DOWN";
                }
            }
            finally
            {
                VarzLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// The hander for dea.status message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="reply">The reply.</param>
        /// <param name="subject">The subject.</param>
        private void DeaStatusHandler(string message, string reply, string subject)
        {
            Logger.Debug(Strings.DEAreceivedstatusmessage);
            DeaStatusMessageResponse response = new DeaStatusMessageResponse();

            response.Id = UUID;
            response.Host = Host;
            response.FileViewerPort = this.fileViewer.Port;
            response.Version = Version;
            response.MaxMemoryMbytes = this.monitoring.MaxMemoryMbytes;
            response.MemoryReservedMbytes = this.monitoring.MemoryReservedMbytes;
            response.MemoryUsageKbytes = this.monitoring.MemoryUsageKbytes;
            response.NumberOfClients = this.monitoring.Clients;
            if (this.shuttingDown)
            {
                response.State = "SHUTTING_DOWN";
            }

            this.deaReactor.SendReply(reply, response.SerializeToJson());
        }

        /// <summary>
        /// The handler for the droplet.status message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="reply">The reply.</param>
        /// <param name="subject">The subject.</param>
        private void DropletStatusHandler(string message, string reply, string subject)
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

        /// <summary>
        /// The handler for the dea.discover message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="reply">The reply.</param>
        /// <param name="subject">The subject.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "More clear.")]
        private void DeaDiscoverHandler(string message, string reply, string subject)
        {
            Logger.Debug(Strings.DeaReceivedDiscoveryMessage, message);
            if (this.shuttingDown || this.monitoring.Clients >= this.monitoring.MaxClients || this.monitoring.MemoryReservedMbytes > this.monitoring.MaxMemoryMbytes)
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

            // Ensure app's prod flag is set if DEA's prod flag is set
            if (this.onlyProductionApps && !pmessage.Prod)
            {
                Logger.Debug("Ignoring request, app's prod flag isn't set to true, and DEA 'only Production Apps' is.");
                return;
            }

            if (this.monitoring.MemoryReservedMbytes + pmessage.Limits.MemoryMbytes > this.monitoring.MaxMemoryMbytes)
            {
                Logger.Debug(Strings.IgnoringRequestNotEnoughMemory);
                return;
            }

            int curStartingApps = 0;

            this.droplets.ForEach(delegate(DropletInstance instance)
            {
                if (instance.Properties.State == DropletInstanceState.Starting)
                {
                    curStartingApps++;
                }
            });

            if (curStartingApps >= this.maxConcurrentStarts)
            {
                Logger.Debug("More then {0} apps starting. Ignoring request", this.maxConcurrentStarts);
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
            TimerHelper.DelayedCall(
                taintMs,
                delegate
                {
                    this.deaReactor.SendReply(reply, this.helloMessage.SerializeToJson());
                });
        }

        /// <summary>
        /// The handler for dea.find.droplet message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="reply">The reply.</param>
        /// <param name="subject">The subject.</param>
        private void DeaFindDropletHandler(string message, string reply, string subject)
        {
            if (this.shuttingDown)
            {
                return;
            }

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
                        response.DeaId = UUID;
                        response.Version = instance.Properties.Version;
                        response.DropletId = instance.Properties.DropletId;
                        response.InstanceId = instance.Properties.InstanceId;
                        response.Index = instance.Properties.InstanceIndex;
                        response.State = instance.Properties.State;
                        response.StateTimestamp = instance.Properties.StateTimestamp;
                        response.FileUri = string.Format(CultureInfo.InvariantCulture, Strings.HttpDroplets, Host, this.fileViewer.Port);
                        response.FileAuth = this.fileViewer.Credentials;
                        response.Staged = instance.Properties.Staged;
                        response.DebugIP = instance.Properties.DebugIP;
                        response.DebugPort = instance.Properties.DebugPort;

                        if (pmessage.IncludeStates && instance.Properties.State == DropletInstanceState.Running)
                        {
                            response.Stats = instance.GenerateDropletStatusMessage();
                            response.Stats.Host = Host;
                            response.Stats.Cores = DEAUtilities.NumberOfCores();
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

        /// <summary>
        /// The handler for dea.update handler.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="replay">The replay.</param>
        /// <param name="subject">The subject.</param>
        private void DeaUpdateHandler(string message, string replay, string subject)
        {
            if (this.shuttingDown)
            {
                return;
            }

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

        /// <summary>
        /// The handler for dea.locate message.
        /// The DEA should respond on dea.advertise when receiving this message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="replay">The replay.</param>
        /// <param name="subject">The subject.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Debug(System.String)", Justification = "Readable.")]
        private void DeaLocateHandler(string message, string replay, string subject)
        {
            Logger.Debug("Dea received locate message");
            this.SendAdvertise();
        }

        /// <summary>
        /// The handler for the dea.stop message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="replay">The replay.</param>
        /// <param name="subject">The subject.</param>
        private void DeaStopHandler(string message, string replay, string subject)
        {
            if (this.shuttingDown)
            {
                return;
            }

            DeaStopMessageRequest pmessage = new DeaStopMessageRequest();
            pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));

            Logger.Debug(Strings.DeaReceivedStopMessage, message);

            this.droplets.ForEach(
                true,
                delegate(DropletInstance instance)
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

        /// <summary>
        /// Stops the a droplet instance.
        /// </summary>
        /// <param name="instance">The instance to be stopped.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged, and error must not bubble up.")]
        private void StopDroplet(DropletInstance instance)
        {
            try
            {
                instance.Lock.EnterWriteLock();

                if (instance.Properties.StopProcessed)
                {
                    return;
                }

                // Unplug us from the system immediately, both the routers and health managers.
                if (!instance.Properties.NotifiedExited)
                {
                    this.UnregisterInstanceFromRouter(instance);

                    if (instance.Properties.ExitReason == null)
                    {
                        instance.Properties.ExitReason = DropletExitReason.Crashed;
                        instance.Properties.State = DropletInstanceState.Crashed;
                        instance.Properties.StateTimestamp = DateTime.Now;
                        if (!instance.IsProcessIdRunning)
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
                    //// this.ScheduleTheReaper
                }

                // this.monitoring.RemoveInstanceResources(instance);
                instance.Properties.StopProcessed = true;
            }
            catch (Exception ex)
            {
                Logger.Error(Strings.ErrorRecoveringDropletWarningMessage, instance.Properties.DropletId, instance.Properties.InstanceId, ex.ToString());
            }
            finally
            {
                instance.Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Handler for the dea.{guid}.start message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="reply">The reply.</param>
        /// <param name="subject">The subject.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Readable message."), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "No specific type known.")]
        private void DeaStartHandler(string message, string reply, string subject)
        {
            DeaStartMessageRequest pmessage;
            DropletInstance instance;

            try
            {
                this.droplets.Lock.EnterWriteLock();

                if (this.shuttingDown)
                {
                    return;
                }

                Logger.Debug(Strings.DeaReceivedStartMessage, message);

                pmessage = new DeaStartMessageRequest();

                // Environment variable ca be of any type. It comes directly from the user with no validation, sanitization or preprocessing.
                try
                {
                    pmessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(message));
                }
                catch (Exception e)
                {
                    Logger.Error("Ignoring dea.start request. Unable to parse dea.start message. Exception: {0}", e.ToString());
                    return;
                }

                long memoryMbytes = pmessage.Limits != null && pmessage.Limits.MemoryMbytes != null ? pmessage.Limits.MemoryMbytes.Value : Monitoring.DefaultAppMemoryMbytes;
                long diskMbytes = pmessage.Limits != null && pmessage.Limits.DiskMbytes != null ? pmessage.Limits.DiskMbytes.Value : Monitoring.DefaultAppDiskMbytes;
                long fds = pmessage.Limits != null && pmessage.Limits.FileDescriptors != null ? pmessage.Limits.FileDescriptors.Value : Monitoring.DefaultAppFDS;

                if (this.monitoring.MemoryReservedMbytes + memoryMbytes > this.monitoring.MaxMemoryMbytes || this.monitoring.Clients >= this.monitoring.MaxClients)
                {
                    Logger.Info(Strings.Donothaveroomforthisclient);
                    return;
                }

                if (string.IsNullOrEmpty(pmessage.SHA1) || string.IsNullOrEmpty(pmessage.ExecutableFile) || string.IsNullOrEmpty(pmessage.ExecutableUri))
                {
                    Logger.Warning(Strings.StartRequestMissingProper, message);
                    return;
                }

                if (!this.stager.RuntimeSupported(pmessage.Runtime))
                {
                    Logger.Warning(Strings.CloudNotStartRuntimeNot, message);
                    return;
                }

                // Ensure app's prod flag is set if DEA's prod flag is set
                if (this.onlyProductionApps && !pmessage.Prod)
                {
                    Logger.Info("Ignoring request, app's prod flag isn't set to true, and DEA 'only Production Apps' is.");
                    return;
                }

                instance = this.droplets.CreateDropletInstance(pmessage);
                instance.Properties.MemoryQuotaBytes = memoryMbytes * 1024 * 1024;
                instance.Properties.DiskQuotaBytes = diskMbytes * 1024 * 1024;
                instance.Properties.FDSQuota = fds;
                instance.Properties.Staged = instance.Properties.Name + "-" + instance.Properties.InstanceIndex + "-" + instance.Properties.InstanceId;
                instance.Properties.Directory = Path.Combine(this.stager.AppsDir, instance.Properties.Staged);

                if (!string.IsNullOrEmpty(instance.Properties.DebugMode))
                {
                    instance.Properties.DebugPort = NetworkInterface.GrabEphemeralPort();
                    instance.Properties.DebugIP = Host;
                }

                instance.Properties.Port = NetworkInterface.GrabEphemeralPort();
                instance.Properties.EnvironmentVariables = this.SetupInstanceEnv(instance, pmessage.Environment, pmessage.Services);

                if (this.enforceUlimit)
                {
                    instance.JobObject.JobMemoryLimit = instance.Properties.MemoryQuotaBytes;
                }

                this.monitoring.AddInstanceResources(instance);
            }
            finally
            {
                this.droplets.Lock.ExitWriteLock();
            }

            // toconsider: the pre-starting stage should be able to gracefuly stop when the shutdown flag is set
            ThreadPool.QueueUserWorkItem(delegate(object data)
            {
                this.StartDropletInstance(instance, pmessage.SHA1, pmessage.ExecutableFile, new Uri(pmessage.ExecutableUri));
            });
        }

        /// <summary>
        /// Starts the droplet instance after the basic initialization is done.
        /// </summary>
        /// <param name="instance">The instance to be started.</param>
        /// <param name="sha1">The sha1 of the droplet file.</param>
        /// <param name="executableFile">The path to the droplet file.</param>
        /// <param name="executableUri">The URI to the droplet file.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Info(System.String,System.Object[])", Justification = "More clear"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Major rewrites have to be done."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged, and error must not bubble up.")]
        private void StartDropletInstance(DropletInstance instance, string sha1, string executableFile, Uri executableUri)
        {
            try
            {
                try
                {
                    instance.Lock.EnterWriteLock();

                    instance.Properties.WindowsPassword = "P4s$" + Credentials.GenerateCredential();
                    instance.Properties.WindowsUserName = WindowsVCAPUsers.CreateDecoratedUser(instance.Properties.InstanceId, instance.Properties.WindowsPassword);
                    Logger.Info("Created Windows Local User: {0}", instance.Properties.WindowsUserName);
                }
                finally
                {
                    instance.Lock.ExitWriteLock();
                }

                string tgzFile = Path.Combine(this.stager.StagedDir, sha1 + ".tgz");
                this.stager.StageAppDirectory(executableFile, executableUri, sha1, tgzFile, instance);
                Logger.Debug(Strings.Downloadcompleate);

                string starting = string.Format(CultureInfo.InvariantCulture, Strings.StartingUpInstanceOnPort, instance.Properties.LoggingId, instance.Properties.Port);

                if (!string.IsNullOrEmpty(instance.Properties.DebugMode))
                {
                    Logger.Info(starting + Strings.WithDebuggerPort, instance.Properties.DebugPort);
                }
                else
                {
                    Logger.Info(starting);
                }

                Logger.Debug(Strings.Clients, this.monitoring.Clients);
                Logger.Debug(Strings.ReservedMemoryUsageMb, this.monitoring.MemoryReservedMbytes, this.monitoring.MaxMemoryMbytes);

                List<ApplicationVariable> appVariables = new List<ApplicationVariable>();
                try
                {
                    instance.Lock.EnterWriteLock();

                    if (this.useDiskQuota)
                    {
                        // Set the quoata limits for the windows user of the app instance
                        instance.UserDiskQuota = this.diskQuotaControl.AddUser(instance.Properties.WindowsUserName);
                        instance.UserDiskQuota.QuotaLimit = instance.Properties.DiskQuotaBytes;
                        instance.UserDiskQuota.QuotaThreshold = instance.Properties.DiskQuotaBytes;
                    }

                    instance.Properties.EnvironmentVariables.Add(VcapWindowsUserVariable, instance.Properties.WindowsUserName);
                    instance.Properties.EnvironmentVariables.Add(VcapWindowsUserPasswordVariable, instance.Properties.WindowsPassword);
                    instance.Properties.EnvironmentVariables.Add(VcapPluginStagingInfoVariable, File.ReadAllText(Path.Combine(instance.Properties.Directory, "startup")));

                    foreach (KeyValuePair<string, string> appEnv in instance.Properties.EnvironmentVariables)
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

                Logger.Debug(Strings.TookXTimeToLoadConfigureAndStartDebugMessage, (DateTime.Now - start).TotalSeconds);

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
            catch (Exception ex)
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
            ThreadPool.QueueUserWorkItem(
                delegate
                {
                    DetectAppReady(
                        instance,
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
                        });
                });
        }

        /// <summary>
        /// Setups the instance environment variables to be passed when configuring the plugin of an instance.
        /// </summary>
        /// <param name="instance">The instance for which to generate the variables.</param>
        /// <param name="appVars">The user application variables.</param>
        /// <param name="services">The services to be bound to the instance.</param>
        /// <returns>The application variables.</returns>
        private Dictionary<string, string> SetupInstanceEnv(DropletInstance instance, string[] appVars, Dictionary<string, object>[] services)
        {
            Dictionary<string, string> env = new Dictionary<string, string>();

            env.Add(HomeVariable, instance.Properties.Directory);
            env.Add(VcapApplicationVariable, this.CreateInstanceVariable(instance));
            env.Add(VcapServicesVariable, CreateServicesApplicationVariable(services));
            env.Add(VcapAppHostVariable, Host);
            env.Add(VcapAppPortVariable, instance.Properties.Port.ToString(CultureInfo.InvariantCulture));

            env.Add(VcapAppDebugIpVariable, instance.Properties.DebugIP);
            env.Add(VcapAppDebugPortVariable, instance.Properties.DebugPort != null ? instance.Properties.DebugPort.ToString() : null);

            if (instance.Properties.DebugPort != null && this.stager.Runtimes[instance.Properties.Runtime].DebugEnvironmentVariables != null)
            {
                if (this.stager.Runtimes[instance.Properties.Runtime].DebugEnvironmentVariables.ContainsKey(instance.Properties.DebugMode))
                {
                    foreach (KeyValuePair<string, string> debugEnv in this.stager.Runtimes[instance.Properties.Runtime].DebugEnvironmentVariables[instance.Properties.DebugMode])
                    {
                        env.Add(debugEnv.Key, debugEnv.Value);
                    }
                }
            }

            // Do the runtime environment settings
            foreach (KeyValuePair<string, string> runtimeEnv in this.stager.Runtimes[instance.Properties.Runtime].Environment)
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

            Dictionary<string, object> jsonInstance = instance.Properties.ToJsonIntermediateObject();

            foreach (string key in whitelist)
            {
                if (jsonInstance.ContainsKey(key))
                {
                    // result[key] = JsonConvertibleObject.ObjectToValue<object>(jInstance[key]);
                    result[key] = jsonInstance[key];
                }
            }

            result["host"] = Host;
            result["limits"] = new Dictionary<string, object>() 
            {
                { "fds", instance.Properties.FDSQuota },
                { "mem", instance.Properties.MemoryQuotaBytes },
                { "disk", instance.Properties.DiskQuotaBytes }
            };

            return JsonConvertibleObject.SerializeToJson(result);
        }

        /// <summary>
        /// Handler for router.start Nats messages.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="reply">The reply token.</param>
        /// <param name="subject">The message subject.</param>
        private void RouterStartHandler(string message, string reply, string subject)
        {
            if (this.shuttingDown)
            {
                return;
            }

            Logger.Debug(Strings.DeaReceivedRouterStart, message);

            this.droplets.ForEach(delegate(DropletInstance instance)
            {
                if (instance.Properties.State == DropletInstanceState.Running)
                {
                    this.RegisterInstanceWithRouter(instance);
                }
            });
        }

        /// <summary>
        /// Registers the instance with the Vcap router. Called when the application is running and ready.
        /// </summary>
        /// <param name="instance">The instance to be registered.</param>
        private void RegisterInstanceWithRouter(DropletInstance instance)
        {
            RouterMessage response = new RouterMessage();
            try
            {
                instance.Lock.EnterReadLock();

                if (instance.Properties.Uris == null || instance.Properties.Uris.Length == 0)
                {
                    return;
                }

                response.DeaId = UUID;
                response.DropletId = instance.Properties.DropletId;
                response.Host = Host;
                response.Port = instance.Properties.Port;
                response.Uris = new List<string>(instance.Properties.Uris).ToArray();

                response.Tags = new RouterMessage.TagsObject();
                response.Tags.Framework = instance.Properties.Framework;
                response.Tags.Runtime = instance.Properties.Runtime;

                response.PrivateInstanceId = instance.Properties.PrivateInstanceId;
            }
            finally
            {
                instance.Lock.ExitReadLock();
            }

            this.deaReactor.SendRouterRegister(response.SerializeToJson());
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

                if (instance.Properties.Uris == null || instance.Properties.Uris.Length == 0)
                {
                    return;
                }

                response.DeaId = UUID;
                response.Host = Host;
                response.Port = instance.Properties.Port;
                response.Uris = instance.Properties.Uris;

                response.Tags = new RouterMessage.TagsObject();
                response.Tags.Framework = instance.Properties.Framework;
                response.Tags.Runtime = instance.Properties.Runtime;
            }
            finally
            {
                instance.Lock.ExitReadLock();
            }

            this.deaReactor.SendRouterUnregister(response.SerializeToJson());
        }

        /// <summary>
        /// The handler for healthmanager.start Nats message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="replay">The replay token.</param>
        /// <param name="subject">The message subject.</param>
        private void HealthmanagerStartHandler(string message, string replay, string subject)
        {
            if (this.shuttingDown)
            {
                return;
            }

            Logger.Debug(Strings.DeaReceivedHealthmanagerStart, message);

            this.SendHeartbeat();
        }

        /// <summary>
        /// Checks the system resource usage (cpu, memeory, disk size) and associate the respective counters to each instance.
        /// Stop an instance if it's usage is above its quota.
        /// Update the varz with resouce usage.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Trying to keep similarity to Ruby version."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged, and error must not bubble up.")]
        private void MonitorApps()
        {
            // AgentMonitoring.MemoryUsageKbytes = 0;
            long memoryUsageKbytes = 0;
            List<object> runningApps = new List<object>();

            DateTime monitorStart = DateTime.Now;

            Dictionary<string, Dictionary<string, Dictionary<string, long>>> metrics = new Dictionary<string, Dictionary<string, Dictionary<string, long>>>() 
            {
                { "framework", new Dictionary<string, Dictionary<string, long>>() }, 
                { "runtime", new Dictionary<string, Dictionary<string, long>>() }
            };

            this.droplets.ForEach(
                true,
                delegate(DropletInstance instance)
                {
                    if (instance.Properties.State != DropletInstanceState.Running)
                    {
                        return;
                    }

                    bool isPortReady = instance.IsPortReady(1500);

                    if (!instance.Lock.TryEnterWriteLock(10))
                    {
                        return;
                    }

                    try
                    {
                        if (isPortReady)
                        {
                            long currentTicks = instance.JobObject.TotalProcessorTime.Ticks;
                            DateTime currentTicksTimestamp = DateTime.Now;

                            long lastTicks = instance.Usage.Count >= 1 ? instance.Usage[instance.Usage.Count - 1].TotalProcessTicks : 0;

                            long ticksDelta = currentTicks - lastTicks;

                            // this is the case when the cpu utilization is reported between the last sample timestamp and now
                            // DateTime lastTickTimestamp = instance.Usage.Count >= 1 ? instance.Usage[instance.Usage.Count - 1].Time : currentTicksTimestamp;
                            // long tickTimespan = (currentTicksTimestamp - lastTickTimestamp).Ticks;

                            // this is the case when the cpu utilization is reported as the total life of the app
                            long tickTimespan = (currentTicksTimestamp - instance.Properties.Start).Ticks;

                            float cpu = tickTimespan != 0 ? ((float)ticksDelta / tickTimespan) * 100 : 0;

                            // trim it to one decimal precision
                            cpu = float.Parse(cpu.ToString("F1", CultureInfo.CurrentCulture), CultureInfo.CurrentCulture);

                            long memBytes = instance.JobObject.WorkingSetMemory;

                            if (this.useDiskQuota)
                            {
                                // Invalidate will update quota used
                                instance.UserDiskQuota.Invalidate();
                            }

                            long diskBytes = instance.UserDiskQuota != null ? (long)instance.UserDiskQuota.QuotaUsed : 0;

                            instance.AddUsage(memBytes, cpu, diskBytes, currentTicks);

                            if (this.secure)
                            {
                                this.CheckUsage(instance);
                            }

                            memoryUsageKbytes += memBytes / 1024;

                            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, long>>> kvp in metrics)
                            {
                                Dictionary<string, long> metric = new Dictionary<string, long>() 
                                        {
                                            { "used_memory", 0 },
                                            { "reserved_memory", 0 },
                                            { "used_disk", 0 },
                                            { "used_cpu", 0 }
                                        };

                                if (kvp.Key == "framework")
                                {
                                    if (!metrics.ContainsKey(instance.Properties.Framework))
                                    {
                                        kvp.Value[instance.Properties.Framework] = metric;
                                    }

                                    metric = kvp.Value[instance.Properties.Framework];
                                }

                                if (kvp.Key == "runtime")
                                {
                                    if (!metrics.ContainsKey(instance.Properties.Runtime))
                                    {
                                        kvp.Value[instance.Properties.Runtime] = metric;
                                    }

                                    metric = kvp.Value[instance.Properties.Runtime];
                                }

                                metric["used_memory"] += memBytes / 1024;
                                metric["reserved_memory"] += instance.Properties.MemoryQuotaBytes / 1024;
                                metric["used_disk"] += diskBytes;
                                metric["used_cpu"] += (long)cpu;
                            }

                            // Track running apps for varz tracking
                            runningApps.Add(instance.Properties.ToJsonIntermediateObject());
                        }
                        else
                        {
                            instance.Properties.ProcessId = 0;
                            if (instance.Properties.State == DropletInstanceState.Running)
                            {
                                Logger.Warning(Strings.AppNotDetectedReady, instance.Properties.Name, instance.Properties.InstanceId);
                                this.StopDroplet(instance);
                            }
                        }
                    }
                    finally
                    {
                        instance.Lock.ExitWriteLock();
                    }
                });

            this.monitoring.MemoryUsageKbytes = memoryUsageKbytes;

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

            TimeSpan ttlog = DateTime.Now - monitorStart;
            if (ttlog.TotalMilliseconds > 1000)
            {
                Logger.Warning(Strings.TookXSecondsToProcessPsAndDu, ttlog.TotalSeconds);
            }
        }

        /// <summary>
        /// Checks the usage of the instance. If it has a usage above the quota and the DEA is in secure mode, the instance will be stopped.
        /// </summary>
        /// <param name="instance">The instance to checks.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MiB", Justification = "Correct"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.FileLogger.Fatal(System.String,System.Object[])", Justification = "More clear")]
        private void CheckUsage(DropletInstance instance)
        {
            DropletInstanceUsage curUsage = instance.Properties.LastUsage;

            if (instance == null || curUsage == null)
            {
                return;
            }

            // Check Memory
            // Memory usage also enforced by windows job object
            if (curUsage.MemoryKbytes > (instance.Properties.MemoryQuotaBytes / 1024) * 1.05)
            {
                instance.ErrorLog.Fatal(
                     "Memory size usage exceeded the limit of {0} MiB. Memory size used: {1} MiB. Stopping the app instance... :(",
                     instance.Properties.MemoryQuotaBytes / 1024 / 1024,
                     curUsage.MemoryKbytes / 1024);
                this.StopDroplet(instance);
            }

            // Check Disk
            // Disk usage also enforced by windows disk quota
            if (curUsage.DiskBytes > instance.Properties.DiskQuotaBytes * 1.05)
            {
                instance.ErrorLog.Fatal(
                    "Disk size usage exceeded the limit of {0} MiB. Disk size used: {1} MiB. Stopping the app instance... :(",
                    instance.Properties.DiskQuotaBytes / 1024 / 1024,
                    curUsage.DiskBytes / 1024 / 1024);
                this.StopDroplet(instance);
            }

            // Check CPU
            if (instance.Usage.Count == 0)
            {
                return;
            }

            if (curUsage.Cpu > Monitoring.BeginReniceCpuThreshold)
            {
                int nice = instance.Properties.Nice + 1;
                if (nice <= Monitoring.MaxReniceValue)
                {
                    instance.Properties.Nice = nice;
                    ProcessPriorityClass priority =
                        nice == 0 ? ProcessPriorityClass.Normal :
                        nice == 1 ? ProcessPriorityClass.BelowNormal :
                                    ProcessPriorityClass.Idle;

                    instance.ErrorLog.Warning(Strings.LoggerLoweringPriority, priority.ToString());
                    Logger.Info(Strings.LoweringPriorityOnCpuBound, instance.Properties.Name, priority);

                    // Process.GetProcessById(instance.Properties.ProcessId).PriorityClass = priority;
                    instance.JobObject.PriorityClass = priority;
                }
            }

            // TODO, Check for an attack, or what looks like one, and look at history?
            // pegged_cpus = @num_cores * 100
            // also check for opened handles
        }

        /// <summary>
        /// Does all the cleaning that is needed for an instance if stopped gracefully or has crashed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Warning(System.String,System.Object[])", Justification = "More clear"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged, and error must not bubble up.")]
        private void TheReaper()
        {
            this.droplets.ForEach(
                true,
                delegate(DropletInstance instance)
                {
                    // TODO: watch for race conditions
                    bool removeDroplet = false;

                    bool isCrashed = instance.Properties.State == DropletInstanceState.Crashed;
                    bool isOldCrash = instance.Properties.State == DropletInstanceState.Crashed && (DateTime.Now - instance.Properties.StateTimestamp).TotalMilliseconds > Monitoring.CrashesReaperTimeoutMilliseconds;
                    bool isStopped = instance.Properties.State == DropletInstanceState.Stopped;
                    bool isDeleted = instance.Properties.State == DropletInstanceState.Deleted;

                    // Stop the instance gracefully before cleaning up.
                    if (isStopped)
                    {
                        this.monitoring.RemoveInstanceResources(instance);
                        if (instance.Plugin != null)
                        {
                            try
                            {
                                instance.Plugin.StopApplication();
                            }
                            catch (Exception ex)
                            {
                                Logger.Warning("Unable to stop application {0}. Exception: {1}", instance.Properties.Name, ex.ToString());
                            }
                        }
                    }

                    // Remove the instance system resources, except the instance directory
                    if (isCrashed || isOldCrash || isStopped || isDeleted)
                    {
                        this.monitoring.RemoveInstanceResources(instance);
                        if (instance.Plugin != null)
                        {
                            Logger.Debug(Strings.CrashesReaperDeleted, instance.Properties.InstanceId);

                            try
                            {
                                instance.Plugin.CleanupApplication(instance.Properties.Directory);
                                UserImpersonator.DeleteUserProfile(instance.Properties.WindowsUserName, instance.Properties.WindowsPassword);
                                WindowsVCAPUsers.DeleteDecoratedBasedUser(instance.Properties.InstanceId);
                                PluginHost.RemoveInstance(instance.Plugin);
                            }
                            catch (Exception ex)
                            {
                                Logger.Warning("Unable to cleanup application {0}. Exception: {1}", instance.Properties.Name, ex.ToString());
                            }
                            finally
                            {
                                instance.Plugin = null;
                            }
                        }
                    }

                    // Remove the instance directory, including the logs
                    if (isOldCrash || isStopped || isDeleted)
                    {
                        if (this.stager.DisableDirCleanup)
                        {
                            instance.Properties.Directory = null;
                        }

                        if (instance.Properties.Directory != null && instance.Plugin == null)
                        {
                            try
                            {
                                try
                                {
                                    Directory.Delete(instance.Properties.Directory, true);
                                }
                                catch (IOException)
                                {
                                }

                                instance.Properties.Directory = null;
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                Logger.Warning("Unable to delete application directory {0}. Exception: {1}", instance.Properties.Directory, ex.ToString());
                            }
                        }

                        if (instance.Plugin == null && instance.Properties.Directory == null)
                        {
                            removeDroplet = true;
                        }
                    }

                    // If the remove droplet flag was set, delete the instance form the DEA. The removal is made here to avoid deadlocks.
                    if (removeDroplet)
                    {
                        this.monitoring.RemoveInstanceResources(instance);
                        this.droplets.RemoveDropletInstance(instance);
                    }
                });
        }

        /// <summary>
        /// Monitors the instance process and adds it to the instance JobObject.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Error(System.String,System.Object[])", Justification = "More clear."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.FileLogger.Error(System.String,System.Object[])", Justification = "More clear."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged, and error must not bubble up.")]
        private void InstanceProcessMonitor()
        {
            Dictionary<string, List<Process>> userMappedProcesses = new Dictionary<string, List<Process>>();
            Process[] pss = Process.GetProcesses();
            foreach (Process p in pss)
            {
                try
                {
                    string processUser = ProcessUser.GetProcessUser(p);
                    if (!userMappedProcesses.ContainsKey(processUser))
                    {
                        userMappedProcesses[processUser] = new List<Process>();
                    }

                    userMappedProcesses[processUser].Add(p);
                }
                catch
                {
                }
            }

            this.droplets.ForEach(
                true,
                delegate(DropletInstance instance)
                {
                    if (instance.Properties.State != DropletInstanceState.Running || !instance.Lock.TryEnterWriteLock(10))
                    {
                        return;
                    }

                    try
                    {
                        if (!userMappedProcesses.ContainsKey(instance.Properties.WindowsUserName))
                        {
                            return;
                        }

                        List<Process> usersProcesses = userMappedProcesses[instance.Properties.WindowsUserName];

                        foreach (Process instanceProcess in usersProcesses)
                        {
                            instance.Properties.ProcessId = instanceProcess.Id;
                            if (!instance.JobObject.HasProcess(instanceProcess))
                            {
                                try
                                {
                                    if (!instanceProcess.HasExited)
                                    {
                                        instance.JobObject.AddProcess(instanceProcess);
                                    }
                                }
                                catch (Win32Exception e)
                                {
                                    Logger.Warning(Strings.InstanceProcessCoudNotBeAdded, instanceProcess.Id, e.ToString());
                                    instanceProcess.Kill();
                                    if (instance.ErrorLog != null)
                                    {
                                        instance.ErrorLog.Error("Killed process {0}. Reason: Unable to sandbox the process.", instanceProcess.Id);
                                    }
                                }
                            }

                            if (instance.TotalTerminatedProcessesTracked < instance.JobObject.TotalTerminatedProcesses)
                            {
                                if (instance.ErrorLog != null)
                                {
                                    instance.ErrorLog.Error("{0} process(es) killed by the sandbox.", instance.JobObject.TotalTerminatedProcesses - instance.TotalTerminatedProcessesTracked);
                                }

                                instance.TotalTerminatedProcessesTracked = instance.JobObject.TotalTerminatedProcesses;
                            }
                            else if (instance.TotalTerminatedProcessesTracked > instance.JobObject.TotalTerminatedProcesses)
                            {
                                instance.TotalTerminatedProcessesTracked = instance.JobObject.TotalTerminatedProcesses;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Unable to add process to the job object for app: {0}. Exception: {1}", instance.Properties.Name, ex.ToString());
                    }
                    finally
                    {
                        instance.Lock.ExitWriteLock();
                    }
                });
        }
    }
}