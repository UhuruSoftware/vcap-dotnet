using System;
using System.Configuration;
using Uhuru.Utilities.HttpTunnel;

namespace Uhuru.Utilities.TunnelService
{
    public class HttpTunnel : ServerEnd
    {
        public HttpTunnel()
        {
            try
            {
                Logger.Info("Initializing HTTP Tunnel.");
                string destinationIp;
                int destinationPort;
                string protocol;

                destinationIp = ConfigurationManager.AppSettings["destinationIp"];
                destinationPort = int.Parse(ConfigurationManager.AppSettings["destinationPort"]);
                protocol = ConfigurationManager.AppSettings["protocol"];

                TunnelProtocolType tunnelProtocol = (TunnelProtocolType)Enum.Parse(typeof(TunnelProtocolType), protocol);
                base.Initialize(destinationIp, destinationPort, tunnelProtocol);
                Logger.Info("Finished initializing HTTP Tunnel.");
            }
            catch (Exception ex)
            {
                Logger.Warning("Error initializing HttpTunnel Server end: " + ex.ToString());
            }
        }
    }
}
