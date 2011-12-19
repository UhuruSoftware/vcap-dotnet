using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System;
using Uhuru.CloudFoundry.Cloud;

namespace Uhuru.CloudFoundry.Test.System
{
    [TestClass, Ignore]
    public class CloudTestAppTest
    {
        static string target;
        static string username;
        static string password;
        static string cloudTestAppDir;
        static List<string> directoriesCreated;

        Client client;
        private readonly object lck = new object();

        [ClassInitialize]
        public static void ClassInitialize()
        {
            directoriesCreated = new List<string>();
            target = ConfigurationManager.AppSettings["target"];
            cloudTestAppDir = Path.GetFullPath(@"..\..\..\TestApps\CloudTestApp");
            username = "cloudTestApp@uhurucloud.net";
            password = Guid.NewGuid().ToString();

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
            client.Logout();
        }

        [TestMethod, TestCategory("System")]
        public void TC001_CloudTestAppCreate()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = target.Replace("api", name);
            
            // Act
            PushApp(name, cloudTestAppDir, url);
            
            // Assert
            Assert.IsTrue(TestUtil.TestUrl(url));
            Assert.IsTrue(client.AppExists(name));
        }

        [TestMethod, TestCategory("System")]
        public void TC002_CloudTestAppDelete()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = target.Replace("api", name);
            PushApp(name, cloudTestAppDir, url);

            // Act
            client.DeleteApp(name);

            // Assert
            Assert.IsFalse(TestUtil.TestUrl(url));
            Assert.IsFalse(client.AppExists(name));
        }

        [TestMethod, TestCategory("System")]
        public void TC003_CloudTestAppCreate5Sequential()
        {
            // Arrange
            Dictionary<string, string> apps = new Dictionary<string, string>();
            for (int i = 0; i < 5; i++)
            {
                string name = TestUtil.GenerateAppName();
                string url = target.Replace("api", name);
                apps.Add(name, url);
            }

            // Act
            foreach (KeyValuePair<string, string> pair in apps)
            {
                PushApp(pair.Key, cloudTestAppDir, pair.Value);
            }

            // Assert
            foreach (KeyValuePair<string, string> pair in apps)
            {
                Assert.IsTrue(client.AppExists(pair.Key));
                Assert.IsTrue(TestUtil.TestUrl(pair.Value));
            }
        }

        [TestMethod, TestCategory("System")]
        public void TC004_CloudTestAppDelete5Sequential()
        {

        }

        [TestMethod, TestCategory("System")]
        public void TC005_CloudTestAppCreate5Parallel()
        {

        }

        [TestMethod, TestCategory("System")]
        public void TC006_CloudTestAppDelete5Parallel()
        {

        }

        [TestMethod, TestCategory("System")]
        public void TC007_CloudTestAppCreate10Parallel()
        {

        }

        [TestMethod, TestCategory("System")]
        public void TC008_CloudTestAppDelete10Parallel()
        {

        }

        private void PushApp(string appName, string sourceDir, string url)
        {
            string path = TestUtil.CopyFolderToTemp(sourceDir);
            lock (lck)
            {
                directoriesCreated.Add(path);
            }
            client.Push(appName, url, path, 1, "dotNet", "iis", 64, new List<string>(), false, true);
        }
    }
}
