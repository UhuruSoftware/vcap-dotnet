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
