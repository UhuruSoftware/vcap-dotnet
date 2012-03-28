using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Globalization;
using System.Configuration;
using Uhuru.CloudFoundry.Cloud;

namespace Uhuru.CloudFoundry.Test.System
{
    [TestClass]
    public class MsSqlServicesTest
    {
        private string target;
        private string username = "dbtest@uhurucloud.net";
        private string password = "password1234!";
        Client cfClient;

        [TestInitialize]
        public void TestFixtureSetup()
        {
            target = ConfigurationManager.AppSettings["target"];
            cfClient = new Client();
            cfClient.Target(target);
            cfClient.AddUser(username, password);
            cfClient.AddUser("dev@cloudfoundry.org", "password1234!");
            cfClient.Login(username, password);
        }

        [TestCleanup]
        public void TestFixtureTeardown()
        {
            foreach (App app in cfClient.Apps())
            {
                if (!String.IsNullOrEmpty(app.Services))
                {
                    cfClient.UnbindService(app.Name, app.Services);
                }
                cfClient.DeleteApp(app.Name);
            }

            foreach (ProvisionedService service in cfClient.ProvisionedServices())
            {
                cfClient.DeleteService(service.Name);
            }

            cfClient.Logout();
            cfClient.Login("dev@cloudfoundry.org", "password1234!");
            cfClient.DeleteUser(username);
        }

        [TestMethod]
        [TestCategory("System")]
        public void TC001_DatabaseCreate()
        {
            string serviceName = Guid.NewGuid().ToString();
            bool serviceProvisioned = false;

            try
            {
                cfClient.CreateService(serviceName, "mssql");
                ICollection<ProvisionedService> services = cfClient.ProvisionedServices();
                foreach (ProvisionedService svc in services)
                {
                    if (svc.Name == serviceName)
                    {
                        serviceProvisioned = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            Assert.IsTrue(serviceProvisioned);
        }

        [TestMethod]
        [TestCategory("System")]
        public void TC002_DatabaseDelete()
        {
            string serviceName = Guid.NewGuid().ToString();
            bool serviceProvisioned = false;
            bool serviceDeleted = true;

            try
            {
                cfClient.CreateService(serviceName, "mssql");
                ICollection<ProvisionedService> services = cfClient.ProvisionedServices();
                foreach (ProvisionedService svc in services)
                {
                    if (svc.Name == serviceName)
                    {
                        serviceProvisioned = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            Assert.IsTrue(serviceProvisioned);

            try
            {
                cfClient.DeleteService(serviceName);
                ICollection<ProvisionedService> services = cfClient.ProvisionedServices();
                foreach (ProvisionedService svc in services)
                {
                    if (svc.Name == serviceName)
                    {
                        serviceDeleted = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            Assert.IsTrue(serviceDeleted);
        }

        [TestMethod]
        [TestCategory("System")]
        public void TC003_3Secquential()
        {
            List<string> serviceNames = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                string serviceName = Guid.NewGuid().ToString();
                bool serviceProvisioned = false;
                try
                {
                    cfClient.CreateService(serviceName, "mssql");
                    ICollection<ProvisionedService> services = cfClient.ProvisionedServices();
                    foreach (ProvisionedService svc in services)
                    {
                        if (svc.Name == serviceName)
                        {
                            serviceProvisioned = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.ToString());
                }
                Assert.IsTrue(serviceProvisioned);
                serviceNames.Add(serviceName);
            }

            for (int i = 0; i < 3; i++)
            {
                string serviceName = serviceNames[i];
                bool serviceDeleted = true;
                try
                {
                    cfClient.DeleteService(serviceName);
                    ICollection<ProvisionedService> services = cfClient.ProvisionedServices();
                    foreach (ProvisionedService svc in services)
                    {
                        if (svc.Name == serviceName)
                        {
                            serviceDeleted = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.ToString());
                }
                Assert.IsTrue(serviceDeleted);
            }
        }

        [TestMethod]
        [TestCategory("System")]
        public void TC004_5Parallel()
        {
            List<string> services = new List<string>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();
            object lck = new object();

            for (int i = 0; i < 5; i++)
            {
                string serviceName = Guid.NewGuid().ToString();
                ThreadStart s = delegate
                {
                    try
                    {
                        cfClient.CreateService(serviceName, "mssql");

                    }
                    catch (Exception ex)
                    {
                        lock (lck)
                        {
                            exceptions.Add(ex);
                        }
                    }
                };

                services.Add(serviceName);

                Thread t = new Thread(s);
                t.Name = "createService" + i.ToString(CultureInfo.InvariantCulture);
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

            Assert.AreEqual(0, exceptions.Count);

            foreach (string service in services)
            {
                Assert.IsTrue(cfClient.ProvisionedServices().Any(ps => ps.Name == service));
            }

            threads = new List<Thread>();

            for (int i = 0; i < 5; i++)
            {
                string serviceName = services[i];
                ThreadStart s = delegate
                {
                    try
                    {
                        cfClient.DeleteService(serviceName);
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
                t.Name = "deleteService" + i.ToString(CultureInfo.InvariantCulture);
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

            Assert.AreEqual(0, exceptions.Count);

            foreach (string service in services)
            {
                Assert.IsFalse(cfClient.ProvisionedServices().Any(ps => ps.Name == service));
            }
        }

        [TestMethod]
        [TestCategory("System")]
        public void TC005_16Parallel()
        {
            foreach (ProvisionedService srv in cfClient.ProvisionedServices())
            {
                cfClient.DeleteService(srv.Name);
            }

            List<string> services = new List<string>();
            List<Thread> threads = new List<Thread>();
            List<Exception> exceptions = new List<Exception>();
            object lck = new object();

            for (int i = 0; i < 16; i++)
            {
                string serviceName = Guid.NewGuid().ToString();
                ThreadStart s = delegate
                {
                    try
                    {
                        cfClient.CreateService(serviceName, "mssql");

                    }
                    catch (Exception ex)
                    {
                        lock (lck)
                        {
                            exceptions.Add(ex);
                        }
                    }
                };

                services.Add(serviceName);

                Thread t = new Thread(s);
                t.Name = "createService" + i.ToString(CultureInfo.InvariantCulture);
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
            foreach (string service in services)
            {
                if (!cfClient.ProvisionedServices().Any(ps => ps.Name == service))
                {
                    Assert.Inconclusive("Service " + service + " was not created");
                }
            }

            threads = new List<Thread>();

            for (int i = 0; i < 16; i++)
            {
                string serviceName = services[i];
                ThreadStart s = delegate
                {
                    try
                    {
                        cfClient.DeleteService(serviceName);
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
                t.Name = "deleteService" + i.ToString(CultureInfo.InvariantCulture);
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
            foreach (string service in services)
            {
                if (cfClient.ProvisionedServices().Any(ps => ps.Name == service))
                {
                    Assert.Inconclusive("Service " + service + " was not deleted");
                }
            }
        }
    }
}
