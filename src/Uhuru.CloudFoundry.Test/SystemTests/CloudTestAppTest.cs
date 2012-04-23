using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System;
using System.Threading;
using System.Text;
using Uhuru.CloudFoundry.Adaptor;
using System.Security;
using Uhuru.CloudFoundry.Adaptor.Objects;
using Uhuru.CloudFoundry.Objects.Packaging;

namespace Uhuru.CloudFoundry.Test.SystemTests
{
    [TestClass]
    public class CloudTestAppTest
    {
        static string target;
        static string username;
        static string password;
        static string cloudTestAppDir;
        static string currentTestFramework = "iis";
        static List<string> directoriesCreated;
        static CloudConnection cloudConnection = null;

        private readonly object lck = new object();

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            directoriesCreated = new List<string>();
            target = ConfigurationManager.AppSettings["target"];
            cloudTestAppDir = Path.GetFullPath(@"..\..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\CloudTestApp\app");
            username = TestUtil.GenerateAppName() + "@uhurucloud.net";
            password = TestUtil.GenerateAppName();

            cloudConnection = TestUtil.CreateAndImplersonateUser(username, password);
           
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {

            foreach (App app in cloudConnection.Apps)
            {
                app.Delete();
            }

            foreach (ProvisionedService service in cloudConnection.ProvisionedServices)
            {
                service.Delete();
            }

            TestUtil.DeleteUser(username, directoriesCreated);
        }

        //[TestInitialize]
        //public void TestInitialize()
        //{
        //    client = new Client();
        //    client.Target(target);
        //    client.Login(username, password);
        //}

        [TestCleanup]
        public void TestCleanup()
        {
            foreach (App app in cloudConnection.Apps)
            {
                app.Delete();
            }
            Thread.Sleep(3000);
        }

        [TestMethod, TestCategory("System")]
        public void TC001_CloudTestAppCreate()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);

            // Act
            TestUtil.PushApp(name, cloudTestAppDir, url, directoriesCreated, cloudConnection);

