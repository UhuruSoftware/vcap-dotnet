using System;
using System.Collections.Generic;
using NUnit.Framework;
using CloudFoundry.Net;
using System.Configuration;
using System.Threading;

namespace CloudFoundry.Net.Test.Automation
{
    public class CFApiTest
    {
        private string deploymentPath = ConfigurationManager.AppSettings["CFApiTestAppDir"];

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            Client client = new Client();
            client.Target("api.uhurucloud.net");
            client.AddUser("dev@cloudfoundry.org", "password1234!");
        }

        [TestFixtureTearDown]
        public void TestFixtureTeardown()
        {
            //Client client = new Client();
            //client.Target("api.uhurucloud.net");
            //client.Login("dev@cloudfoundry.org", "password1234!");
            //client.DeleteUser("dev@cloudfoundry.org");
        }

        [Test]
        public void LogOnTestOk()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            string email = "dev@cloudfoundry.org";
            string password = "password1234!";
            bool expected = true;
            bool actual;
            actual = target.Login(email, password);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LogOnTestBad()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            string email = "dev@cloudfoundry.org";
            string password = "password1234";
            bool expected = false;
            bool actual;
            actual = target.Login(email, password);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AppsTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");


            if (target.AppExists("testapp"))
            {
                Assert.IsTrue(target.DeleteApp("testapp"));
            }

            Assert.IsTrue(target.Push(
                "testapp", "testapp.uhurucloud.net", deploymentPath, 1, "net", "iis",
                128, new List<string>(), false, false, false));

            Assert.IsTrue(target.StartApp("testapp", false));
            Assert.IsTrue(target.StopApp("testapp"));
            List<App> app = target.Apps();

            Assert.Less(0, app.Count);

            Assert.IsTrue(target.DeleteApp("testapp"));
        }

        [Test]
        public void FrameworksTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");

            List<Framework> frameworks = target.Frameworks();
            
            Assert.Less(0, frameworks.Count);
        }

        [Test]
        public void ProvisionedServicesTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");


            if (target.ProvisionedServices().Exists(service => service.Name == "testService"))
            {
                Assert.IsTrue(target.DeleteService("testService"));
            }

            Assert.IsTrue(target.CreateService("testService", "mssql"));
            Assert.IsTrue(target.ProvisionedServices().Exists(service => service.Name == "testService"));

            List<ProvisionedService> services = target.ProvisionedServices();
            Assert.Less(0, services.Count);

            Assert.IsTrue(target.DeleteService("testService"));
        }

        [Test]
        public void RuntimesTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");

            List<Runtime> runtimes = target.Runtimes();
            Assert.AreEqual(8, runtimes.Count);
        }

        [Test]
        public void ServicesTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");

            List<Service> services = target.Services();

            Assert.AreEqual(1, services.Count);
        }

        [Test]
        public void UsersTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");

            List<User> users = target.Users();

            Assert.Less(0, users.Count);
        }

        [Test]
        public void CreateServiceTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");

            if (target.ProvisionedServices().Exists(service => service.Name == "testService"))
            {
                Assert.IsTrue(target.DeleteService("testService"));
            }

            Assert.IsTrue(target.CreateService("testService", "mssql"));
            Assert.IsTrue(target.ProvisionedServices().Exists(service => service.Name == "testService"));
            Assert.IsTrue(target.DeleteService("testService"));
        }


        [Test]
        public void PushDeleteTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");

            if (target.AppExists("testapp"))
            {
                Assert.IsTrue(target.DeleteApp("testapp"));
            }

            Assert.IsTrue(target.Push(
                "testapp", "testapp.uhurucloud.net", deploymentPath, 1, "net", "iis", 
                128, new List<string>(), false, true, false));

            Assert.IsTrue(target.DeleteApp("testapp"));
        }

        [Test]
        public void StartStopTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");

            if (target.AppExists("testapp"))
            {
                Assert.IsTrue(target.DeleteApp("testapp"));
            }

            Assert.IsTrue(target.Push(
                "testapp", "testapp.uhurucloud.net", deploymentPath, 1, "net", "iis", 
                128, new List<string>(), false, false, false));

            Assert.IsTrue(target.StartApp("testapp", false));
            Assert.IsTrue(target.StopApp("testapp"));
            Assert.IsTrue(target.DeleteApp("testapp"));
        }

        [Test]
        public void BindTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");

            if (target.AppExists("testapp"))
            {
                Assert.IsTrue(target.DeleteApp("testapp"));
            }

            if (target.ProvisionedServices().Exists(service => service.Name == "testservice"))
            {
                Assert.IsTrue(target.DeleteService("testservice"));
            }

            Assert.IsTrue(target.Push(
                "testapp", "testapp.uhurucloud.net", deploymentPath, 1, "net", "iis",
                128, new List<string>(), false, false, false));

            Assert.IsTrue(target.CreateService("testservice", "mssql"));

            Assert.IsTrue(target.BindService("testapp", "testservice"));

            Assert.IsTrue(target.UnbindService("testapp", "testservice"));

            Assert.IsTrue(target.DeleteService("testservice"));

            Assert.IsTrue(target.DeleteApp("testapp"));
        }

        [Test]
        public void AddDeleteUserTest()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");

            string username = "u" + Guid.NewGuid().ToString("N") + "@foobar.com";

            Assert.IsTrue(target.AddUser(username, "pass1234"));
            target.Logout();
            Assert.IsTrue(target.Login(username, "pass1234"));
            target.Logout();
            target.Login("dev@cloudfoundry.org", "password1234!");
            Assert.IsTrue(target.DeleteUser(username));
        }

        [Test]
        public void MapUnmapUri()
        {
            Client target = new Client();
            target.Target("api.uhurucloud.net");
            target.Login("dev@cloudfoundry.org", "password1234!");

            if (target.AppExists("testapp"))
            {
                Assert.IsTrue(target.DeleteApp("testapp"));
            }

            if (target.ProvisionedServices().Exists(service => service.Name == "testservice"))
            {
                Assert.IsTrue(target.DeleteService("testservice"));
            }

            Assert.IsTrue(target.Push(
                "testapp", "testapp.uhurucloud.net", deploymentPath, 1, "net", "iis",
                128, new List<string>(), false, false, false));

            Assert.IsTrue(target.MapUri("testapp", "testapp2.uhurucloud.net"));

            Assert.IsTrue(target.UnmapUri("testapp", "testapp2.uhurucloud.net"));

            Assert.IsTrue(target.DeleteApp("testapp"));
        }
    }
}