using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.IO;
using Uhuru.CloudFoundry.Adaptor;
using System.Security;
using System.Threading;
using Uhuru.CloudFoundry.Adaptor.Objects;
using Uhuru.CloudFoundry.Adaptor.Objects.Packaging;

namespace Uhuru.CloudFoundry.Test.SystemTests
{
    [TestClass]
    public class MongoTestAppTest
    {
        static string target;
        static string username;
        static string password;
        static string cloudTestAppDir;
        static List<string> directoriesCreated;
        static CloudConnection cloudConnection;
       
        private readonly object lck = new object();

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            directoriesCreated = new List<string>();
            target = ConfigurationManager.AppSettings["target"];
            cloudTestAppDir = Path.GetFullPath(@"..\..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\MongoTestApp\app");
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
                //client.DeleteService(service.Name);
            }

            TestUtil.DeleteUser(username, directoriesCreated);

        }

        [TestInitialize]
        public void TestInitialize()
        {
            //client = new Client();
            //client.Target(target);
            //client.Login(username, password);
        }

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
        public void TC001_MongoTestAppCreate()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);

            // Act
            PushApp(name, cloudTestAppDir, url);

            // Assert
            Assert.IsTrue(TestUtil.TestUrl(url));
            bool exists = false;
                 
            foreach (App app in cloudConnection.Apps)
            {
                if (app.Name == name)
                {
                    exists = true;
                    break;
                }
            }
            Assert.IsTrue(exists);
        }

        [TestMethod, TestCategory("System")]
        public void TC002_MongoTestAppDelete()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);
            PushApp(name, cloudTestAppDir, url);

            // Act
            foreach (App app in cloudConnection.Apps)
            {
                if (app.Name == name)
                {
                    app.Delete();
                    Thread.Sleep(1000);
                }
            }

            // Assert
            Assert.IsFalse(TestUtil.TestUrl(url));
            bool exists = false;
            foreach (App app in cloudConnection.Apps)
            {
                if (app.Name == name)
                {
                    exists = true;
                    break;
                }
            }
            Assert.IsFalse(exists);
        }

        private void PushApp(string appName, string sourceDir, string url)
        {
            string serviceName = appName + "svc";
            string path = TestUtil.CopyFolderToTemp(sourceDir);
            TestUtil.UpdateWebConfigKey(path + "\\Web.config", "mongoConnectionString", "mongodb://{" + serviceName + "#user}:{" + serviceName + "#password}@{" + serviceName + "#host}:{" + serviceName + "#port}/db");
            TestUtil.PushApp(appName, sourceDir, url, directoriesCreated, cloudConnection, "mongodb", serviceName, path);
        }

    }
}
