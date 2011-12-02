using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Uhuru.Utilities
{
    public static class NetworkInterface
    {
        public static string GetLocalIPAddress()
        {
            return GetLocalIPAddress("198.41.0.4");
        }

        //returns the ip used by the OS to connect to the RouteIPAddress. Pointing to a interface address will return that address
        public static string GetLocalIPAddress(string routeIPAddress = "198.41.0.4")
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Connect(routeIPAddress, 1);
                IPEndPoint ep = (IPEndPoint)udpClient.Client.LocalEndPoint;
                return ep.Address.ToString();
            }
        }
    }
}
