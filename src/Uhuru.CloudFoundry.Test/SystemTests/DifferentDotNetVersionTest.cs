using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.Adaptor;
using Uhuru.CloudFoundry.Adaptor.Objects;

namespace Uhuru.CloudFoundry.Test.SystemTests
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
        //static string curentFramework = "iis";
        static List<string> directoriesCreated;
        static CloudConnection cloudConnection;
        

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            directoriesCreated = new List<string>();
            target = ConfigurationManager.AppSettings["target"];
            username = TestUtil.GenerateAppName() + "@uhurucloud.net";
            password = TestUtil.GenerateAppName();

            cloudConnection = TestUtil.CreateAndImplersonateUser(username, password);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
         

            TestUtil.DeleteUser(username, directoriesCreated);
            //foreach (string str in directoriesCreated)
            //{
            //    Directory.Delete(str, true);
            //}
        }

        //[TestInitialize]
        //public void TestInitialize()
        //{
        //    client = new Client();
        //    client.Target(target);
        //    client.Login(username, password);
        //}

        //[TestCleanup]
        //public void TestCleanup()
        //{
        //    foreach (App app in client.Apps())
        //    {
        //        client.DeleteApp(app.Name);
        //    }
        //    client.Logout();
        //}

        [TestMethod, TestCategory("System")]
        public void TC001_CloudTestApp20()
        {
            // Arrange
            string name = TestUtil.GenerateAppName();
            string url = "http://" + target.Replace("api", name);

            // Act
            TestUtil.PushApp(name, cloudTestAppDir20, url, directoriesCreated, cloudConnection);

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
            TestUtil.PushApp(name, cloudTestAppDir35, url, directoriesCreated, cloudConnection);
           

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
            TestUtil.PushApp(name, cloudTestAppDir40, url, directoriesCreated, cloudConnection);
            

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
            WebClient webClient = new WebClient();
            string html = webClient.DownloadString(url);
            Assert.IsTrue(html.Contains("My ASP.NET Application"));
        }

        //private void PushApp(string appName, string sourceDir, string url)
        //{
        //    string path = TestUtil.CopyFolderToTemp(sourceDir);
        //    directoriesCreated.Add(path);
        //    client.Push(appName, url, path, 1, "dotNet", "iis", 128, new List<string>(), false, true);
        //}
    }
}
