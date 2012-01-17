using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.Cloud;

namespace Uhuru.CloudFoundry.Test.System
{
    [TestClass]
    public class DifferentDotNetVersionTest
    {
        static string target;
        static string username;
        static string password;
        static string cloudTestAppDir40 = @"..\..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\CloudTestApp\app";
        static string cloudTestAppDir35 = @"..\..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\CloudTestApp35\app";
        static string cloudTestAppDir20 = @"..\..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\CloudTestApp20\app";
        static List<string> directoriesCreated;

        Client client;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            directoriesCreated = new List<string>();
            target = ConfigurationManager.AppSettings["target"];
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
        public void TC001_CloudTestApp20()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);

            // Act
            PushApp(name, cloudTestAppDir20, url);

            // Assert
            Assert.IsTrue(TestUtil.TestUrl(url));
            Assert.IsTrue(client.AppExists(name));
            WebClient webClient = new WebClient();
            string html = webClient.DownloadString(url);
            Assert.IsTrue(html.Contains("This is a .net 2.0 web application!"));
        }

        [TestMethod, TestCategory("System")]
        public void TC002_CloudTestApp35()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);

            // Act
            PushApp(name, cloudTestAppDir35, url);

            // Assert
            Assert.IsTrue(TestUtil.TestUrl(url));
            Assert.IsTrue(client.AppExists(name));
            WebClient webClient = new WebClient();
            string html = webClient.DownloadString(url);
            Assert.IsTrue(html.Contains("This is a .net 3.5 web application!"));
        }

        [TestMethod, TestCategory("System")]
        public void TC003_CloudTestApp40()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);

            // Act
            PushApp(name, cloudTestAppDir40, url);

            // Assert
            Assert.IsTrue(TestUtil.TestUrl(url));
            Assert.IsTrue(client.AppExists(name));
            WebClient webClient = new WebClient();
            string html = webClient.DownloadString(url);
            Assert.IsTrue(html.Contains("My ASP.NET Application"));
        }

        private void PushApp(string appName, string sourceDir, string url)
        {
            string path = TestUtil.CopyFolderToTemp(sourceDir);
            directoriesCreated.Add(path);
            client.Push(appName, url, path, 1, "dotNet", "iis", 128, new List<string>(), false, true);
        }
    }
}