            // Assert
            Assert.IsTrue(TestUtil.TestUrl(url));
            bool exists=false;
            foreach (App app in cloudConnection.Apps)
            {
                if (app.Name == name)
                {
                    exists =true;
                    break;
                }
            }
            Assert.IsTrue(exists);
        }

        [TestMethod, TestCategory("System")]
        public void TC002_CloudTestAppDelete()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);
            
            TestUtil.PushApp(name, cloudTestAppDir, url, directoriesCreated, cloudConnection);
            
            // Act
            foreach (App app in cloudConnection.Apps)
            {
                if (app.Name == name)
                {
                    app.Delete();
                    Thread.Sleep(10000);
                }
            }

            // Assert
            bool exists = false;
            foreach (App app in cloudConnection.Apps)
            {
                if (app.Name == name)
                {
                    exists = true;
                    break;
                }
            }
            Assert.IsFalse(TestUtil.TestUrl(url));
            Assert.IsFalse(exists);
        }

        [TestMethod, TestCategory("System")]
        public void TC003_CloudTestAppCreate5Sequential()
        {
            // Arrange
            Dictionary<string, string> apps = new Dictionary<string, string>();
            for (int i = 0; i < 5; i++)
            {
                string name = TestUtil.GenerateAppName();
                string url = "http://" + target.Replace("api", name);
                apps.Add(name, url);
            }

            // Act
            foreach (KeyValuePair<string, string> pair in apps)
            {
                TestUtil.PushApp(pair.Key, cloudTestAppDir, pair.Value, directoriesCreated, cloudConnection);
                Thread.Sleep(1000);
            }

            // Assert
            foreach (KeyValuePair<string, string> pair in apps)
            {
                bool exists = false;
                foreach (App app in cloudConnection.Apps)
                {
                    if (app.Name == pair.Key)
                    {
                        exists = true;
                        break;
                    }
                }
                Assert.IsTrue(exists);
                Assert.IsTrue(TestUtil.TestUrl(pair.Value));

            }
        }

        [TestMethod, TestCategory("System")]
        public void TC004_CloudTestAppDelete5Sequential()
        {
            // Arrange
            Dictionary<string, string> apps = new Dictionary<string, string>();
            for (int i = 0; i < 5; i++)
            {
                string name = TestUtil.GenerateAppName();
                string url = "http://" + target.Replace("api", name);
                apps.Add(name, url);
                TestUtil.PushApp(name, cloudTestAppDir, url, directoriesCreated, cloudConnection);
                //PushApp(name, cloudTestAppDir, url);
            }

            // Act
            foreach (string app in apps.Keys)
            {
                foreach (App appObj in cloudConnection.Apps)
                {
                    if (appObj.Name == app)
                    {
                        appObj.Delete();
                        Thread.Sleep(1000);
                    }
                }
                
            }

            Thread.Sleep(2000);
            
            // Assert
            foreach (KeyValuePair<string, string> pair in apps)
            {
                bool exists = false;
                foreach (App app in cloudConnection.Apps)
                {
                    if (app.Name == pair.Key)
                    {
                        exists = true;
                        break;
                    }
                }
                Assert.IsFalse(exists);
                Assert.IsFalse(TestUtil.TestUrl(pair.Value));
            }
        }

        [TestMethod, TestCategory("System")]
        public void TC005_CloudTestAppCreate5Parallel()
        {
            // Arrange
            Dictionary<string, string> apps = new Dictionary<string, string>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();
            for (int i = 0; i < 5; i++)
            {
                string name = TestUtil.GenerateAppName();
                string url = "http://" + target.Replace("api", name);
                apps.Add(name, url);
                Thread t = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        TestUtil.PushApp(name, cloudTestAppDir, url, directoriesCreated, cloudConnection);
                    }
                    catch (Exception ex)
                    {
                        lock (lck)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
                threads.Add(t);
            }

            // Act
            foreach (Thread t in threads)
            {
                t.Start();
            }
            foreach (Thread t in threads)
            {
                t.Join();
            }

            // Assert
            if (exceptions.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception ex in exceptions)
                {
                    sb.AppendLine(ex.ToString());
                }
                Assert.Fail("At least one exception has been  thrown:" + sb.ToString());
            }
            foreach (KeyValuePair<string, string> pair in apps)
            {
                bool exists = false;
                foreach (App app in cloudConnection.Apps)
                {
                    if (app.Name == pair.Key)
                    {
                        exists = true;
                        break;
                    }
                }
                Assert.IsTrue(exists);
                Assert.IsTrue(TestUtil.TestUrl(pair.Value));
            }
        }

        [TestMethod, TestCategory("System")]
        public void TC006_CloudTestAppDelete5Parallel()
        {
            // Arrange
            Dictionary<string, string> apps = new Dictionary<string, string>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();
            for (int i = 0; i < 5; i++)
            {
                string name = TestUtil.GenerateAppName();
                string url = "http://" + target.Replace("api", name);
                apps.Add(name, url);
                TestUtil.PushApp(name, cloudTestAppDir, url,directoriesCreated,cloudConnection);
            }
            foreach (string str in apps.Keys)
            {
                string name = str;
                Thread t = new Thread(new ThreadStart(delegate
                {
                    try
                    {

                        foreach (App appObj in cloudConnection.Apps)
                        {
                            if (appObj.Name == name)
                            {
                                appObj.Delete();
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (lck)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
                threads.Add(t);
            }

            // Act
            foreach (Thread t in threads)
            {
                t.Start();
            }
            foreach (Thread t in threads)
            {
                t.Join();
            }

            // Assert
            if (exceptions.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception ex in exceptions)
                {
                    sb.AppendLine(ex.ToString());
                }
                Assert.Fail("At least one exception has been  thrown:" + sb.ToString());
            }
            Thread.Sleep(20000);
            foreach (KeyValuePair<string, string> pair in apps)
            {
                bool exists = false;
                foreach (App app in cloudConnection.Apps)
                {
                    if (app.Name == pair.Key)
                    {
                        exists = true;
                        break;
                    }
                }
                Assert.IsFalse(exists);
                Assert.IsFalse(TestUtil.TestUrl(pair.Value));
            }
        }

        //private void PushApp(string appName, string sourceDir, string url)
        //{
        //    string path = TestUtil.CopyFolderToTemp(sourceDir);
        //    lock (lck)
        //    {
        //        directoriesCreated.Add(path);
        //    }
        //    CloudApplication cloudApplication = new CloudApplication();
        //    cloudApplication.Name = appName;
        //    cloudApplication.Urls = new string[] { url };
        //    cloudApplication.Framework = "iis";
        //    cloudApplication.Runtime = "dotNet";
        //    cloudApplication.InstanceCount = 1;
        //    cloudApplication.Memory = 128;
        //    cloudApplication.Deployable = true;
        //    PushTracker pushTracker = new PushTracker();
        //    pushTracker.TrackId = Guid.NewGuid();
        //    cloudConnection.PushJob.Start(pushTracker, cloudApplication);
        //    //client.Push(appName, url, path, 1, "dotNet", "iis", 128, new List<string>(), false, true);
        //}
    }
}
