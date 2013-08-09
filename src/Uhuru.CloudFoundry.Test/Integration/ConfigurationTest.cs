using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.Configuration;
using System.IO;
using System.Configuration;

namespace Uhuru.CloudFoundry.Test.Integration
{
    [TestClass]
    [DeploymentItem("uhuruTest.config")]
    public class ConfigurationTest
    {
        [TestMethod]
        [TestCategory("Integration")]
        public void TC001_TestConfig()
        {
            UhuruSection uhuruSection = (UhuruSection)ConfigurationManager.GetSection("uhuru");

            if (!File.Exists("uhuruTest.config"))
                Assert.Fail();
            Assert.AreEqual("c:\\droplets", uhuruSection.DEA.BaseDir);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void TC002_ServiceTestConfig()
        {
            UhuruSection uhuruSection = (UhuruSection)ConfigurationManager.GetSection("uhuru");

            Assert.AreEqual(".\\", uhuruSection.Service.BaseDir);
            Assert.AreEqual(0, uhuruSection.Service.Index);
            Assert.AreEqual("198.41.0.4", uhuruSection.Service.LocalRoute);
            Assert.AreEqual("(local)", uhuruSection.Service.MSSql.Host);
            Assert.AreEqual("sa", uhuruSection.Service.MSSql.User);
            Assert.AreEqual(1433, uhuruSection.Service.MSSql.Port);
            Assert.AreEqual(200, uhuruSection.Service.Uhurufs.MaxStorageSize);
            Assert.AreEqual(false, uhuruSection.Service.FqdnHosts);
        }
    }
}
