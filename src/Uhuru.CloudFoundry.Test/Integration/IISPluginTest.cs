using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.DEA.Plugins;
using Uhuru.CloudFoundry.DEA.PluginBase;
using System.Threading;
using Uhuru.Utilities;
using System.Net;
using System.IO;

namespace Uhuru.CloudFoundry.Test.Integration
{
    [TestClass]
    [DeploymentItem("log4net.config")]
    public class IISPluginTest
    {
        string user;
        string password;
        string testAppLoc = Path.GetFullPath(@"..\..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\CloudTestApp");

        [TestInitialize()]
        public void TestInitialize()
        {
            try
            {
                Utilities.WindowsVCAPUsers.DeleteUser("IISPluginTest");
            }
            catch { }

            password = "!@#33Pass";
            user = Utilities.WindowsVCAPUsers.CreateUser("IISPluginTest", password);
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            try
            {
                Utilities.WindowsVCAPUsers.DeleteUser(user);
            }
            catch { }
        }

        /// <summary>cd
        ///A test for ConfigureApplication
        ///</summary>
        [TestMethod()]
        [TestCategory("Integration")]
        public void TC001_ConfigureApplicationTest()
        {
            //Arrange
            IISPlugin target = new IISPlugin();
            ApplicationVariable[] appVariables = new ApplicationVariable[] {
              new ApplicationVariable() { Name = "VCAP_PLUGIN_STAGING_INFO", Value=@"{""assembly"":""Uhuru.CloudFoundry.DEA.Plugins.dll"",""class_name"":""Uhuru.CloudFoundry.DEA.Plugins.IISPlugin"",""logs"":{""app_error"":""logs/stderr.log"",""dea_error"":""logs/err.log"",""startup"":""logs/startup.log"",""app"":""logs/stdout.log""},""auto_wire_templates"":{""mssql-2008"":""Data Source={host},{port};Initial Catalog={name};User Id={user};Password={password};MultipleActiveResultSets=true"",""mysql-5.1"":""server={host};port={port};Database={name};Uid={user};Pwd={password};""}}" },
              new ApplicationVariable() { Name = "VCAP_APPLICATION", Value=@"{""instance_id"":""646c477f54386d8afb279ec2f990a823"",""instance_index"":0,""name"":""sinatra_env_test_app"",""uris"":[""sinatra_env_test_app.uhurucloud.net""],""users"":[""dev@cloudfoundry.org""],""version"":""c394f661a907710b8a8bb70b84ff0c83354dbbed-1"",""start"":""2011-12-07 14:40:12 +0200"",""runtime"":""iis"",""state_timestamp"":1323261612,""port"":51202,""limits"":{""fds"":256,""mem"":67108864,""disk"":2147483648},""host"":""192.168.1.117""}" },
              new ApplicationVariable() { Name = "VCAP_SERVICES", Value=@"{""mssql-2008"":[{""name"":""mssql-b24a2"",""label"":""mssql-2008"",""plan"":""free"",""tags"":[""mssql"",""2008"",""relational""],""credentials"":{""name"":""D4Tac4c307851cfe495bb829235cd384f094"",""username"":""US3RTfqu78UpPM5X"",""user"":""US3RTfqu78UpPM5X"",""password"":""P4SSdCGxh2gYjw54"",""hostname"":""192.168.1.3"",""port"":1433,""bind_opts"":{}}}]}" },
              new ApplicationVariable() { Name = "VCAP_APP_HOST", Value=@"192.168.1.118" },
              new ApplicationVariable() { Name = "VCAP_APP_PORT", Value=@"65498" },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER_PASSWORD", Value = password },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER", Value = user },
              new ApplicationVariable() { Name = "HOME", Value=TestUtil.CopyFolderToTemp(testAppLoc) }
            };

            Exception exception = null;

            //Act
            try
            {
                target.ConfigureApplication(appVariables);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            //Assert
            Assert.IsNull(exception, "Exception thrown");
        }




