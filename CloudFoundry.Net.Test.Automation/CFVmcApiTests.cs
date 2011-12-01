using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CloudFoundry.Net;
using System.Globalization;

namespace CloudFoundry.Net.Test.Automation
{
    [Ignore]
    public class VmcClientTests
    {
        [Test]
        public void RunVMCTest()
        {
            VmcClient target = new VmcClient();
            string parameters = "target api.uhurucloud.net";
            string expected = "succesfully";
            string actual;
            actual = target.RunVMC(parameters, true);
            Assert.IsTrue(actual.ToUpperInvariant().StartsWith(expected, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void RunUserOperationsTest()
        {
            VmcClient cfClient = new VmcClient();
            //Assert.IsTrue(cfClient.Target("api.uhurucloud.net"), "vmc target failed!");
            Assert.IsTrue(cfClient.AddUser("foobar@vmware.com", "pass1234"), "VMC add user failed!");
            Assert.IsTrue(cfClient.Login("foobar@vmware.com", "pass1234"), "VMC login failed!");
            Assert.IsTrue(cfClient.DeleteUser("foobar@vmware.com"), "VMC delete failed");
        }

        [Test]
        public void RunTargetTest()
        {
            VmcClient cfClient = new VmcClient();
            Assert.IsTrue(cfClient.Target("api.uhurucloud.net"));
        }

        [Test]
        public void GetAppsTest()
        {
            VmcClient cfClient = new VmcClient();
            cfClient.Target("api.uhurucloud.net");
            //            Assert.IsTrue(cfClient.AddUser("foobar@vmware.com", "pass1234"), "vmc add user failed!");
            //            Assert.IsTrue(cfClient.Login("foobar@vmware.com", "pass1234"), "vmc login failed!");
            Assert.IsTrue(cfClient.Login("test@test.com", "test"), "vmc login failed!");
            List<App> apps = cfClient.Apps();

            Assert.AreEqual(2, apps.Count);
            Assert.AreEqual("castravete", apps[0].Name);


            //Assert.AreEqual(0, apps.Count, "New user should have no apps.");

            //Assert.IsTrue(cfClient.DeleteUser("foobar@vmware.com"), "vmc delete failed");
        }

        [Test]
        public void GetServicesTest()
        {
            VmcClient cfClient = new VmcClient();
            cfClient.Target("api.uhurucloud.net");

            Assert.IsTrue(cfClient.Login("foobar@vmware.com", "pass"), "VMC login failed!");
            List<Service> services = cfClient.Services();

            Assert.IsTrue(services.Count > 0);
            Assert.AreEqual("mongodb", services[0].Vendor);


            //Assert.AreEqual(0, apps.Count, "New user should have no apps.");

            //Assert.IsTrue(cfClient.DeleteUser("foobar@vmware.com"), "vmc delete failed");
        }

        [Test]
        public void GetProvisionedServicesTest()
        {
            VmcClient cfClient = new VmcClient();
            cfClient.Target("api.uhurucloud.net");

            Assert.IsTrue(cfClient.Login("foobar@vmware.com", "pass"), "VMC login failed!");
            List<ProvisionedService> services = cfClient.ProvisionedServices();

            Assert.IsTrue(services.Count == 1);
            Assert.AreEqual("myredis", services[0].Name);

            //Assert.AreEqual(0, apps.Count, "New user should have no apps.");
            //Assert.IsTrue(cfClient.DeleteUser("foobar@vmware.com"), "vmc delete failed");
        }
    }
}
