using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Uhuru.Utilities.HttpTunnel;
using System.Configuration;

namespace Uhuru.Utilities.TunnelService
{
    public class HttpTunnel : ServerEnd
    {
        public HttpTunnel()
        {
            try
            {
                string destinationIp;
                int destinationPort;
                string protocol;

                destinationIp = ConfigurationManager.AppSettings["destinationIp"];
                destinationPort = int.Parse(ConfigurationManager.AppSettings["destinationPort"]);
                protocol = ConfigurationManager.AppSettings["protocol"];

                TunnelProtocolType tunnelProtocol = (TunnelProtocolType)Enum.Parse(typeof(TunnelProtocolType), protocol);
                base.Initialize(destinationIp, destinationPort, tunnelProtocol);
            }
            catch (Exception ex)
            {
                Logger.Warning("Error initializing HttpTunnel Server end: " + ex.ToString());
            }
        }
    }
}
