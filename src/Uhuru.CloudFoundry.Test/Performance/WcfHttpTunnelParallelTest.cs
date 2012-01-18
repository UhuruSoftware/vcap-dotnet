using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.DEA.PluginBase;
using Uhuru.CloudFoundry.DEA.Plugins;
using Uhuru.Utilities;
using Uhuru.Utilities.HttpTunnel;

namespace Uhuru.CloudFoundry.Test.Performance
{
    [TestClass]
    [DeploymentItem("log4net.config")]
    public class WcfHttpTunnelParallelTest
    {
        string tunnelServiceSourceDir = @"..\..\..\..\src\Uhuru.Utilities.TunnelService";
        string tunnelServiceDir;
        static string user;
        static string password;

        int actualServerPort;
        int appPort;
        int clientPort;

        IISPlugin target;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            password = "!@#33Pass";
            user = Utilities.WindowsVCAPUsers.CreateUser("WcfHttpTunnel", password);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Utilities.WindowsVCAPUsers.DeleteUser("WcfHttpTunnel");
        }

        [TestInitialize]
        public void TestInitialize()
        {
            string tempFolder = Path.GetTempPath();
            tunnelServiceDir = tempFolder + Guid.NewGuid().ToString();
            Directory.CreateDirectory(tunnelServiceDir);

            actualServerPort = NetworkInterface.GrabEphemeralPort();
            appPort = NetworkInterface.GrabEphemeralPort();
            clientPort = NetworkInterface.GrabEphemeralPort();

            target = new IISPlugin();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            target.StopApplication();
        }

