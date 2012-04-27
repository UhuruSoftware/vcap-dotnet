using Uhuru.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Uhuru.Utilities.HttpTunnel;
using System.ServiceModel;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace Uhuru.CloudFoundry.Test.Unit
{


    /// <summary>
    ///This is a test class for HttpTunnel and is intended
    ///to contain all HttpTunnel Unit Tests
    ///</summary>
    [TestClass()]
    [DeploymentItem("log4net.config")]
    public class HttpTunnelTest
    {
        private class TunnelWCFServerTcp
        {
            ServiceHost host;

            public void StartServer(int wcfPort, int tunnelPort)
            {
                Uri baseAddress = new Uri("http://127.0.0.1:" + wcfPort.ToString());

                ServerEnd service = new ServerEnd();

                BasicHttpBinding httpBinding = new BasicHttpBinding();

                host = new ServiceHost(service, baseAddress);
                host.AddServiceEndpoint(typeof(ITunnel), httpBinding, baseAddress);

                ((ServerEnd)host.SingletonInstance).Initialize("127.0.0.1", tunnelPort, TunnelProtocolType.Tcp);
                host.Open();
            }

            public void Stop()
            {
                if (host != null)
                {
                    host.Close();
                }
            }
        }

        private class TunnelWCFServerUdp
        {
            ServiceHost host;

            public void StartServer(int wcfPort, int tunnelPort, bool receiver)
            {
                Uri baseAddress = new Uri("http://127.0.0.1:" + wcfPort.ToString());

                ServerEnd service = new ServerEnd();

                BasicHttpBinding httpBinding = new BasicHttpBinding();

                host = new ServiceHost(service, baseAddress);
                host.AddServiceEndpoint(typeof(ITunnel), httpBinding, baseAddress);

                ((ServerEnd)host.SingletonInstance).Initialize("127.0.0.1", tunnelPort, receiver ? TunnelProtocolType.UdpIncoming : TunnelProtocolType.UdpOutgoing);
                host.Open();
            }

            public void Stop()
            {
                if (host != null)
                {
                    host.Close();
                }
            }
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [Timeout(10000)]
        public void TestTCPTunnelSimpleSend()
        {
            // This test makes sure that this scenario works:
            // actualClient -> clientEnd (tunnel start) -> serverEnd (tunnel end) -> actualServer

            TunnelWCFServerTcp wcfServer = new TunnelWCFServerTcp();
            ClientEnd clientEnd = null;
            TcpListener actualServer = null;

            try
            {
                // Arrange
                int wcfServerPort = NetworkInterface.GrabEphemeralPort();
                int actualServerPort = NetworkInterface.GrabEphemeralPort();
                int clientEndPort = NetworkInterface.GrabEphemeralPort();
                clientEnd = new ClientEnd();
                actualServer = new TcpListener(IPAddress.Parse("127.0.0.1"), actualServerPort);

                using (TcpClient actualClient = new TcpClient())
                {

                    // Act
                    actualServer.Start();
                    wcfServer.StartServer(wcfServerPort, actualServerPort);
                    clientEnd.Start(new Uri("http://127.0.0.1:" + wcfServerPort.ToString()), clientEndPort, "127.0.0.1", TunnelProtocolType.Tcp);
                    actualClient.Connect(IPAddress.Parse("127.0.0.1"), clientEndPort);

                    using (TcpClient acceptedClient = actualServer.AcceptTcpClient())
                    {

                        byte[] sentBytes = new byte[] { 0, 1, 2, 3, 4 };
                        actualClient.GetStream().Write(sentBytes, 0, 5);
                        byte[] receivedBytes = new byte[5];
                        int receivedBytesCount = acceptedClient.GetStream().Read(receivedBytes, 0, 5);

                        // Assert
                        Assert.AreEqual(sentBytes.Length, receivedBytesCount);
                        for (int i = 0; i < sentBytes.Length; i++)
                        {
                            Assert.AreEqual(sentBytes[i], receivedBytes[i]);
                        }
                    }
                }
            }
            finally
            {
                wcfServer.Stop();
                if (clientEnd != null)
                {
                    clientEnd.Stop();
                }
                if (actualServer != null)
                {
                    actualServer.Stop();
                }
            }
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [Timeout(10000)]
        public void TestTCPTunnelMessageReceive()
        {
            // This test makes sure that this scenario works:
            // actualClient <- clientEnd (tunnel start) <- serverEnd (tunnel end) <- actualServer

            TunnelWCFServerTcp wcfServer = new TunnelWCFServerTcp();
            ClientEnd clientEnd = null;
            TcpListener actualServer = null;

            try
            {
                // Arrange
                int wcfServerPort = NetworkInterface.GrabEphemeralPort();
                int actualServerPort = NetworkInterface.GrabEphemeralPort();
                int clientEndPort = NetworkInterface.GrabEphemeralPort();
                clientEnd = new ClientEnd();
                actualServer = new TcpListener(IPAddress.Parse("127.0.0.1"), actualServerPort);

                using (TcpClient actualClient = new TcpClient())
                {

                    // Act
                    actualServer.Start();
                    wcfServer.StartServer(wcfServerPort, actualServerPort);
                    clientEnd.Start(new Uri("http://127.0.0.1:" + wcfServerPort.ToString()), clientEndPort, "127.0.0.1", TunnelProtocolType.Tcp);
                    actualClient.Connect(IPAddress.Parse("127.0.0.1"), clientEndPort);

                    using (TcpClient acceptedClient = actualServer.AcceptTcpClient())
                    {
                        byte[] sentBytes = new byte[] { 0, 1, 2, 3, 4 };
                        acceptedClient.GetStream().Write(sentBytes, 0, 5);
                        byte[] receivedBytes = new byte[5];
                        int receivedBytesCount = actualClient.GetStream().Read(receivedBytes, 0, 5);

                        // Assert
                        Assert.AreEqual(sentBytes.Length, receivedBytesCount);
                        for (int i = 0; i < sentBytes.Length; i++)
                        {
                            Assert.AreEqual(sentBytes[i], receivedBytes[i]);
                        }
                    }
                }
            }
            finally
            {
                wcfServer.Stop();
                if (clientEnd != null)
                {
                    clientEnd.Stop();
                }
                if (actualServer != null)
                {
                    actualServer.Stop();
                }
            }
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [Timeout(10000)]
        public void TestTCPTunnelMessageSendReceive()
        {
            // This test makes sure that this scenario works:
            // actualClient <- clientEnd (tunnel start) <- serverEnd (tunnel end) <- actualServer
            // actualClient -> clientEnd (tunnel start) -> serverEnd (tunnel end) -> actualServer

            TunnelWCFServerTcp wcfServer = new TunnelWCFServerTcp();
            ClientEnd clientEnd = null;
            TcpListener actualServer = null;

            try
            {
                // Arrange
                int wcfServerPort = NetworkInterface.GrabEphemeralPort();
                int actualServerPort = NetworkInterface.GrabEphemeralPort();
                int clientEndPort = NetworkInterface.GrabEphemeralPort();
                clientEnd = new ClientEnd();
                actualServer = new TcpListener(IPAddress.Parse("127.0.0.1"), actualServerPort);

                using (TcpClient actualClient = new TcpClient())
                {

                    // Act
                    actualServer.Start();
                    wcfServer.StartServer(wcfServerPort, actualServerPort);
                    clientEnd.Start(new Uri("http://127.0.0.1:" + wcfServerPort.ToString()), clientEndPort, "127.0.0.1", TunnelProtocolType.Tcp);
                    actualClient.Connect(IPAddress.Parse("127.0.0.1"), clientEndPort);

                    using (TcpClient acceptedClient = actualServer.AcceptTcpClient())
                    {

                        byte[] sentBytesFrom = new byte[] { 0, 1, 2, 3, 4 };
                        acceptedClient.GetStream().Write(sentBytesFrom, 0, 5);
                        byte[] receivedBytesFrom = new byte[5];

                        byte[] sentBytesTo = new byte[] { 0, 1, 2, 3, 4 };
                        actualClient.GetStream().Write(sentBytesTo, 0, 5);
                        byte[] receivedBytesTo = new byte[5];


                        int receivedBytesCountFrom = actualClient.GetStream().Read(receivedBytesFrom, 0, 5);
                        int receivedBytesCountTo = acceptedClient.GetStream().Read(receivedBytesTo, 0, 5);

                        // Assert
                        Assert.AreEqual(sentBytesFrom.Length, receivedBytesCountFrom);
                        for (int i = 0; i < sentBytesFrom.Length; i++)
                        {
                            Assert.AreEqual(sentBytesFrom[i], receivedBytesFrom[i]);
                        }

                        Assert.AreEqual(sentBytesTo.Length, receivedBytesCountTo);
                        for (int i = 0; i < sentBytesTo.Length; i++)
                        {
                            Assert.AreEqual(sentBytesTo[i], receivedBytesTo[i]);
                        }
                    }
                }
            }
            finally
            {
                wcfServer.Stop();
                if (clientEnd != null)
                {
                    clientEnd.Stop();
                }
                if (actualServer != null)
                {
                    actualServer.Stop();
                }
            }
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [Timeout(12000)]
        public void TestTCPTunnelMultipleSend()
        {
            // This test makes sure that this scenario works:
            // actualClient -> clientEnd (tunnel start) -> serverEnd (tunnel end) -> actualServer
            // actualClient -> clientEnd (tunnel start) -> serverEnd (tunnel end) -> actualServer
            // actualClient -> clientEnd (tunnel start) -> serverEnd (tunnel end) -> actualServer
            // actualClient -> clientEnd (tunnel start) -> serverEnd (tunnel end) -> actualServer

            TunnelWCFServerTcp wcfServer = new TunnelWCFServerTcp();
            ClientEnd clientEnd = null;
            TcpListener actualServer = null;

            try
            {
                // Arrange
                int wcfServerPort = NetworkInterface.GrabEphemeralPort();
                int actualServerPort = NetworkInterface.GrabEphemeralPort();
                int clientEndPort = NetworkInterface.GrabEphemeralPort();
                clientEnd = new ClientEnd();
                actualServer = new TcpListener(IPAddress.Parse("127.0.0.1"), actualServerPort);

                using (TcpClient actualClient = new TcpClient())
                {

                    // Act
                    actualServer.Start();
                    wcfServer.StartServer(wcfServerPort, actualServerPort);
                    clientEnd.Start(new Uri("http://127.0.0.1:" + wcfServerPort.ToString()), clientEndPort, "127.0.0.1", TunnelProtocolType.Tcp);
                    actualClient.Connect(IPAddress.Parse("127.0.0.1"), clientEndPort);

                    using (TcpClient acceptedClient = actualServer.AcceptTcpClient())
                    {

                        byte[] sentBytes = new byte[] { 0, 1, 2, 3, 4 };

                        int writeCount = 100;

                        for (int i = 0; i < writeCount; i++)
                        {
                            actualClient.GetStream().Write(sentBytes, 0, 5);
                        }

                        // Give the packets a chance to reach their destination.
                        Thread.Sleep(2000);

                        byte[] receivedBytes = new byte[1024];
                        int receivedBytesCount = acceptedClient.GetStream().Read(receivedBytes, 0, 1024);

                        // Assert
                        Assert.AreEqual(sentBytes.Length * writeCount, receivedBytesCount);
                        for (int i = 0; i < receivedBytesCount; i++)
                        {
                            Assert.AreEqual(sentBytes[i % sentBytes.Length], receivedBytes[i], "Invalid byte at index " + i.ToString());
                        }
                    }
                }
            }
            finally
            {
                wcfServer.Stop();
                if (clientEnd != null)
                {
                    clientEnd.Stop();
                }
                if (actualServer != null)
                {
                    actualServer.Stop();
                }
            }
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [Timeout(70000)]
        public void TestTCPTunnelParallelClientsSend()
        {
            // This test makes sure that this scenario works:
            //              /-> clientEnd (tunnel start) -> serverEnd (tunnel end) \
            // actualClient -> clientEnd (tunnel start) -> serverEnd (tunnel end)  -> actualServer
            //              \-> clientEnd (tunnel start) -> serverEnd (tunnel end) /

            TunnelWCFServerTcp wcfServer = new TunnelWCFServerTcp();
            ClientEnd clientEnd = null;
            TcpListener actualServer = null;

            try
            {
                // Arrange
                int wcfServerPort = NetworkInterface.GrabEphemeralPort();
                int actualServerPort = NetworkInterface.GrabEphemeralPort();
                int clientEndPort = NetworkInterface.GrabEphemeralPort();
                clientEnd = new ClientEnd();
                actualServer = new TcpListener(IPAddress.Parse("127.0.0.1"), actualServerPort);


                // Act
                actualServer.Start();
                wcfServer.StartServer(wcfServerPort, actualServerPort);
                clientEnd.Start(new Uri("http://127.0.0.1:" + wcfServerPort.ToString()), clientEndPort, "127.0.0.1", TunnelProtocolType.Tcp);



                byte[] sentBytes = new byte[] { 0, 1, 2, 3, 4 };
                List<byte[]> receivedBytes = new List<byte[]>();
                List<int> receivedBytesCount = new List<int>();
                List<Thread> threads = new List<Thread>();
                int writeCount = 100;

                for (int i = 0; i < 20; i++)
                {
                    receivedBytes.Add(new byte[0]);
                    receivedBytesCount.Add(0);

                    Thread thread = new Thread(new ParameterizedThreadStart(
                        delegate(object indexObject)
                        {
                            int index = (int)indexObject;

                            using (TcpClient actualClient = new TcpClient())
                            {
                                actualClient.Connect(IPAddress.Parse("127.0.0.1"), clientEndPort);

                                using (TcpClient acceptedClient = actualServer.AcceptTcpClient())
                                {
                                    for (int j = 0; j < writeCount; j++)
                                    {
                                        actualClient.GetStream().Write(sentBytes, 0, 5);
                                    }

                                    // Give the packets a chance to reach their destination.
                                    Thread.Sleep(2000);

                                    byte[] localReceivedBytes = new byte[1024];
                                    int localReceivedBytesCount = acceptedClient.GetStream().Read(localReceivedBytes, 0, 1024);

                                    receivedBytes[index] = localReceivedBytes;
                                    receivedBytesCount[index] = localReceivedBytesCount;
                                }
                            }
                        }
                        ));
                    threads.Add(thread);
                    thread.IsBackground = true;
                    thread.Start(i);
                }

                for (int i = 0; i < 20; i++)
                {
                    threads[i].Join(3000);

                    // Assert
                    Assert.AreEqual(sentBytes.Length * writeCount, receivedBytesCount[i], "Byte count invalid for thread " + i);
                    for (int j = 0; j < receivedBytesCount[i]; j++)
                    {
                        Assert.AreEqual(sentBytes[j % sentBytes.Length], receivedBytes[i][j], "Invalid byte at index " + j.ToString());
                    }


                }

            }
            finally
            {
                wcfServer.Stop();
                if (clientEnd != null)
                {
                    clientEnd.Stop();
                }
                if (actualServer != null)
                {
                    actualServer.Stop();
                }
            }
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [Timeout(130000)]
        public void TestTCPTunnelParallelClientsSendReceive()
        {
            // This test makes sure that this scenario works:
            // This test makes sure that this scenario works:
            //                /-> clientEnd (tunnel start) -> serverEnd (tunnel end)\
            //               / clientEnd (tunnel start) <- serverEnd (tunnel end)  <-\
            // actualClient <-> clientEnd (tunnel start) -> serverEnd (tunnel end)  <-> actualServer
            //               \ clientEnd (tunnel start) <- serverEnd (tunnel end)  <-/
            //                \-> clientEnd (tunnel start) -> serverEnd (tunnel end)/

            TunnelWCFServerTcp wcfServer = new TunnelWCFServerTcp();
            ClientEnd clientEnd = null;
            TcpListener actualServer = null;

            try
            {
                // Arrange
                int wcfServerPort = NetworkInterface.GrabEphemeralPort();
                int actualServerPort = NetworkInterface.GrabEphemeralPort();
                int clientEndPort = NetworkInterface.GrabEphemeralPort();
                clientEnd = new ClientEnd();
                actualServer = new TcpListener(IPAddress.Parse("127.0.0.1"), actualServerPort);


                // Act
                actualServer.Start();
                wcfServer.StartServer(wcfServerPort, actualServerPort);
                clientEnd.Start(new Uri("http://127.0.0.1:" + wcfServerPort.ToString()), clientEndPort, "127.0.0.1", TunnelProtocolType.Tcp);



                byte[] sentBytes = new byte[] { 0, 1, 2, 3, 4 };
                List<byte[]> receivedBytesFrom = new List<byte[]>();
                List<int> receivedBytesCountFrom = new List<int>();

                List<byte[]> receivedBytesTo = new List<byte[]>();
                List<int> receivedBytesCountTo = new List<int>();


                List<Thread> threads = new List<Thread>();
                int writeCount = 50;

                for (int i = 0; i < 20; i++)
                {
                    receivedBytesTo.Add(new byte[0]);
                    receivedBytesCountTo.Add(0);

                    receivedBytesFrom.Add(new byte[0]);
                    receivedBytesCountFrom.Add(0);

                    Thread thread = new Thread(new ParameterizedThreadStart(
                        delegate(object indexObject)
                        {
                            int index = (int)indexObject;

                            using (TcpClient actualClient = new TcpClient())
                            {
                                actualClient.Connect(IPAddress.Parse("127.0.0.1"), clientEndPort);

                                // we may not accept the same client that we connected, but the data is the same
                                using (TcpClient acceptedClient = actualServer.AcceptTcpClient())
                                {
                                    byte[] localReceivedBytes;
                                    int localReceivedBytesCount;

                                    for (int j = 0; j < writeCount; j++)
                                    {
                                        actualClient.GetStream().Write(sentBytes, 0, 5);
                                    }

                                    // Give the packets a chance to reach their destination.
                                    Thread.Sleep(1000);

                                    localReceivedBytes = new byte[1024];
                                    localReceivedBytesCount = acceptedClient.GetStream().Read(localReceivedBytes, 0, 1024);

                                    receivedBytesFrom[index] = localReceivedBytes;
                                    receivedBytesCountFrom[index] = localReceivedBytesCount;

                                    for (int j = 0; j < writeCount; j++)
                                    {
                                        acceptedClient.GetStream().Write(sentBytes, 0, 5);
                                    }

                                    // Give the packets a chance to reach their destination.
                                    Thread.Sleep(1000);

                                    localReceivedBytes = new byte[1024];
                                    localReceivedBytesCount = actualClient.GetStream().Read(localReceivedBytes, 0, 1024);


                                    receivedBytesTo[index] = localReceivedBytes;
                                    receivedBytesCountTo[index] = localReceivedBytesCount;
                                }
                            }
                        }
                        ));
                    threads.Add(thread);
                    thread.IsBackground = true;
                    thread.Start(i);
                }

                for (int i = 0; i < 20; i++)
                {
                    threads[i].Join();
                }


                // Assert
                for (int i = 0; i < 20; i++)
                {
                    Assert.AreEqual(sentBytes.Length * writeCount, receivedBytesCountFrom[i], "Byte count invalid for thread " + i);
                    Assert.AreEqual(sentBytes.Length * writeCount, receivedBytesCountTo[i], "Byte count invalid for thread " + i);
                    for (int j = 0; j < receivedBytesCountFrom[i]; j++)
                    {
                        Assert.AreEqual(sentBytes[j % sentBytes.Length], receivedBytesFrom[i][j], "Invalid byte at index " + j.ToString());
                        Assert.AreEqual(sentBytes[j % sentBytes.Length], receivedBytesTo[i][j], "Invalid byte at index " + j.ToString());
                    }
                }

            }
            finally
            {
                wcfServer.Stop();
                if (clientEnd != null)
                {
                    clientEnd.Stop();
                }
                if (actualServer != null)
                {
                    actualServer.Stop();
                }
            }
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [Timeout(10000)]
        [Ignore]
        public void TestUDPTunnelSimpleSend()
        {
            // This test makes sure that this scenario works (for UDP):
            // actualClient -> clientEnd (tunnel start) -> serverEnd (tunnel end) -> actualServer

            TunnelWCFServerUdp wcfServer = new TunnelWCFServerUdp();
            ClientEnd clientEnd = null;
            UdpClient actualServer = null;

            try
            {
                // Arrange
                int wcfServerPort = NetworkInterface.GrabEphemeralPort();
                int actualServerPort = 9888;
                int clientEndPort = 9855;
                clientEnd = new ClientEnd();
                actualServer = new UdpClient(actualServerPort);

                using (UdpClient actualClient = new UdpClient())
                {

                    // Act
                    wcfServer.StartServer(wcfServerPort, actualServerPort, false);
                    clientEnd.Start(new Uri("http://127.0.0.1:" + wcfServerPort.ToString()), clientEndPort, "127.0.0.1", TunnelProtocolType.UdpOutgoing);
                    actualClient.Connect(IPAddress.Parse("127.0.0.1"), clientEndPort);

                    byte[] sentBytes = new byte[] { 0, 1, 2, 3, 4 };
                    actualClient.Send(sentBytes, 5);
                    byte[] receivedBytes = new byte[5];

                    IPEndPoint remoteSender = null;
                    receivedBytes = actualServer.Receive(ref remoteSender);

                    // Assert
                    Assert.AreEqual(sentBytes.Length, receivedBytes.Length);
                    for (int i = 0; i < sentBytes.Length; i++)
                    {
                        Assert.AreEqual(sentBytes[i], receivedBytes[i]);
                    }
                }
            }
            finally
            {
                wcfServer.Stop();
                if (clientEnd != null)
                {
                    clientEnd.Stop();
                }
                if (actualServer != null)
                {
                    actualServer.Close();
                }
            }
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [Timeout(10000)]
        [Ignore]
        public void TestUDPTunnelSimpleReceive()
        {
            // This test makes sure that this scenario works (for UDP):
            // actualClient <- clientEnd (tunnel start) <- serverEnd (tunnel end) <- actualServer

            TunnelWCFServerUdp wcfServer = new TunnelWCFServerUdp();
            ClientEnd clientEnd = null;
            UdpClient actualServer = null;

            try
            {
                // Arrange
                int wcfServerPort = NetworkInterface.GrabEphemeralPort();
                int actualServerPort = 9877;
                int clientEndPort = 9866;
                clientEnd = new ClientEnd();
                using (actualServer = new UdpClient())
                {
                    actualServer.Connect(IPAddress.Parse("127.0.0.1"), actualServerPort);
                    using (UdpClient actualClient = new UdpClient(clientEndPort))
                    {
                        // Act
                        wcfServer.StartServer(wcfServerPort, actualServerPort, true);
                        clientEnd.Start(new Uri("http://127.0.0.1:" + wcfServerPort.ToString()), clientEndPort, "127.0.0.1", TunnelProtocolType.UdpIncoming);
                        
                        byte[] sentBytes = new byte[] { 0, 1, 2, 3, 4 };
                        actualServer.Send(sentBytes, 5);
                        byte[] receivedBytes = new byte[5];

                        IPEndPoint remoteSender = null;
                        receivedBytes = actualClient.Receive(ref remoteSender);

                        // Assert
                        Assert.AreEqual(sentBytes.Length, receivedBytes.Length);
                        for (int i = 0; i < sentBytes.Length; i++)
                        {
                            Assert.AreEqual(sentBytes[i], receivedBytes[i]);
                        }
                    }
                }
            }
            finally
            {
                wcfServer.Stop();
                if (clientEnd != null)
                {
                    clientEnd.Stop();
                }
                if (actualServer != null)
                {
                    actualServer.Close();
                }
            }
        }
    }
}
