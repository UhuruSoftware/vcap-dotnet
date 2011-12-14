using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Uhuru.CloudFoundry.DEA.AutoWiring;

namespace Uhuru.CloudFoundry.Test.Integration
{
    [TestClass]
    public class AutoWiringTest
    {

        // private string applicationPhysicalPath = Path.GetFullPath(@"..\..\..\TestApps\CloudTestApp");
        private string applicationPhysicalPath = Path.GetFullPath(@"..\..\..\src\Uhuru.CloudFoundry.Test\TestApps\CloudTestApp\App");
        private bool nodeExisted;
        private string tempAppFolder;

        [TestInitialize]
        public void Setup()
        {
            tempAppFolder = TestUtil.CopyFolderToTemp(applicationPhysicalPath);
        }

        [TestCleanup]
        public void Teardown()
        {
            Directory.Delete(tempAppFolder, true);
            tempAppFolder = string.Empty;
            nodeExisted = false;
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void TC001_TestRewireAspEventsSection()
        {
            //Arrange
            FileStream fs = File.Open(Path.Combine(tempAppFolder, "web.config"), FileMode.Open, FileAccess.Read);
            XmlDocument testDoc = new XmlDocument();
            testDoc.Load(fs);
            XPathNavigator nav = testDoc.CreateNavigator();
            nodeExisted = (nav.SelectSingleNode("configuration/system.web/healthMonitoring") != null);
            fs.Close();
            ISiteConfigManager configManager = new SiteConfig(tempAppFolder, true);
            INodeConfigRewireBase hmNodeConfig = new HealthMonRewire();

            //Act
            hmNodeConfig.Register(configManager);
            configManager.Rewire(false);
            configManager.CommitChanges();

            //Assert
            fs = File.Open(Path.Combine(tempAppFolder, "web.config"), FileMode.Open, FileAccess.Read);
            testDoc = new XmlDocument();
            testDoc.Load(fs);
            nav = testDoc.CreateNavigator();
            int sCount = 0;
            XPathNodeIterator iter = nav.Select("configuration/system.web/healthMonitoring");
            while (iter.MoveNext())
            {
                sCount++;
            }
            if (nodeExisted && sCount != 1)
            {
                Assert.Fail("Duplicate healthMonitoring section after rewiring");
                fs.Close();
            }
            else if (nodeExisted == false && sCount != 1)
            {
                Assert.Fail("Health Monitoring node was not wired in web config");
                fs.Close();
            }
            sCount = 0;
            iter = nav.Select("configuration/system.web/healthMonitoring/@*");
            while (iter.MoveNext())
            {
                sCount++;
            }

            if (sCount == 0)
                Assert.Fail("Rewired node must have only the 'configSource' attribute set.");
            fs.Close();
        }
    }
}
