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
            Assert.AreEqual("iis", uhuruSection.DEA.Runtimes["iis"].Name);
            Assert.AreEqual("7.0", uhuruSection.DEA.Runtimes["iis"].Environment["iisVersion"].Value);
            Assert.AreEqual("3.5;4.0", uhuruSection.DEA.Runtimes["iis"].Environment["supportedFrameworks"].Value);
            Assert.AreEqual("true", uhuruSection.DEA.Runtimes["iis"].Debug["simple"].Environment["useCredentials"].Value);
            Assert.AreEqual("60000", uhuruSection.DEA.Runtimes["iis"].Debug["simple"].Environment["connectionTimeout"].Value);
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
