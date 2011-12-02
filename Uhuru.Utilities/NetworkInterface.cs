using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Uhuru.Utilities
{
    public class NetworkInterface
    {
        //returns the ip used by the OS to connect to the RouteIPAddress. Pointing to a interface address will return that address
        public static string GetLocalIpAddress(string RouteIPAddress = "198.41.0.4")
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(RouteIPAddress, 1);
            IPEndPoint ep = (IPEndPoint)udpClient.Client.LocalEndPoint;
            udpClient.Close();
            return ep.Address.ToString();
        }
    }
}
