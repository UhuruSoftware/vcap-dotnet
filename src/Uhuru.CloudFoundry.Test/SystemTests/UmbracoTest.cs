using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.IO;
using System.Configuration;
using System.Globalization;
using Uhuru.CloudFoundry.Adaptor;
using Uhuru.CloudFoundry.Adaptor.Objects;
using Uhuru.CloudFoundry.Connection.JCO;

namespace Uhuru.CloudFoundry.Test.SystemTests
{
    [TestClass]
    public class UmbracoTest
    {
        string target;
        string userName; //= "dev@cloudfoundry.org";
        string password;//= "password1234!";
        string umbracoRootDir;
        List<string> foldersCreated = new List<string>();

        static CloudConnection cloudConnection;

        [TestInitialize]
        public void TestFixtureSetUp()
        {
            target = ConfigurationManager.AppSettings["target"];
            umbracoRootDir = ConfigurationManager.AppSettings["umbracoRootDir"];
            userName = "umbraco@uhurucloud.net";
            password = "password1234!";

            //client = new Client();
            //client.Target(target);
            //client.AddUser(userName, password);
            //client.AddUser("dev@cloudfoundry.org", "password1234!");
            //client.Login(userName, password);
            cloudConnection = TestUtil.CreateAndImplersonateUser(userName, password);
        }

        [TestCleanup]
        public void TestFixtureTearDown()
        {
            TestUtil.DeleteUser(userName, foldersCreated);
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
            Assert.IsTrue(TestUtil.TestUrl(url));
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
                Assert.IsTrue(TestUtil.TestUrl(url));
                DeleteApp(appName, serviceName);
                Thread.Sleep(10000);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            bool exists = false;
            foreach (App app in cloudConnection.Apps)
            {
                if (app.Name == appName)
                {
                    exists = true;
                    break;
                }
            }
            Assert.IsFalse(exists);
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
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.ToString());
                }
                Assert.IsTrue(TestUtil.TestUrl(url));
                apps.Add(name, service);
            }

            foreach (KeyValuePair<string, string> pair in apps)
            {
                DeleteApp(pair.Key, pair.Value);
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
                //Assert.IsFalse(client.AppExists(pair.Key));

                Assert.IsFalse(cloudConnection.ProvisionedServices.Any(service => service.Name == pair.Value));
            }
        }

        private void PushUmbraco(string appName, string serviceName, string deploymentDir, string url)
        {
            //Client cl = new Client();
            //cl.Target(target);
            //cl.Login(userName, password);

            //if (cl.AppExists(appName))
            //    cl.DeleteApp(appName);
            //if (cl.ProvisionedServices().Any(service => service.Name == serviceName))
            //    cl.DeleteService(serviceName);

            string targetDir = TestUtil.CopyFolderToTemp(deploymentDir);
            foldersCreated.Add(targetDir);
            TestUtil.UpdateWebConfigKey(targetDir + "\\Web.config", "umbracoDbDSN", "{mssql-2008#" + serviceName + "}");

            RawSystemService systemService = cloudConnection.SystemServices.FirstOrDefault(ss => ss.Vendor == "mssql");
            cloudConnection.CreateProvisionedService(systemService, serviceName);

            //if (!cl.CreateService(serviceName, "mssql"))
            //{
            //    throw new Exception("Unable to create service :(");
            //}
            TestUtil.PushApp(appName, targetDir, url, foldersCreated, cloudConnection);
            //cl.Push(appName, url, targetDir, 1, "dotNet", "iis", 128, new List<string>(), false, false, false);
            //cl.BindService(appName, serviceName);
            //cl.StartApp(appName, true, false);

            Thread.Sleep(10000);
        }

        private void DeleteApp(string name, string service)
        {
            App currentApp = cloudConnection.Apps.FirstOrDefault(app => app.Name == name);
            currentApp.Delete();
            //Client cl = new Client();
            //cl.Target(target);
            //cl.Login(userName, password);

            //if (service != null)
            //{
            //    cl.UnbindService(name, service);
            //    cl.DeleteService(service);
            //}
            //cl.DeleteApp(name);
        }
    }

}
