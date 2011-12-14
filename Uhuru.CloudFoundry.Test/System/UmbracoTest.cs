using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.IO;
using Uhuru.CloudFoundry.Cloud;
using System.Configuration;
using System.Globalization;

namespace Uhuru.CloudFoundry.Test.System
{
    [TestClass]
    public class UmbracoTest
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

        [TestMethod, Timeout(300000), TestCategory("System")]
        public void TC001_Create_UmbracoSimple()
        {
            string appName = "umbraco";
            string serviceName = "umbracoMssqlService";
            string url = "http://umbraco.uhurucloud.net";
            try
            {
                PushUmbraco(appName, serviceName, umbracoRootDir, url);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            TestUtil.TestUrl(url);
        }

        [TestMethod, Timeout(300000), TestCategory("System")]
        public void TC002_Delete_UmbracoSimple()
        {
            string appName = "testdelete";
            string serviceName = "testdeleteMssqlService";
            string url = "http://testdelete.uhurucloud.net";

            try
            {
                PushUmbraco(appName, serviceName, umbracoRootDir, url);
                TestUtil.TestUrl(url);
                DeleteApp(appName, serviceName);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            Assert.IsFalse(client.AppExists(appName));
        }

        [TestMethod, Timeout(900000), TestCategory("System")]
        public void TC003_Create_3Secquential()
        {
            Dictionary<string, string> apps = new Dictionary<string, string>();
            for (int i = 0; i < 3; i++)
            {
                string name = "umbraco" + i.ToString(CultureInfo.InvariantCulture);
                string service = name + "service";
                string url = "http://" + name + ".uhurucloud.net";
                try
                {
                    PushUmbraco(name, service, umbracoRootDir, url);
                    TestUtil.TestUrl(url);
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.ToString());
                }
                apps.Add(name, service);
            }

            foreach (KeyValuePair<string, string> pair in apps)
            {
                DeleteApp(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<string, string> pair in apps)
            {
                Assert.IsFalse(client.AppExists(pair.Key));
                Assert.IsFalse(client.ProvisionedServices().Exists(service => service.Name == pair.Value));
            }
        }

        [TestMethod, Timeout(1000000), TestCategory("System")]
        public void TC004_Create_5Parallel()
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
                Assert.Inconclusive("At least one exception has been  thrown:" + sb.ToString());
            }
            foreach (string uri in umbracoUris)
            {
                TestUtil.TestUrl(uri);
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
                Assert.Inconclusive("At least one exception has been  thrown:" + sb.ToString());
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

            cl.CreateService(serviceName, "mssql");
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
