using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.IO;
using System.Configuration;
using Uhuru.CloudFoundry.Cloud;

namespace Uhuru.CloudFoundry.Test.Performance
{
    [TestClass]
    public class CloudTestAppParallelTest
    {
        static string target;
        static string username;
        static string password;
        static string cloudTestAppDir;
        static List<string> directoriesCreated;

        Client client;
        private readonly object lck = new object();

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            directoriesCreated = new List<string>();
            target = ConfigurationManager.AppSettings["target"];
            cloudTestAppDir = Path.GetFullPath(@"..\..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\CloudTestApp\app");
            username = TestUtil.GenerateAppName() + "@uhurucloud.net";
            password = TestUtil.GenerateAppName();

            Client client = new Client();
            client.Target(target);
            client.AddUser(username, password);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Client client = new Client();
            client.Target(target);
            client.Login(username, password);

            foreach (App app in client.Apps())
            {
                client.DeleteApp(app.Name);
            }

            foreach (ProvisionedService service in client.ProvisionedServices())
            {
                client.DeleteService(service.Name);
            }

            client.Logout();

            string adminUser = ConfigurationManager.AppSettings["adminUsername"];
            string adminPassword = ConfigurationManager.AppSettings["adminPassword"];

            client.Login(adminUser, adminPassword);
            client.DeleteUser(username);
            client.Logout();
            foreach (string str in directoriesCreated)
            {
                Directory.Delete(str, true);
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            client = new Client();
            client.Target(target);
            client.Login(username, password);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            foreach (App app in client.Apps())
            {
                client.DeleteApp(app.Name);
            }
            client.Logout();
            Thread.Sleep(3000);
        }

        [TestMethod, TestCategory("Performance")]
        public void TC001_CloudTestAppCreate10Parallel()
        {
            // Arrange
            Dictionary<string, string> apps = new Dictionary<string, string>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();
            for (int i = 0; i < 10; i++)
            {
                string name = TestUtil.GenerateAppName();
                string url = "http://" + target.Replace("api", name);
                apps.Add(name, url);
                Thread t = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        PushApp(name, cloudTestAppDir, url);
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
                Assert.IsTrue(client.AppExists(pair.Key));
                Assert.IsTrue(TestUtil.TestUrl(pair.Value));
            }
        }

        [TestMethod, TestCategory("Performance")]
        public void TC002_CloudTestAppDelete10Parallel()
        {
            // Arrange
            Dictionary<string, string> apps = new Dictionary<string, string>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();
            for (int i = 0; i < 10; i++)
            {
                string name = TestUtil.GenerateAppName();
                string url = "http://" + target.Replace("api", name);
                apps.Add(name, url);
                PushApp(name, cloudTestAppDir, url);
            }
            foreach (string str in apps.Keys)
            {
                string name = str;
                Thread t = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        client.DeleteApp(name);
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
            Thread.Sleep(2000);
            foreach (KeyValuePair<string, string> pair in apps)
            {
                Assert.IsFalse(client.AppExists(pair.Key));
                Assert.IsFalse(TestUtil.TestUrl(pair.Value));
            }
        }

        [TestMethod, TestCategory("Performance")]
        public void TC003_CloudTestAppCreate15Parallel()
        {
            // Arrange
            Dictionary<string, string> apps = new Dictionary<string, string>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();
            for (int i = 0; i < 15; i++)
            {
                string name = TestUtil.GenerateAppName();
                string url = "http://" + target.Replace("api", name);
                apps.Add(name, url);
                Thread t = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        PushApp(name, cloudTestAppDir, url);
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
                Assert.IsTrue(client.AppExists(pair.Key));
                Assert.IsTrue(TestUtil.TestUrl(pair.Value));
            }
        }

        [TestMethod, TestCategory("Performance")]
        public void TC004_CloudTestAppDelete15Parallel()
        {
            // Arrange
            Dictionary<string, string> apps = new Dictionary<string, string>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();
            for (int i = 0; i < 15; i++)
            {
                string name = TestUtil.GenerateAppName();
                string url = "http://" + target.Replace("api", name);
                apps.Add(name, url);
                PushApp(name, cloudTestAppDir, url);
            }
            foreach (string str in apps.Keys)
            {
                string name = str;
                Thread t = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        client.DeleteApp(name);
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
            Thread.Sleep(4000);
            foreach (KeyValuePair<string, string> pair in apps)
            {
                Assert.IsFalse(client.AppExists(pair.Key));
                Assert.IsFalse(TestUtil.TestUrl(pair.Value));
            }
        }

        private void PushApp(string appName, string sourceDir, string url)
        {
            string path = TestUtil.CopyFolderToTemp(sourceDir);
            lock (lck)
            {
                directoriesCreated.Add(path);
            }
            client.Push(appName, url, path, 1, "dotNet", "iis", 128, new List<string>(), false, true);
        }
    }
}
