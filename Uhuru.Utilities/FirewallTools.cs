// -----------------------------------------------------------------------
// <copyright file="FirewallTools.cs" company="Uhuru Software">
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using NetFwTypeLib;

    /// <summary>
    /// a set of firewall-related tools
    /// </summary>
    public static class FirewallTools
    {
        /// <summary>
        /// opens a firewall port to an application
        /// </summary>
        /// <param name="port">the port to open</param>
        /// <param name="applicationName">the application to open the port to</param>
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

        /// <summary>
        /// closes a port
        /// </summary>
        /// <param name="port">the port to be closed</param>
        public static void ClosePort(int port)
        {
            Type NetFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
            INetFwMgr mgr = (INetFwMgr)Activator.CreateInstance(NetFwMgrType);
            INetFwOpenPorts openPorts = (INetFwOpenPorts)mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;
            openPorts.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
        }
    }
}
