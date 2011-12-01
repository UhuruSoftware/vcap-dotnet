using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA
{
    public class DropletCollection
    {

        public string AppStateFile { get; set; }

        public object GenerateHeartbeat()
        {
            throw new System.NotImplementedException();
        }

        public bool NoMonitorableApps()
        {
            throw new System.NotImplementedException();
        }

        public void MonitorApps()
        {
            throw new System.NotImplementedException();
        }

        public void CrashesReaper()
        {
            throw new System.NotImplementedException();
        }

        public void SnapshotAppState()
        {
            throw new System.NotImplementedException();
        }
    }
}
