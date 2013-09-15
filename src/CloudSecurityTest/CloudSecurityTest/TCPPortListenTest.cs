using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudSecurityTest
{


    class TCPPortListenTest : SecurityTest, ISecurityTest
    {

        public bool IsLowerScoreBetter
        {
            get { return true; }
        }

        public string Description
        {
            get { return "Test if an app can open ports other than the one assigned (9911)."; }
        }

        public string Metric
        {
            get { return "Number of invalid ports opened."; }
        }

        public void RunTest()
        {
            int[] ports = new int[] { 80, 22, 1, 100, 3389, 9999, 50000, 43000, 0 };

            int score = 0;
            foreach (int port in ports)
            {
                try
                {
                    Message(@"Trying to open port '{0}'", port);


                    TcpListener actualServer = null;

                    try
                    {
                        int actualServerPort = port;
                        actualServer = new TcpListener(IPAddress.Parse("127.0.0.1"), actualServerPort);

                        using (TcpClient actualClient = new TcpClient())
                        {
                            actualServer.Start();
                            actualClient.Connect(IPAddress.Parse("127.0.0.1"), port);

                            using (TcpClient acceptedClient = actualServer.AcceptTcpClient())
                            {

                                byte[] sentBytes = new byte[] { 0, 1, 2, 3, 4 };
                                actualClient.GetStream().Write(sentBytes, 0, 5);
                                byte[] receivedBytes = new byte[5];
                                int receivedBytesCount = acceptedClient.GetStream().Read(receivedBytes, 0, 5);
                            }
                        }
                    }
                    finally
                    {
                        if (actualServer != null)
                        {
                            actualServer.Stop();
                        }
                    }


                    score += 1;
                }
                catch { }
                Score(score);
            }
        }
    }
}
