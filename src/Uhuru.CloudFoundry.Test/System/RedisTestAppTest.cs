using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.IO;
using Uhuru.CloudFoundry.Adaptor;
using System.Security;
using Uhuru.CloudFoundry.Adaptor.Objects;
using Uhuru.CloudFoundry.Connection.JCO;

namespace Uhuru.CloudFoundry.Test.System
{
    [TestClass]
    public class RedisTestAppTest
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
            //target = ConfigurationManager.AppSettings["target"];
            cloudTestAppDir = Path.GetFullPath(@"..\..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\RedisTestApp\app");
            username = TestUtil.GenerateAppName() + "@uhurucloud.net";
            password = TestUtil.GenerateAppName();


            cloudConnection = TestUtil.CreateAndImplersonateUser(username, password);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            //Client client = new Client();
            //client.Target(target);
            //client.Login(username, password);

            //foreach (App app in client.Apps())
            //{
            //    client.DeleteApp(app.Name);
            //}

            //foreach (ProvisionedService service in client.ProvisionedServices())
            //{
            //    client.DeleteService(service.Name);
            //}

            //target = ConfigurationManager.AppSettings["target"];
            //CloudCredentialsEncryption encryptor = new CloudCredentialsEncryption();
            //SecureString encryptedPassword = encryptor.Decrypt(ConfigurationManager.AppSettings["adminPassword"].ToString());
            //CloudManager cloudManager = CloudManager.Instance();
            //CloudTarget cloudTarget = new CloudTarget(ConfigurationManager.AppSettings["adminUsername"].ToString(), encryptedPassword, new Uri(target));
            //cloudConnection = cloudManager.GetConnection(cloudTarget);

            //User tempUser = cloudConnection.Users.First(usr => usr.Email == username);
            //tempUser.Delete();
            //foreach (string str in directoriesCreated)
            //{
            //    Directory.Delete(str, true);
            //}
            TestUtil.DeleteUser(username, directoriesCreated);
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
        public void TC001_RedisTestAppCreate()
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
        public void TC002_RedisTestAppDelete()
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
                    break;
                }
            }
            //client.DeleteApp(name);

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
            string path = TestUtil.CopyFolderToTemp(sourceDir);
            directoriesCreated.Add(path);

            string serviceName = appName + "svc";
            RawSystemService systemService = cloudConnection.SystemServices.FirstOrDefault(ss => ss.Vendor == "redis");
            cloudConnection.CreateProvisionedService(systemService, serviceName);
            //Assert.IsTrue(client.CreateService(serviceName, "redis"));

            TestUtil.UpdateWebConfigKey(path + "\\Web.config", "redisHost", "{" + serviceName + "#host}");
            TestUtil.UpdateWebConfigKey(path + "\\Web.config", "redisPort", "{" + serviceName + "#port}");
            TestUtil.UpdateWebConfigKey(path + "\\Web.config", "redisPassword", "{" + serviceName + "#password}");
            TestUtil.PushApp(appName, path, url, directoriesCreated, cloudConnection);
            //client.Push(appName, url, path, 1, "dotNet", "iis", 128, new List<string>(), false, false, false);
            //client.BindService(appName, serviceName);
            //client.StartApp(appName, true, false);
        }
    }
}
