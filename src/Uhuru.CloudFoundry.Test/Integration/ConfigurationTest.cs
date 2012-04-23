using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.Configuration;
using System.IO;

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
            if (!File.Exists("uhuruTest.config"))
                Assert.Fail();
            Assert.AreEqual("c:\\droplets", UhuruSection.GetSection().DEA.BaseDir);
            Assert.AreEqual("iis", UhuruSection.GetSection().DEA.Runtimes["iis"].Name);
            Assert.AreEqual("7.0", UhuruSection.GetSection().DEA.Runtimes["iis"].Environment["iisVersion"].Value);
            Assert.AreEqual("3.5;4.0", UhuruSection.GetSection().DEA.Runtimes["iis"].Environment["supportedFrameworks"].Value);
            Assert.AreEqual("true", UhuruSection.GetSection().DEA.Runtimes["iis"].Debug["simple"].Environment["useCredentials"].Value);
            Assert.AreEqual("60000", UhuruSection.GetSection().DEA.Runtimes["iis"].Debug["simple"].Environment["connectionTimeout"].Value);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void TC002_ServiceTestConfig()
        {
            Assert.AreEqual(".\\", UhuruSection.GetSection().Service.BaseDir);
            Assert.AreEqual(0, UhuruSection.GetSection().Service.Index);
            Assert.AreEqual("198.41.0.4", UhuruSection.GetSection().Service.LocalRoute);
            Assert.AreEqual("(local)", UhuruSection.GetSection().Service.MSSql.Host);
            Assert.AreEqual("sa", UhuruSection.GetSection().Service.MSSql.User);
            Assert.AreEqual(1433, UhuruSection.GetSection().Service.MSSql.Port);
        }
    }
}