        [TestMethod()]
        [TestCategory("Performance")]
        [Timeout(100000)]
        public void TC001_TCPTunnelParallelClientsSend()
        {
            // Arrange
            TunnelPackage.Create(tunnelServiceSourceDir, Path.Combine(tunnelServiceDir, "App"), actualServerPort.ToString(), "127.0.0.1", "Tcp");


            TcpListener actualServer = null;
            ClientEnd clientEnd = null;

            ApplicationVariable[] appVariables = new ApplicationVariable[] {
              new ApplicationVariable() { Name = "VCAP_PLUGIN_STAGING_INFO", Value=@"{""assembly"":""Uhuru.CloudFoundry.DEA.Plugins.dll"",""class_name"":""Uhuru.CloudFoundry.DEA.Plugins.IISPlugin"",""logs"":{""app_error"":""logs/stderr.log"",""dea_error"":""logs/err.log"",""startup"":""logs/startup.log"",""app"":""logs/stdout.log""},""auto_wire_templates"":{""mssql-2008"":""Data Source={host},{port};Initial Catalog={name};User Id={user};Password={password};MultipleActiveResultSets=true"",""mysql-5.1"":""server={host};port={port};Database={name};Uid={user};Pwd={password};""}}" },
              new ApplicationVariable() { Name = "VCAP_APPLICATION", Value=@"{""instance_id"":""" + Guid.NewGuid().ToString() + @""",""instance_index"":0,""name"":""MyTestApp"",""uris"":[""sinatra_env_test_app.uhurucloud.net""],""users"":[""dev@cloudfoundry.org""],""version"":""c394f661a907710b8a8bb70b84ff0c83354dbbed-1"",""start"":""2011-12-07 14:40:12 +0200"",""runtime"":""iis"",""state_timestamp"":1323261612,""port"":51202,""limits"":{""fds"":256,""mem"":67108864,""disk"":2147483648},""host"":""192.168.1.117""}" },
              new ApplicationVariable() { Name = "VCAP_SERVICES", Value=@"{""mssql-2008"":[{""name"":""mssql-b24a2"",""label"":""mssql-2008"",""plan"":""free"",""tags"":[""mssql"",""2008"",""relational""],""credentials"":{""name"":""D4Tac4c307851cfe495bb829235cd384f094"",""username"":""US3RTfqu78UpPM5X"",""user"":""US3RTfqu78UpPM5X"",""password"":""P4SSdCGxh2gYjw54"",""hostname"":""192.168.1.3"",""port"":1433,""bind_opts"":{}}}]}" },
              new ApplicationVariable() { Name = "VCAP_APP_HOST", Value=TestUtil.GetLocalIp() },
              new ApplicationVariable() { Name = "VCAP_APP_PORT", Value = appPort.ToString() },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER_PASSWORD", Value = password },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER", Value = user },
              new ApplicationVariable() { Name = "HOME",  Value = tunnelServiceDir}
            };

            target.ConfigureApplication(appVariables);

            try
            {
                actualServer = new TcpListener(IPAddress.Parse("127.0.0.1"), actualServerPort);
                actualServer.Start();

                target.StartApplication();

                clientEnd = new ClientEnd();
                clientEnd.Start(new Uri("http://127.0.0.1:" + appPort + "/HttpTunnel.svc"), clientPort, "127.0.0.1", TunnelProtocolType.Tcp);

                // Act
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
                                actualClient.Connect(IPAddress.Parse("127.0.0.1"), clientPort);

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
        [TestCategory("Performance")]
        [Timeout(100000)]
        public void TC002_TCPTunnelParallelClientsSendReceive()
        {
            // Arrange
            TunnelPackage.Create(tunnelServiceSourceDir, Path.Combine(tunnelServiceDir, "App"), actualServerPort.ToString(), "127.0.0.1", "Tcp");


            TcpListener actualServer = null;
            ClientEnd clientEnd = null;

            ApplicationVariable[] appVariables = new ApplicationVariable[] {
              new ApplicationVariable() { Name = "VCAP_PLUGIN_STAGING_INFO", Value=@"{""assembly"":""Uhuru.CloudFoundry.DEA.Plugins.dll"",""class_name"":""Uhuru.CloudFoundry.DEA.Plugins.IISPlugin"",""logs"":{""app_error"":""logs/stderr.log"",""dea_error"":""logs/err.log"",""startup"":""logs/startup.log"",""app"":""logs/stdout.log""},""auto_wire_templates"":{""mssql-2008"":""Data Source={host},{port};Initial Catalog={name};User Id={user};Password={password};MultipleActiveResultSets=true"",""mysql-5.1"":""server={host};port={port};Database={name};Uid={user};Pwd={password};""}}" },
              new ApplicationVariable() { Name = "VCAP_APPLICATION", Value=@"{""instance_id"":""" + Guid.NewGuid().ToString() + @""",""instance_index"":0,""name"":""MyTestApp"",""uris"":[""sinatra_env_test_app.uhurucloud.net""],""users"":[""dev@cloudfoundry.org""],""version"":""c394f661a907710b8a8bb70b84ff0c83354dbbed-1"",""start"":""2011-12-07 14:40:12 +0200"",""runtime"":""iis"",""state_timestamp"":1323261612,""port"":51202,""limits"":{""fds"":256,""mem"":67108864,""disk"":2147483648},""host"":""192.168.1.117""}" },
              new ApplicationVariable() { Name = "VCAP_SERVICES", Value=@"{""mssql-2008"":[{""name"":""mssql-b24a2"",""label"":""mssql-2008"",""plan"":""free"",""tags"":[""mssql"",""2008"",""relational""],""credentials"":{""name"":""D4Tac4c307851cfe495bb829235cd384f094"",""username"":""US3RTfqu78UpPM5X"",""user"":""US3RTfqu78UpPM5X"",""password"":""P4SSdCGxh2gYjw54"",""hostname"":""192.168.1.3"",""port"":1433,""bind_opts"":{}}}]}" },
              new ApplicationVariable() { Name = "VCAP_APP_HOST", Value=TestUtil.GetLocalIp() },
              new ApplicationVariable() { Name = "VCAP_APP_PORT", Value = appPort.ToString() },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER_PASSWORD", Value = password },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER", Value = user },
              new ApplicationVariable() { Name = "HOME",  Value = tunnelServiceDir}
            };

            target.ConfigureApplication(appVariables);

            try
            {
                actualServer = new TcpListener(IPAddress.Parse("127.0.0.1"), actualServerPort);
                actualServer.Start();

                target.StartApplication();

                clientEnd = new ClientEnd();
                clientEnd.Start(new Uri("http://127.0.0.1:" + appPort + "/HttpTunnel.svc"), clientPort, "127.0.0.1", TunnelProtocolType.Tcp);

                byte[] sentBytes = new byte[] { 0, 1, 2, 3, 4 };
                List<byte[]> receivedBytesFrom = new List<byte[]>();
                List<int> receivedBytesCountFrom = new List<int>();

                List<byte[]> receivedBytesTo = new List<byte[]>();
                List<int> receivedBytesCountTo = new List<int>();


                List<Thread> threads = new List<Thread>();
                int writeCount = 50;

                // Act

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
                                actualClient.Connect(IPAddress.Parse("127.0.0.1"), clientPort);

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
    }
}
