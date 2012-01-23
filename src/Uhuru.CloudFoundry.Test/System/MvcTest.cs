using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.Cloud;
using System.Configuration;
using System.IO;

namespace Uhuru.CloudFoundry.Test.System
{
    [TestClass]
    public class MvcTest
    {
        static string target;
        static string username;
        static string password;
        static string mvc2TestAppDir;
        static string mvc3TestAppDir;
        static List<string> directoriesCreated;

        Client client;
        private readonly object lck = new object();

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            directoriesCreated = new List<string>();
            target = ConfigurationManager.AppSettings["target"];
            mvc2TestAppDir = Path.GetFullPath(@"..\..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\Mvc2Application\app");
            mvc3TestAppDir = Path.GetFullPath(@"..\..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\Mvc3Application\app");
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
        }

        [TestMethod, TestCategory("System")]
        public void TC001_Mvc2Create()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);

            // Act
            PushApp(name, mvc2TestAppDir, url);

            // Assert
            Assert.IsTrue(TestUtil.TestUrl(url));
            Assert.IsTrue(client.AppExists(name));
        }

        [TestMethod, TestCategory("System")]
        public void TC002_Mvc2TestAppDelete()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);
            PushApp(name, mvc2TestAppDir, url);

            // Act
            client.DeleteApp(name);

            // Assert
            Assert.IsFalse(TestUtil.TestUrl(url));
            Assert.IsFalse(client.AppExists(name));
        }

        [TestMethod, TestCategory("System")]
        public void TC003_Mvc3Create()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);

            // Act
            PushApp(name, mvc3TestAppDir, url);

            // Assert
            Assert.IsTrue(TestUtil.TestUrl(url));
            Assert.IsTrue(client.AppExists(name));
        }

        [TestMethod, TestCategory("System")]
        public void TC004_Mvc3TestAppDelete()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);
            PushApp(name, mvc3TestAppDir, url);

            // Act
            client.DeleteApp(name);

            // Assert
            Assert.IsFalse(TestUtil.TestUrl(url));
            Assert.IsFalse(client.AppExists(name));
        }

        private void PushApp(string appName, string sourceDir, string url)
        {
            string path = TestUtil.CopyFolderToTemp(sourceDir);
            directoriesCreated.Add(path);

            client.Push(appName, url, path, 1, "dotNet", "iis", 128, new List<string>(), false, false, false);
            client.StartApp(appName, true, false);
        }
    }
}
