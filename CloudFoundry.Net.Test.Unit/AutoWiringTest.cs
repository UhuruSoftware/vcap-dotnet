using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.Autowiring;
using Uhuru.Utilities.AutoWiring;

namespace CloudFoundry.Net.Test.Unit
{
    [TestClass]
    public class WebConfigAutoWireTest
    {
        // private string applicationPhysicalPath = Path.GetFullPath(@"..\..\..\TestApps\CloudTestApp");
        private string applicationPhysicalPath = Path.GetFullPath(@"..\cf.net\TestApps\CloudTestApp");
        private bool nodeExisted;
        private string tempAppFolder;

        [TestInitialize]
        public void Setup()
        {
            tempAppFolder = Helper.CopyFolderToTemp(applicationPhysicalPath);
        }

        [TestCleanup]
        public void Teardown()
        {
            Directory.Delete(tempAppFolder, true);
            tempAppFolder = string.Empty;
            nodeExisted = false;
        }

        [TestMethod]
        public void TestRewireAspEventsSection()
        {
            try
            {
                FileStream fs = File.Open(Path.Combine(tempAppFolder, "web.config"), FileMode.Open, FileAccess.Read);
                XmlDocument testDoc = new XmlDocument();

                testDoc.Load(fs);

                XPathNavigator nav = testDoc.CreateNavigator();

                nodeExisted = (nav.SelectSingleNode("configuration/system.web/healthMonitoring") != null);

                fs.Close();

                ISiteConfigManager configManager = new SiteConfig(tempAppFolder, true);
                INodeConfigRewireBase hmNodeConfig = new HealthMonRewire();
                hmNodeConfig.Register(configManager);
                configManager.Rewire(false);
                configManager.CommitChanges();

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
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }
    }
}
