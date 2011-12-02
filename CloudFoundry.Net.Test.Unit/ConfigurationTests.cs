using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CloudFoundry.Net.Test.Unit
{
    [TestClass]
    [DeploymentItem("uhuruTest.config")]
    public class ConfigurationTests
    {
        [TestMethod]
        public void TestConfig()
        {
            Assert.AreEqual("c:\\droplets", UhuruSection.GetSection().DEA.BaseDir);
            Assert.AreEqual("iis", UhuruSection.GetSection().DEA.Runtimes["iis"].Name);
            Assert.AreEqual("7.0", UhuruSection.GetSection().DEA.Runtimes["iis"].Environment["iisVersion"].Value);
            Assert.AreEqual("3.5;4.0", UhuruSection.GetSection().DEA.Runtimes["iis"].Environment["supportedFrameworks"].Value);
            Assert.AreEqual("true", UhuruSection.GetSection().DEA.Runtimes["iis"].Debug["simple"].Environment["useCredentials"].Value);
            Assert.AreEqual("60000", UhuruSection.GetSection().DEA.Runtimes["iis"].Debug["simple"].Environment["connectionTimeout"].Value);
            
        }
    }
}
