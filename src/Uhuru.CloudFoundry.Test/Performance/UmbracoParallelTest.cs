using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.Cloud;

namespace Uhuru.CloudFoundry.Test.Performance
{
    [TestClass]
    public class UmbracoParallelTest
    {
        string target;
        string userName;
        string password;
        string umbracoRootDir;
        List<string> foldersCreated = new List<string>();

        Client client;
        private readonly object lck = new object();

        [TestInitialize]
        public void TestFixtureSetUp()
        {
            target = ConfigurationManager.AppSettings["target"];
            umbracoRootDir = ConfigurationManager.AppSettings["umbracoRootDir"];
            userName = "umbraco@uhurucloud.net";
            password = "password1234!";

            client = new Client();
            client.Target(target);
            client.AddUser(userName, password);
            client.AddUser("dev@cloudfoundry.org", "password1234!");
            client.Login(userName, password);
        }

        [TestCleanup]
        public void TestFixtureTearDown()
        {
            foreach (App app in client.Apps())
            {
                if (!String.IsNullOrEmpty(app.Services))
                {
                    client.UnbindService(app.Name, app.Services);
                }
                client.DeleteApp(app.Name);
            }

            foreach (ProvisionedService service in client.ProvisionedServices())
            {
                client.DeleteService(service.Name);
            }

            client.Logout();
            client.Login("dev@cloudfoundry.org", "password1234!");
            client.DeleteUser(userName);

            foreach (string str in foldersCreated)
            {
                Directory.Delete(str, true);
            }
        }

        [TestMethod, Timeout(1000000), TestCategory("Performance")]
        public void TC001_Create_5Parallel()
        {
            Dictionary<string, string> apps = new Dictionary<string, string>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();
            List<string> umbracoUris = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                string name = "u" + Guid.NewGuid().ToString().Substring(0, 6);
                string service = name + "service";
                string url = "http://" + name + ".uhurucloud.net";
                umbracoUris.Add(url);
                apps.Add(name, service);
                ThreadStart s = delegate
                {
                    try
                    {
                        PushUmbraco(name, service, umbracoRootDir, url);
                    }
                    catch (Exception ex)
                    {
                        lock (lck)
                        {
                            exceptions.Add(ex);
                        }
                    }
                };
                Thread t = new Thread(s);
                t.Name = "umbraco" + i.ToString(CultureInfo.InvariantCulture);
                threads.Add(t);
            }

            foreach (Thread t in threads)
            {
                t.Start();
            }

            foreach (Thread t in threads)
            {
                t.Join(300000);
            }

            if (exceptions.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception ex in exceptions)
                {
                    sb.AppendLine(ex.ToString());
                }
                Assert.Fail("At least one exception has been  thrown:" + sb.ToString());
            }
            foreach (string uri in umbracoUris)
            {
                Assert.IsTrue(TestUtil.TestUrl(uri));
            }

            threads = new List<Thread>();
            exceptions = new List<Exception>();

            foreach (KeyValuePair<string, string> pair in apps)
            {
                string name = pair.Key;
                string service = pair.Value;
                ThreadStart s = delegate
                {
                    try
                    {
                        DeleteApp(name, service);
                    }
                    catch (Exception ex)
                    {
                        lock (lck)
                        {
                            exceptions.Add(ex);
                        }
                    }
                };
                Thread t = new Thread(s);
                t.Name = pair.Key;
                threads.Add(t);
            }

            foreach (Thread t in threads)
            {
                t.Start();
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }
            if (exceptions.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception ex in exceptions)
                {
                    sb.AppendLine(ex.ToString());
                }
                Assert.Fail("At least one exception has been  thrown:" + sb.ToString());
            }
        }

        [TestMethod, Timeout(1000000), TestCategory("Performance")]
        public void TC002_Create_10Parallel()
        {
            Dictionary<string, string> apps = new Dictionary<string, string>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();
            List<string> umbracoUris = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                string name = "u" + Guid.NewGuid().ToString().Substring(0, 6);
                string service = name + "service";
                string url = "http://" + name + ".uhurucloud.net";
                umbracoUris.Add(url);
                apps.Add(name, service);
                ThreadStart s = delegate
                {
                    try
                    {
                        PushUmbraco(name, service, umbracoRootDir, url);
                    }
                    catch (Exception ex)
                    {
                        lock (lck)
                        {
                            exceptions.Add(ex);
                        }
                    }
                };
                Thread t = new Thread(s);
                t.Name = "umbraco" + i.ToString(CultureInfo.InvariantCulture);
                threads.Add(t);
            }

            foreach (Thread t in threads)
            {
                t.Start();
            }

            foreach (Thread t in threads)
            {
                t.Join(300000);
            }

            if (exceptions.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception ex in exceptions)
                {
                    sb.AppendLine(ex.ToString());
                }
                Assert.Fail("At least one exception has been  thrown:" + sb.ToString());
            }
            foreach (string uri in umbracoUris)
            {
                Assert.IsTrue(TestUtil.TestUrl(uri));
            }

            threads = new List<Thread>();
            exceptions = new List<Exception>();

            foreach (KeyValuePair<string, string> pair in apps)
            {
                string name = pair.Key;
                string service = pair.Value;
                ThreadStart s = delegate
                {
                    try
                    {
                        DeleteApp(name, service);
                    }
                    catch (Exception ex)
                    {
                        lock (lck)
                        {
                            exceptions.Add(ex);
                        }
                    }
                };
                Thread t = new Thread(s);
                t.Name = pair.Key;
                threads.Add(t);
            }

            foreach (Thread t in threads)
            {
                t.Start();
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }
            if (exceptions.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception ex in exceptions)
                {
                    sb.AppendLine(ex.ToString());
                }
                Assert.Fail("At least one exception has been  thrown:" + sb.ToString());
            }
        }

        private void PushUmbraco(string appName, string serviceName, string deploymentDir, string url)
        {
            Client cl = new Client();
            cl.Target(target);
            cl.Login(userName, password);

            if (cl.AppExists(appName))
                cl.DeleteApp(appName);
            if (cl.ProvisionedServices().Exists(service => service.Name == serviceName))
                cl.DeleteService(serviceName);

            string targetDir = TestUtil.CopyFolderToTemp(deploymentDir);
            lock (lck)
            {
                foldersCreated.Add(targetDir);
            }
            TestUtil.UpdateWebConfigKey(targetDir + "\\Web.config", "umbracoDbDSN", "{mssql-2008#" + serviceName + "}");

            if (!cl.CreateService(serviceName, "mssql"))
            {
                throw new Exception("Unable to create service :(");
            }
            cl.Push(appName, url, targetDir, 1, "dotNet", "iis", 128, new List<string>(), false, false, false);
            cl.BindService(appName, serviceName);
            cl.StartApp(appName, true, false);

            Thread.Sleep(10000);
        }

        private void DeleteApp(string name, string service)
        {
            Client cl = new Client();
            cl.Target(target);
            cl.Login(userName, password);

            if (service != null)
            {
                cl.UnbindService(name, service);
                cl.DeleteService(service);
            }
            cl.DeleteApp(name);
        }
    }
}
