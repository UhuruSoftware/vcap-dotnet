using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Uhuru.Utilities;
using Uhuru.Utilities.ProcessPerformance;

namespace Uhuru.CloudFoundry.DEA
{
    public class DropletInstance
    {



        public ReaderWriterLockSlim Lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public DropletInstanceProperties Properties = new DropletInstanceProperties();
        public List<DropletInstanceUsage> Usage = new List<DropletInstanceUsage>();

        
        public const int MaxUsageSamples = 30;

        public bool IsRunning
        {
            get
            {
                if (Properties.Pid == 0)
                    return false;

                return ProcessInformation.GetProcessUsage(Properties.Pid) != null;
            }
        }

        
        public HearbeatMessage.InstanceHeartbeat GenerateInstanceHearbeat()
        {
            HearbeatMessage.InstanceHeartbeat beat = new HearbeatMessage.InstanceHeartbeat();
            try
            {
                Lock.EnterReadLock();
                
                beat.DropletId = Properties.DropletId;
                beat.Version = Properties.Version;
                beat.InstanceId = Properties.InstanceId;
                beat.InstanceIndex = Properties.InstanceIndex;
                beat.State = Properties.State;

            }
            finally
            {
                Lock.ExitReadLock();
            }
            return beat;
        }

        public HearbeatMessage GenerateHeartbeat()
        {
            HearbeatMessage response = new HearbeatMessage();
            response.Droplets.Add(GenerateInstanceHearbeat().ToJsonIntermediateObject());
            return response;
        }

        /// <summary>
        /// returns the instances exited message
        /// </summary>
        public DropletExitedMessage GenerateDropletExitedMessage()
        {
            DropletExitedMessage response = new DropletExitedMessage();

            try
            {
                Lock.EnterReadLock();
                response.DropletId = Properties.DropletId;
                response.Version = Properties.Version;
                response.InstanceId = Properties.InstanceId;
                response.Index = Properties.InstanceIndex;
                response.ExitReason = Properties.ExitReason;

                if (Properties.State == DropletInstanceState.CRASHED)
                    response.CrashedTimestamp = Properties.StateTimestamp;

            }
            finally
            {
                Lock.ExitReadLock();
            }

            return response;

        }

        public DropletStatusMessageResponse GenerateDropletStatusMessage()
        {
            DropletStatusMessageResponse response = new DropletStatusMessageResponse();

            try
            {
                Lock.EnterReadLock();
                response.Name = Properties.Name;
                response.Port = Properties.Port;
                response.Uris = Properties.Uris;
                response.Uptime = (DateTime.Now - Properties.Start).TotalSeconds;
                response.MemoryQuotaBytes = Properties.MemoryQuotaBytes;
                response.DiskQuotaBytes = Properties.DiskQuotaBytes;
                response.FdsQuota = Properties.FdsQuota;
                if (Usage.Count > 0)
                {
                    response.Usage = Usage[Usage.Count - 1];
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }

            return response;
        }

        public void GenerateDeaFindDropletResponse()
        {
            throw new System.NotImplementedException();
        }

        public void StopDroplet()
        {
            throw new System.NotImplementedException();
        }

        public void SetupInstanceEnvironment(List<string> app_env, List<Dictionary<string, object>> services)
        {
            /*
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

            return env;*/
        }

        private void CreateDebugForEnvironment()
        {
            throw new System.NotImplementedException();
        }

        private void CreateInstanceForEnvironment()
        {
            throw new System.NotImplementedException();
        }

        private void CreateLegacyServicesForEnvironment()
        {
            throw new System.NotImplementedException();
        }

        private void CreateServicesForEnvironment()
        {
            throw new System.NotImplementedException();
        }

        public void DetectAppPid()
        {
            throw new System.NotImplementedException();
        }

        public void DetectAppReady()
        {
            throw new System.NotImplementedException();
        }
    }
}
