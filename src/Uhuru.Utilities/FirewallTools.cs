// -----------------------------------------------------------------------
// <copyright file="FirewallTools.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Globalization;
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
            Type netFwOpenPortType = Type.GetTypeFromProgID("HNetCfg.FWOpenPort");
            INetFwOpenPort openPort = (INetFwOpenPort)Activator.CreateInstance(netFwOpenPortType);
            openPort.Port = port;
            openPort.Name = applicationName;
            openPort.Enabled = true;
            openPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;

            Type netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
            INetFwMgr mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);
            INetFwOpenPorts openPorts = (INetFwOpenPorts)mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

            openPorts.Add(openPort);
        }

        /// <summary>
        /// closes a port
        /// </summary>
        /// <param name="port">the port to be closed</param>
        public static void ClosePort(int port)
        {
            Type netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
            INetFwMgr mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);
            INetFwOpenPorts openPorts = (INetFwOpenPorts)mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;
            openPorts.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
        }

        /// <summary>
        /// opens a range of firewall ports
        /// </summary>
        /// <param name="lowPort">the start port to open</param>
        /// <param name="highPort">the end port to open</param>
        /// <param name="ruleName">Firewall rule name</param>
        public static void OpenPortRange(int lowPort, int highPort, string ruleName)
        {
            Type netFwOpenPortType = Type.GetTypeFromProgID("HNetCfg.FwRule");
            INetFwRule rule = (INetFwRule)Activator.CreateInstance(netFwOpenPortType);
            rule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            rule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
            rule.Profiles = (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
            rule.LocalPorts = lowPort.ToString(CultureInfo.InvariantCulture) + "-" + highPort.ToString(CultureInfo.InvariantCulture);
            rule.Name = ruleName;
            rule.Enabled = true;

            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallPolicy.Rules.Add(rule);
        }
    }
}