        /// <summary>
        /// A test for Start WebApp
        /// </summary>
        [TestMethod()]
        [TestCategory("Integration")]
        public void TC002_StartWebAppTest()
        {

            //Arrange
            IISPlugin target = new IISPlugin();

            int port = Uhuru.Utilities.NetworkInterface.GrabEphemeralPort();

            ApplicationVariable[] appVariables = new ApplicationVariable[] {
              new ApplicationVariable() { Name = "VCAP_PLUGIN_STAGING_INFO", Value=@"{""assembly"":""Uhuru.CloudFoundry.DEA.Plugins.dll"",""class_name"":""Uhuru.CloudFoundry.DEA.Plugins.IISPlugin"",""logs"":{""app_error"":""logs/stderr.log"",""dea_error"":""logs/err.log"",""startup"":""logs/startup.log"",""app"":""logs/stdout.log""},""auto_wire_templates"":{""mssql-2008"":""Data Source={host},{port};Initial Catalog={name};User Id={user};Password={password};MultipleActiveResultSets=true"",""mysql-5.1"":""server={host};port={port};Database={name};Uid={user};Pwd={password};""}}" },
              new ApplicationVariable() { Name = "VCAP_APPLICATION", Value=@"{""instance_id"":""" + Guid.NewGuid().ToString() + @""",""instance_index"":0,""name"":""MyTestApp"",""uris"":[""sinatra_env_test_app.uhurucloud.net""],""users"":[""dev@cloudfoundry.org""],""version"":""c394f661a907710b8a8bb70b84ff0c83354dbbed-1"",""start"":""2011-12-07 14:40:12 +0200"",""runtime"":""iis"",""state_timestamp"":1323261612,""port"":51202,""limits"":{""fds"":256,""mem"":67108864,""disk"":2147483648},""host"":""192.168.1.117""}" },
              new ApplicationVariable() { Name = "VCAP_SERVICES", Value=@"{""mssql-2008"":[{""name"":""mssql-b24a2"",""label"":""mssql-2008"",""plan"":""free"",""tags"":[""mssql"",""2008"",""relational""],""credentials"":{""name"":""D4Tac4c307851cfe495bb829235cd384f094"",""username"":""US3RTfqu78UpPM5X"",""user"":""US3RTfqu78UpPM5X"",""password"":""P4SSdCGxh2gYjw54"",""hostname"":""192.168.1.3"",""port"":1433,""bind_opts"":{}}}]}" },
              new ApplicationVariable() { Name = "VCAP_APP_HOST", Value=TestUtil.GetLocalIp() },
              new ApplicationVariable() { Name = "VCAP_APP_PORT", Value = port.ToString() },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER_PASSWORD", Value = password },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER", Value = user },
              new ApplicationVariable() { Name = "HOME", Value=TestUtil.CopyFolderToTemp(testAppLoc) }
            };


            //Act
            target.ConfigureApplication(appVariables);
            target.StartApplication();

            //Assert
            WebClient client = new WebClient();
            string html = client.DownloadString("http://localhost:" + port.ToString());
            Assert.IsTrue(html.Contains("My ASP.NET Application"));

            target.StopApplication();

            try
            {
                html = client.DownloadString("http://localhost:" + port.ToString());
            }
            catch
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod()]
        [TestCategory("Integration")]
        public void TC003_MultipleWebApps()
        {
            List<ApplicationVariable[]> appInfos = new List<ApplicationVariable[]>();
            List<IISPlugin> plugins = new List<IISPlugin>();
            List<Thread> threadsStart = new List<Thread>();
            List<Thread> threadsStop = new List<Thread>();

            for (int i = 0; i < 20; i++)
            {
                int port = Uhuru.Utilities.NetworkInterface.GrabEphemeralPort();
                ApplicationVariable[] appInfo = new ApplicationVariable[] {
              new ApplicationVariable() { Name = "VCAP_PLUGIN_STAGING_INFO", Value=@"{""assembly"":""Uhuru.CloudFoundry.DEA.Plugins.dll"",""class_name"":""Uhuru.CloudFoundry.DEA.Plugins.IISPlugin"",""logs"":{""app_error"":""logs/stderr.log"",""dea_error"":""logs/err.log"",""startup"":""logs/startup.log"",""app"":""logs/stdout.log""},""auto_wire_templates"":{""mssql-2008"":""Data Source={host},{port};Initial Catalog={name};User Id={user};Password={password};MultipleActiveResultSets=true"",""mysql-5.1"":""server={host};port={port};Database={name};Uid={user};Pwd={password};""}}" },
              new ApplicationVariable() { Name = "VCAP_APPLICATION", Value=@"{""instance_id"":""" + Guid.NewGuid().ToString() + @""",""instance_index"":0,""name"":""MyTestApp"",""uris"":[""sinatra_env_test_app.uhurucloud.net""],""users"":[""dev@cloudfoundry.org""],""version"":""c394f661a907710b8a8bb70b84ff0c83354dbbed-1"",""start"":""2011-12-07 14:40:12 +0200"",""runtime"":""iis"",""state_timestamp"":1323261612,""port"":51202,""limits"":{""fds"":256,""mem"":67108864,""disk"":2147483648},""host"":""192.168.1.117""}" },
              new ApplicationVariable() { Name = "VCAP_SERVICES", Value=@"{""mssql-2008"":[{""name"":""mssql-b24a2"",""label"":""mssql-2008"",""plan"":""free"",""tags"":[""mssql"",""2008"",""relational""],""credentials"":{""name"":""D4Tac4c307851cfe495bb829235cd384f094"",""username"":""US3RTfqu78UpPM5X"",""user"":""US3RTfqu78UpPM5X"",""password"":""P4SSdCGxh2gYjw54"",""hostname"":""192.168.1.3"",""port"":1433,""bind_opts"":{}}}]}" },
              new ApplicationVariable() { Name = "VCAP_APP_HOST", Value=TestUtil.GetLocalIp() },
              new ApplicationVariable() { Name = "VCAP_APP_PORT", Value = port.ToString() },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER_PASSWORD", Value = password },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER", Value = user },
              new ApplicationVariable() { Name = "HOME", Value=TestUtil.CopyFolderToTemp(testAppLoc) }
            };

                appInfos.Add(appInfo);
                plugins.Add(new IISPlugin());
            }


            for (int i = 0; i < 20; i++)
            {
                threadsStart.Add(new Thread(new ParameterizedThreadStart(delegate(object data)
                {
                    try
                    {
                        IISPlugin target = plugins[(int)data];

                        target.ConfigureApplication(appInfos[(int)data]);
                        target.StartApplication();
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(ex.ToString());
                    }
                })));
            }

            for (int i = 0; i < threadsStart.Count; i++)
            {
                Thread thread = threadsStart[i];
                thread.Start(i);
            }

            foreach (Thread thread in threadsStart)
            {
                thread.Join();
            }


            foreach (ApplicationVariable[] appInfo in appInfos)
            {
                WebClient client = new WebClient();
                string html = client.DownloadString("http://localhost:" + appInfo.First(v => v.Name == "VCAP_APP_PORT").Value);
                Assert.IsTrue(html.Contains("My ASP.NET Application"));
            }


            for (int i = 0; i < 20; i++)
            {
                threadsStop.Add(new Thread(new ParameterizedThreadStart(delegate(object data)
                {
                    try
                    {
                        IISPlugin target = plugins[(int)data];
                        target.StopApplication();
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(ex.ToString());
                    }
                })));
            }


            for (int i = 0; i < threadsStop.Count; i++)
            {
                Thread thread = threadsStop[i];
                thread.Start(i);
            }

            foreach (Thread thread in threadsStop)
            {
                thread.Join();
            }

            foreach (ApplicationVariable[] appInfo in appInfos)
            {
                try
                {
                    WebClient client = new WebClient();
                    string html = client.DownloadString("http://localhost:" + appInfo.First(v => v.Name == "VCAP_APP_PORT").Value);
                    Assert.Fail();
                }
                catch
                {
                }
            }
        }


        [TestMethod()]
        [TestCategory("Integration")]
        public void TC004_ExceptionWebAppTest()
        {
            //Arrange
            IISPlugin target = new IISPlugin();


            string path = TestUtil.CopyFolderToTemp(testAppLoc);

            string logPath = Path.Combine(path, "logs");

            if (Directory.Exists(logPath))
            {
                Directory.Delete(logPath, true);
            }


            int port = Uhuru.Utilities.NetworkInterface.GrabEphemeralPort();

            ApplicationVariable[] appVariables = new ApplicationVariable[] {
              new ApplicationVariable() { Name = "VCAP_PLUGIN_STAGING_INFO", Value=@"{""assembly"":""Uhuru.CloudFoundry.DEA.Plugins.dll"",""class_name"":""Uhuru.CloudFoundry.DEA.Plugins.IISPlugin"",""logs"":{""app_error"":""logs/stderr.log"",""dea_error"":""logs/err.log"",""startup"":""logs/startup.log"",""app"":""logs/stdout.log""},""auto_wire_templates"":{""mssql-2008"":""Data Source={host},{port};Initial Catalog={name};User Id={user};Password={password};MultipleActiveResultSets=true"",""mysql-5.1"":""server={host};port={port};Database={name};Uid={user};Pwd={password};""}}" },
              new ApplicationVariable() { Name = "VCAP_APPLICATION", Value=@"{""instance_id"":""" + Guid.NewGuid().ToString() + @""",""instance_index"":0,""name"":""MyTestApp"",""uris"":[""sinatra_env_test_app.uhurucloud.net""],""users"":[""dev@cloudfoundry.org""],""version"":""c394f661a907710b8a8bb70b84ff0c83354dbbed-1"",""start"":""2011-12-07 14:40:12 +0200"",""runtime"":""iis"",""state_timestamp"":1323261612,""port"":51202,""limits"":{""fds"":256,""mem"":67108864,""disk"":2147483648},""host"":""192.168.1.117""}" },
              new ApplicationVariable() { Name = "VCAP_SERVICES", Value=@"{""mssql-2008"":[{""name"":""mssql-b24a2"",""label"":""mssql-2008"",""plan"":""free"",""tags"":[""mssql"",""2008"",""relational""],""credentials"":{""name"":""D4Tac4c307851cfe495bb829235cd384f094"",""username"":""US3RTfqu78UpPM5X"",""user"":""US3RTfqu78UpPM5X"",""password"":""P4SSdCGxh2gYjw54"",""hostname"":""192.168.1.3"",""port"":1433,""bind_opts"":{}}}]}" },
              new ApplicationVariable() { Name = "VCAP_APP_HOST", Value=TestUtil.GetLocalIp() },
              new ApplicationVariable() { Name = "VCAP_APP_PORT", Value = port.ToString() },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER_PASSWORD", Value = password },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER", Value = user },
              new ApplicationVariable() { Name = "HOME", Value=path }
            };


            //Act
            target.ConfigureApplication(appVariables);
            target.StartApplication();

            //Assert
            WebClient client = new WebClient();

            try
            {
                client.DownloadString("http://localhost:" + port.ToString() + "/exception.aspx");
            }
            catch (WebException wex)
            {
                using (StreamReader reader = new StreamReader(wex.Response.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();
                    Assert.IsTrue(html.Contains("Hello World Exception"));
                    string logContent = File.ReadAllText(Path.Combine(logPath, "stderr.log"));
                    Assert.IsTrue(logContent.Contains("Hello World Exception"));

                }
            }


            target.StopApplication();

            try
            {
                client.DownloadString("http://localhost:" + port.ToString());
            }
            catch
            {
                return;
            }
            Assert.Fail();
        }

        /// <summary>
        ///A test for Start WebApp
        ///</summary>
        [TestMethod()]
        [TestCategory("Integration")]
        public void TC005_TestGetProcessId()
        {
            //Arrange
            IISPlugin target = new IISPlugin();

            Assert.AreEqual(0, target.GetApplicationProcessId());

            int port = Uhuru.Utilities.NetworkInterface.GrabEphemeralPort();

            ApplicationVariable[] appVariables = new ApplicationVariable[] {
                    new ApplicationVariable() { Name = "VCAP_PLUGIN_STAGING_INFO", Value=@"{""assembly"":""Uhuru.CloudFoundry.DEA.Plugins.dll"",""class_name"":""Uhuru.CloudFoundry.DEA.Plugins.IISPlugin"",""logs"":{""app_error"":""logs/stderr.log"",""dea_error"":""logs/err.log"",""startup"":""logs/startup.log"",""app"":""logs/stdout.log""},""auto_wire_templates"":{""mssql-2008"":""Data Source={host},{port};Initial Catalog={name};User Id={user};Password={password};MultipleActiveResultSets=true"",""mysql-5.1"":""server={host};port={port};Database={name};Uid={user};Pwd={password};""}}" },
                    new ApplicationVariable() { Name = "VCAP_APPLICATION", Value=@"{""instance_id"":""" + Guid.NewGuid().ToString() + @""",""instance_index"":0,""name"":""MyTestApp"",""uris"":[""sinatra_env_test_app.uhurucloud.net""],""users"":[""dev@cloudfoundry.org""],""version"":""c394f661a907710b8a8bb70b84ff0c83354dbbed-1"",""start"":""2011-12-07 14:40:12 +0200"",""runtime"":""iis"",""state_timestamp"":1323261612,""port"":51202,""limits"":{""fds"":256,""mem"":67108864,""disk"":2147483648},""host"":""192.168.1.117""}" },
                    new ApplicationVariable() { Name = "VCAP_SERVICES", Value=@"{""mssql-2008"":[{""name"":""mssql-b24a2"",""label"":""mssql-2008"",""plan"":""free"",""tags"":[""mssql"",""2008"",""relational""],""credentials"":{""name"":""D4Tac4c307851cfe495bb829235cd384f094"",""username"":""US3RTfqu78UpPM5X"",""user"":""US3RTfqu78UpPM5X"",""password"":""P4SSdCGxh2gYjw54"",""hostname"":""192.168.1.3"",""port"":1433,""bind_opts"":{}}}]}" },
                    new ApplicationVariable() { Name = "VCAP_APP_HOST", Value=TestUtil.GetLocalIp() },
                    new ApplicationVariable() { Name = "VCAP_APP_PORT", Value = port.ToString() },
                    new ApplicationVariable() { Name = "VCAP_WINDOWS_USER_PASSWORD", Value = password },
                    new ApplicationVariable() { Name = "VCAP_WINDOWS_USER", Value = user },
                    new ApplicationVariable() { Name = "HOME", Value=TestUtil.CopyFolderToTemp(testAppLoc) }
                };


            //Act
            target.ConfigureApplication(appVariables);

            Assert.AreEqual(0, target.GetApplicationProcessId());

            target.StartApplication();

            Assert.AreEqual(0, target.GetApplicationProcessId());

            //Assert
            WebClient client = new WebClient();
            string html = client.DownloadString("http://localhost:" + port.ToString());
            Assert.IsTrue(html.Contains("My ASP.NET Application"));

            Assert.AreNotEqual(0, target.GetApplicationProcessId());

            target.StopApplication();

            Assert.AreEqual(0, target.GetApplicationProcessId());

            try
            {
                html = client.DownloadString("http://localhost:" + port.ToString());
            }
            catch
            {
                return;
            }
            Assert.AreEqual(0, target.GetApplicationProcessId());
        }
    }
}