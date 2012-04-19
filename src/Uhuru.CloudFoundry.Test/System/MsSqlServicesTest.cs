using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Globalization;
using System.Configuration;
using Uhuru.CloudFoundry.Adaptor;
using System.Security;
using Uhuru.CloudFoundry.Adaptor.Objects;
using Uhuru.CloudFoundry.Connection.JCO;
using Uhuru.CloudFoundry.Connection;

namespace Uhuru.CloudFoundry.Test.System
{
    [TestClass]
    public class MsSqlServicesTest
    {
        private string target;
        private string username = "dbtest@uhurucloud.net";
        private string password = "password1234!";
        CloudConnection cloudConnection;

        [TestInitialize]
        public void TestFixtureSetup()
        {


            cloudConnection = TestUtil.CreateAndImplersonateUser(username, password);
            
            
        }

        [TestCleanup]
        public void TestFixtureTeardown()
        {
            TestUtil.DeleteUser(username, new List<string>());
        }

        [TestMethod]
        [TestCategory("System")]
        public void TC001_DatabaseCreate()
        {
            string serviceName = Guid.NewGuid().ToString();
            bool serviceProvisioned = false;

            try
            {
                RawSystemService systemService = cloudConnection.SystemServices.First(ss => ss.Vendor == "mssql");
                cloudConnection.CreateProvisionedService(systemService, serviceName, true);
                Thread.Sleep(10000);
                ICollection<ProvisionedService> services = cloudConnection.ProvisionedServices;
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
                RawSystemService systemService = cloudConnection.SystemServices.First(ss => ss.Vendor == "mssql");
                cloudConnection.CreateProvisionedService(systemService, serviceName, true);
                Thread.Sleep(10000);
                ICollection<ProvisionedService> services = cloudConnection.ProvisionedServices;
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
                ProvisionedService provisionedService = cloudConnection.ProvisionedServices.FirstOrDefault(ps => ps.Name == serviceName);
                provisionedService.Delete();
                Thread.Sleep(10000);
                ICollection<ProvisionedService> services = cloudConnection.ProvisionedServices;
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
                    cloudConnection.CreateProvisionedService(cloudConnection.SystemServices.FirstOrDefault(ss => ss.Vendor == "mssql"), serviceName, true);
                    Thread.Sleep(10000);
                    ICollection<ProvisionedService> services = cloudConnection.ProvisionedServices;
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
                    ProvisionedService provService = cloudConnection.ProvisionedServices.FirstOrDefault(ps => ps.Name == serviceName);
                    provService.Delete();
                    Thread.Sleep(10000);
                    ICollection<ProvisionedService> services = cloudConnection.ProvisionedServices;
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
                        cloudConnection.CreateProvisionedService(cloudConnection.SystemServices.FirstOrDefault(ss => ss.Vendor == "mssql"), serviceName, true);
                        Thread.Sleep(10000);

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

            Thread.Sleep(20000);
            foreach (string service in services)
            {
                Assert.IsTrue(cloudConnection.ProvisionedServices.Any(ps => ps.Name == service));
            }

            threads = new List<Thread>();

            for (int i = 0; i < 5; i++)
            {
                string serviceName = services[i];
                ThreadStart s = delegate
                {
                    try
                    {
                        cloudConnection.ProvisionedServices.FirstOrDefault(ps => ps.Name == serviceName).Delete();
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
            Thread.Sleep(20000);
            foreach (string service in services)
            {
                Assert.IsFalse(cloudConnection.ProvisionedServices.Any(ps => ps.Name == service));
            }
        }

        [TestMethod]
        [TestCategory("System")]
        public void TC005_16Parallel()
        {
            foreach (ProvisionedService srv in cloudConnection.ProvisionedServices)
            {
                cloudConnection.ProvisionedServices.FirstOrDefault(pv => pv.Name == srv.Name).Delete();
                //cfClient.DeleteService(srv.Name);
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
                        cloudConnection.CreateProvisionedService(cloudConnection.SystemServices.FirstOrDefault(ss => ss.Vendor == "mssql"), serviceName, true);
                        

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
            Thread.Sleep(10000);
            foreach (string service in services)
            {
                if (!cloudConnection.ProvisionedServices.Any(ps => ps.Name == service))
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
                        cloudConnection.ProvisionedServices.FirstOrDefault(pv => pv.Name == serviceName).Delete();
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
                if (cloudConnection.ProvisionedServices.Any(ps => ps.Name == service))
                {
                    Assert.Inconclusive("Service " + service + " was not deleted");
                }
            }
        }
    }
}
