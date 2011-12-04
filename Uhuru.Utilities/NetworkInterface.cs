using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Uhuru.Utilities
{
    /// <summary>
    /// Helper class used to retrieve a local ip; this is useful when the machine has multiple NICs
    /// </summary>
    public static class NetworkInterface
    {
        /// <summary>
        /// Get an IP using 198.41.0.4 as a target for openining a socket.
        /// </summary>
        /// <returns>The IP used by the OS to connect to 198.41.0.4</returns>
        public static string GetLocalIPAddress()
        {
            return GetLocalIPAddress("198.41.0.4");
        }

        /// <summary>
        /// Returns the IP used by the OS to connect to the RouteIPAddress.
        /// Pointing to a interface address will return that address.
        /// </summary>
        /// <param name="routeIPAddress">The route IP address.</param>
        /// <returns>The local IP address.</returns>
        public static string GetLocalIPAddress(string routeIPAddress)
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
