using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Collections;

using System.Reflection;


namespace CloudFoundry.Net.IIS
{
    public enum ParentSection { SystemWeb };

    public interface ISiteConfigManager
    {
        void RegisterSectionRewire(INodeConfigRewireBase nodeConfig);
        IXPathNavigable CreateNewSection(INodeConfigRewireBase nodeConfig);
        void Rewire(bool backupOriginal);
        void CommitChanges();

        bool AllowExternalSource
        {
            get;
            set;
        }
    }

    public interface INodeConfigRewireBase
    {
        void Register(ISiteConfigManager siteConfigManager);
        IXPathNavigable RewireConfig(IXPathNavigable configNode, bool createNewIfNotPresent);

        string ConfigSectionName
        {
            get;
            set;
        }

        ParentSection ConfigParent
        {
            get;
            set;
        }

        bool HasExternalSource
        {
            get;
            set;
        }

        string ExternalSource
        {
            get;
            set;
        }
    }


    public class HealthMonRewire : INodeConfigRewireBase
    {
        private string hmExtSource = "UhuruAspNetEventProvider.config";

        private ISiteConfigManager configmanager;

        public void Register(ISiteConfigManager siteConfigManager)
        {
            if (siteConfigManager == null)
            {
                throw new ArgumentNullException("siteConfigManager");
            }
            configmanager = siteConfigManager;
            siteConfigManager.RegisterSectionRewire(this);
        }

        public IXPathNavigable RewireConfig(IXPathNavigable configNode, bool createNewIfNotPresent)
        {
            XmlNode tempConfig = (XmlNode)configNode;

            if (tempConfig == null)
            {
                if (createNewIfNotPresent)
                {
                    tempConfig = (XmlNode)configmanager.CreateNewSection(this);
                }
                else
                {
                    throw new ArgumentException("Creation of a new config section not allowed");
                }
            }
            else
            {
                tempConfig.RemoveAll();
            }

            if (configmanager.AllowExternalSource)
            {
                XPathNavigator nav = tempConfig.CreateNavigator();
                nav.CreateAttribute(null, "configSource", null, hmExtSource);
            }
            else
            {
                throw new NotImplementedException("logging auto-wiring only supports configuration from an external source at this time");
            }

            return tempConfig;
        }

        public string ConfigSectionName
        {
            get { return "healthMonitoring"; }
            set { }
        }

        public ParentSection ConfigParent
        {
            get { return ParentSection.SystemWeb; }
            set { }
        }

        public bool HasExternalSource
        {
            get
            {
                return true;
            }
            set
            {
            }
        }


        public string ExternalSource
        {
            get { return hmExtSource; }
            set { }
        }
    }

    public class SiteConfig : ISiteConfigManager
    {
        private FileStream fsWebSiteConfig;
        private XmlDocument xmlConfigRoot;
        private string rootConfigNode;
        private string configFilePath;
        private string sitePath;

        private Dictionary<int, INodeConfigRewireBase> sectionConfigurators;
        private SortedDictionary<ParentSection, string> configParents;

        public bool AllowExternalSource
        {
            get { return true; }
            set { }
        }

        public SiteConfig(string webConfigPath, bool allowExternalSource)
        {
            sitePath = webConfigPath;
            rootConfigNode = "configuration";

            sectionConfigurators = new Dictionary<int, INodeConfigRewireBase>();
            configParents = new SortedDictionary<ParentSection, string>();

            configParents[ParentSection.SystemWeb] = "system.web";

            configFilePath = Path.Combine(webConfigPath, "Web.config");

            fsWebSiteConfig = File.Open(configFilePath, FileMode.Open, FileAccess.Read);

            xmlConfigRoot = new XmlDocument();
            xmlConfigRoot.Load(fsWebSiteConfig);

            fsWebSiteConfig.Close();

            AllowExternalSource = allowExternalSource;
        }

        public void RegisterSectionRewire(INodeConfigRewireBase nodeConfig)
        {
            if (nodeConfig == null)
            {
                throw new ArgumentNullException("nodeConfig");
            }
            sectionConfigurators.Add(nodeConfig.GetType().GetHashCode(), nodeConfig);
        }

        public void Rewire(bool backupOriginal)
        {
            if (backupOriginal == true)
                throw new NotImplementedException("Site config: Configuration backup not implemented yet.");

            foreach (INodeConfigRewireBase node in sectionConfigurators.Values)
            {
                RewireIndividualSection(node);
                if (node.HasExternalSource)
                {
                    string dstCfgFilePath = Path.Combine(sitePath, node.ExternalSource);

                    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CloudFoundry.Net.IIS.Resources.UhuruAspNetEventProvider.config");
                    byte[] file = new byte[stream.Length];
                    stream.Read(file, 0, (int)stream.Length);
                    File.WriteAllBytes(dstCfgFilePath, file);
                    stream.Close();

                    string srcNetlibPath = Assembly.GetAssembly(typeof(CloudFoundry.Net.IIS.SiteConfig)).Location;
                    string dstNetLibPath = Path.Combine(sitePath, "bin", "CloudFoundry.Net.IIS.dll");
                    File.Copy(srcNetlibPath, dstNetLibPath);
                }
            }
        }

        public void CommitChanges()
        {
            fsWebSiteConfig = File.Open(configFilePath, FileMode.Create, FileAccess.Write);
            xmlConfigRoot.Save(fsWebSiteConfig);
            fsWebSiteConfig.Close();
        }

        public IXPathNavigable CreateNewSection(INodeConfigRewireBase nodeConfig)
        {
            if (nodeConfig == null)
            {
                throw new ArgumentNullException("nodeConfig");
            }
            XmlNode newConfigNode = xmlConfigRoot.CreateNode(XmlNodeType.Element, nodeConfig.ConfigSectionName, null);
            return newConfigNode;
        }

        private void RewireIndividualSection(INodeConfigRewireBase node)
        {
            XmlNode firstParent = xmlConfigRoot.SelectSingleNode(rootConfigNode).SelectSingleNode(configParents[node.ConfigParent]);

            XmlNode oldConfig = firstParent.SelectSingleNode(node.ConfigSectionName);

            XmlNode newConfig = (XmlNode)node.RewireConfig(oldConfig, true);

            XPathNavigator nav = firstParent.CreateNavigator();

            if (oldConfig == null)
            {
                nav.AppendChild(newConfig.CreateNavigator());
            }
            else
            {
                firstParent.ReplaceChild(newConfig, oldConfig);
            }
        }
    }
}