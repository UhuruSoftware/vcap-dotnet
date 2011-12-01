using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetFwTypeLib;

namespace CloudFoundry.Net.IIS.Utilities
{
    static class FirewallTools
    {
        public static void OpenPort(int port, string applicationName)
        {
            Type NetFwOpenPortType = Type.GetTypeFromProgID("HNetCfg.FWOpenPort");
            INetFwOpenPort openPort = (INetFwOpenPort)Activator.CreateInstance(NetFwOpenPortType);
            openPort.Port = port;
            openPort.Name = applicationName;
            openPort.Enabled = true;
            openPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;

            Type NetFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
            INetFwMgr mgr = (INetFwMgr)Activator.CreateInstance(NetFwMgrType);
            INetFwOpenPorts openPorts = (INetFwOpenPorts)mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

            openPorts.Add(openPort);
        }

        public static void ClosePort(int port)
        {
            Type NetFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
            INetFwMgr mgr = (INetFwMgr)Activator.CreateInstance(NetFwMgrType);
            INetFwOpenPorts openPorts = (INetFwOpenPorts)mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;
            openPorts.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
        }
    }
}
