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

namespace Uhuru.CloudFoundry.Test.Integration
{
    [TestClass]
    [DeploymentItem("log4net.config")]
    public class HttpTunnelWcfServiceTest
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
        [TestCategory("Integration")]
        [Timeout(100000)]
        public void TC001_TCPTunnelSimpleSend()
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

                // Act

                using (TcpClient actualClient = new TcpClient())
                {
                    clientEnd.Start(new Uri("http://127.0.0.1:" + appPort + "/HttpTunnel.svc"), clientPort, "127.0.0.1", TunnelProtocolType.Tcp);
                    actualClient.Connect("127.0.0.1", clientPort);

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
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
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
        [TestCategory("Integration")]
        [Timeout(100000)]
        public void TC002_TCPTunnelMessageReceive()
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

                // Act

                using (TcpClient actualClient = new TcpClient())
                {
                    clientEnd.Start(new Uri("http://127.0.0.1:" + appPort + "/HttpTunnel.svc"), clientPort, "127.0.0.1", TunnelProtocolType.Tcp);
                    actualClient.Connect("127.0.0.1", clientPort);

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
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
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
        [TestCategory("Integration")]
        [Timeout(100000)]
        public void TC003_TCPTunnelMessageSendReceive()
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

                // Act

                using (TcpClient actualClient = new TcpClient())
                {
                    clientEnd.Start(new Uri("http://127.0.0.1:" + appPort + "/HttpTunnel.svc"), clientPort, "127.0.0.1", TunnelProtocolType.Tcp);
                    actualClient.Connect("127.0.0.1", clientPort);

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
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
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
        [TestCategory("Integration")]
        [Timeout(100000)]
        public void TC004_TCPTunnelMultipleSend()
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

                // Act

                using (TcpClient actualClient = new TcpClient())
                {
                    clientEnd.Start(new Uri("http://127.0.0.1:" + appPort + "/HttpTunnel.svc"), clientPort, "127.0.0.1", TunnelProtocolType.Tcp);
                    actualClient.Connect("127.0.0.1", clientPort);

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
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
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
        [TestCategory("Integration")]
        [Timeout(100000)]
        public void TC005_UDPTunnelSimpleSend()
        {
            // Arrange
            TunnelPackage.Create(tunnelServiceSourceDir, Path.Combine(tunnelServiceDir, "App"), actualServerPort.ToString(), "127.0.0.1", "UdpOutgoing");


            UdpClient actualServer = null;
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
                actualServer = new UdpClient(actualServerPort);

                target.StartApplication();

                clientEnd = new ClientEnd();

                // Act

                using (UdpClient actualClient = new UdpClient())
                {

                    // Act
                    clientEnd.Start(new Uri("http://127.0.0.1:" + appPort + "/HttpTunnel.svc"), clientPort, "127.0.0.1", TunnelProtocolType.UdpOutgoing);
                    actualClient.Connect(IPAddress.Parse("127.0.0.1"), clientPort);

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
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            finally
            {
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
        [TestCategory("Integration")]
        [Timeout(100000)]
        public void TC006_UDPTunnelSimpleReceive()
        {
            // Arrange
            TunnelPackage.Create(tunnelServiceSourceDir, Path.Combine(tunnelServiceDir, "App"), actualServerPort.ToString(), "127.0.0.1", "UdpIncoming");

            UdpClient actualServer = null;
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
                
                clientEnd = new ClientEnd();

                // Act

                using (actualServer = new UdpClient())
                {
                    actualServer.Connect(IPAddress.Parse("127.0.0.1"), actualServerPort);
                   
                    using (UdpClient actualClient = new UdpClient(clientPort))
                    {
                        // Act
                        target.StartApplication();
                        clientEnd.Start(new Uri("http://127.0.0.1:" + appPort + "/HttpTunnel.svc"), clientPort, "127.0.0.1", TunnelProtocolType.UdpIncoming);

                        TestUtil.TestUrl("http://127.0.0.1:" + appPort + "/HttpTunnel.svc");

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
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            finally
            {
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
