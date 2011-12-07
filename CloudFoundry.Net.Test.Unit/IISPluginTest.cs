using Uhuru.CloudFoundry.DEA.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Uhuru.CloudFoundry.Server.DEA.PluginBase;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using Uhuru.Utilities;

namespace CloudFoundry.Net.Test.Unit
{
    
    
    /// <summary>
    ///This is a test class for IISPluginTest and is intended
    ///to contain all IISPluginTest Unit Tests
    ///</summary>
    [TestClass()]
    [DeploymentItem("log4net.config")]
    public class IISPluginTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for ConfigureApplication
        ///</summary>
        [TestMethod()]
        public void ConfigureApplicationTest()
        {
            IISPlugin target = new IISPlugin(); 
            ApplicationInfo appInfo = new ApplicationInfo();
            appInfo.InstanceId = Guid.NewGuid().ToString();
            appInfo.LocalIp = "192.168.1.4";
            appInfo.Name = "MyTestApp";
            appInfo.Path = @"F:\Code\vcap-dotnet\TestApps\CloudTestApp";
            appInfo.Port = Uhuru.Utilities.NetworkInterface.GrabEphemeralPort();
            appInfo.WindowsUsername = "cfuser";
            appInfo.WindowsPassword = "Password1234!";

            Runtime runtime = new Runtime();
            runtime.Name = "iis";

            ApplicationVariable[] variables = null;
            ApplicationService[] services = null;

            string logFilePath = @"F:\Code\vcap-dotnet\TestApps\cloudtestapp.log";

            target.ConfigureApplication(appInfo, runtime, variables, services, logFilePath);
        }

        [TestMethod()]
        public void MultipleApps()
        {
            List<ApplicationInfo> appInfos = new List<ApplicationInfo>();
            List<IISPlugin> plugins = new List<IISPlugin>();
            List<Thread> threadsStart = new List<Thread>();
            List<Thread> threadsStop = new List<Thread>();

            for (int i = 0; i < 20; i++)
            {
                ApplicationInfo appInfo = new ApplicationInfo();
                appInfo.InstanceId = Guid.NewGuid().ToString();
                appInfo.LocalIp = "192.168.1.4";
                appInfo.Name = "MyTestApp";
                appInfo.Path = Helper.CopyFolderToTemp(@"F:\Code\vcap-dotnet\TestApps\CloudTestApp");
                appInfo.Port = Uhuru.Utilities.NetworkInterface.GrabEphemeralPort();
                appInfo.WindowsUsername = "cfuser";
                appInfo.WindowsPassword = "Password1234!";
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
                            Runtime runtime = new Runtime();
                            runtime.Name = "iis";
                            ApplicationVariable[] variables = null;
                            ApplicationService[] services = null;
                            string logFilePath = @"F:\Code\vcap-dotnet\TestApps\cloudtestapp.log";

                            target.ConfigureApplication(appInfos[(int)data], runtime, variables, services, logFilePath);
                            target.StartApplication();

                            Thread.Sleep(5000);
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


            foreach (ApplicationInfo appInfo in appInfos)
            {
                WebClient client = new WebClient();
                string html = client.DownloadString("http://localhost:" + appInfo.Port.ToString());
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
                        Thread.Sleep(5000);
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

            foreach (ApplicationInfo appInfo in appInfos)
            {
                try
                {
                    WebClient client = new WebClient();
                    string html = client.DownloadString("http://localhost:" + appInfo.Port.ToString());
                    Assert.Fail();
                }
                catch
                {
                }
            }
        }
    

        /// <summary>
        ///A test for StartApplication
        ///</summary>
        [TestMethod()]
        public void StartApplicationTest()
        {
            string password = "~!@213abc";

            try
            {
                // paranoid cleanup
                Uhuru.Utilities.WindowsVcapUsers.DeleteUser("testuser");
            }
            catch { }

            string username = Uhuru.Utilities.WindowsVcapUsers.CreateUser("testuser", password);

            try
            {
                IISPlugin target = new IISPlugin();
                ApplicationInfo appInfo = new ApplicationInfo();
                appInfo.InstanceId = Guid.NewGuid().ToString();
                appInfo.LocalIp = "192.168.1.4";
                appInfo.Name = "MyTestApp";
                appInfo.Path = @"F:\Code\vcap-dotnet\TestApps\CloudTestApp";
                appInfo.Port = Uhuru.Utilities.NetworkInterface.GrabEphemeralPort();
                appInfo.WindowsUsername = username;
                appInfo.WindowsPassword = password;

                Runtime runtime = new Runtime();
                runtime.Name = "iis";

                ApplicationVariable[] variables = null;
                ApplicationService[] services = null;

                string logFilePath = @"F:\Code\vcap-dotnet\TestApps\cloudtestapp.log";

                target.ConfigureApplication(appInfo, runtime, variables, services, logFilePath);
                target.StartApplication();

                Thread.Sleep(5000);

                WebClient client = new WebClient();
                string html = client.DownloadString("http://localhost:" + appInfo.Port.ToString());
                Assert.IsTrue(html.Contains("Welcome to ASP.NET!"));

                target.StopApplication();

                try
                {
                    html = client.DownloadString("http://localhost:" + appInfo.Port.ToString());
                }
                catch
                {
                    return;
                }
                Assert.Fail();
            }
            finally
            {
                Uhuru.Utilities.WindowsVcapUsers.DeleteUser("testuser");
            }
        }

      
    }
}
