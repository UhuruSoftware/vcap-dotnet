// -----------------------------------------------------------------------
// <copyright file="WebConfigRewiring.cs" company="Uhuru Software">
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Autowiring
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.XPath;
    
    public enum ParentSection
    { 
        SystemWeb 
    }

    public interface ISiteConfigManager
    {
        bool AllowExternalSource
        {
            get;
            set;
        }
        
        void RegisterSectionRewire(INodeConfigRewireBase nodeConfig);

        IXPathNavigable CreateNewSection(INodeConfigRewireBase nodeConfig);

        void Rewire(bool backupOriginal);

        void CommitChanges();
    }

    public interface INodeConfigRewireBase
    {
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

        void Register(ISiteConfigManager siteConfigManager);

        IXPathNavigable RewireConfig(IXPathNavigable configNode, bool createNewIfNotPresent);
    }
    
    public class HealthMonRewire : INodeConfigRewireBase
    {
        private string healthMonExtSource = "UhuruAspNetEventProvider.config";
        private ISiteConfigManager configmanager;
                
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
            get { return true; }
            set { }
        }

        public string ExternalSource
        {
            get { return this.healthMonExtSource; }
            set { }
        }

        public void Register(ISiteConfigManager siteConfigManager)
        {
            if (siteConfigManager == null)
            {
                throw new ArgumentNullException("siteConfigManager");
            }

            this.configmanager = siteConfigManager;
            siteConfigManager.RegisterSectionRewire(this);
        }

        public IXPathNavigable RewireConfig(IXPathNavigable configNode, bool createNewIfNotPresent)
        {
            XmlNode tempConfig = (XmlNode)configNode;

            if (tempConfig == null)
            {
                if (createNewIfNotPresent)
                {
                    tempConfig = (XmlNode)this.configmanager.CreateNewSection(this);
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

            if (this.configmanager.AllowExternalSource)
            {
                XPathNavigator nav = tempConfig.CreateNavigator();
                nav.CreateAttribute(null, "configSource", null, this.healthMonExtSource);
            }
            else
            {
                throw new NotImplementedException("logging auto-wiring only supports configuration from an external source at this time");
            }

            return tempConfig;
        }
    }

    public class SiteConfig : ISiteConfigManager
    {
        private FileStream fileStreamWebSiteConfig;
        private XmlDocument xmlConfigRoot;
        private string rootConfigNode;
        private string configFilePath;
        private string sitePath;
        private bool allowExternalSource;

        private Dictionary<int, INodeConfigRewireBase> sectionConfigurators;
        private SortedDictionary<ParentSection, string> configParents;
               
        /// <summary>
        /// Initializes a new instance of the SiteConfig class
        /// </summary>
        /// <param name="webConfigPath"></param>
        /// <param name="allowExternalSource"></param>
        public SiteConfig(string webConfigPath, bool allowExternalSource)
        {
            this.sitePath = webConfigPath;
            this.rootConfigNode = "configuration";

            this.sectionConfigurators = new Dictionary<int, INodeConfigRewireBase>();
            this.configParents = new SortedDictionary<ParentSection, string>();

            this.configParents[ParentSection.SystemWeb] = "system.web";

            this.configFilePath = Path.Combine(webConfigPath, "Web.config");

            this.fileStreamWebSiteConfig = File.Open(this.configFilePath, FileMode.Open, FileAccess.Read);

            this.xmlConfigRoot = new XmlDocument();
            this.xmlConfigRoot.Load(this.fileStreamWebSiteConfig);

            this.fileStreamWebSiteConfig.Close();

            this.allowExternalSource = allowExternalSource;
        }
        
        public bool AllowExternalSource
        {
            get { return this.allowExternalSource; }
            set { }
        }

        public void RegisterSectionRewire(INodeConfigRewireBase nodeConfig)
        {
            if (nodeConfig == null)
            {
                throw new ArgumentNullException("nodeConfig");
            }

            this.sectionConfigurators.Add(nodeConfig.GetType().GetHashCode(), nodeConfig);
        }

        public void Rewire(bool backupOriginal)
        {
            if (backupOriginal == true)
            {
                throw new NotImplementedException("Site config: Configuration backup not implemented yet.");
            }

            foreach (INodeConfigRewireBase node in this.sectionConfigurators.Values)
            {
                this.RewireIndividualSection(node);
                if (node.HasExternalSource)
                {
                    string dstCfgFilePath = Path.Combine(this.sitePath, node.ExternalSource);

                    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CloudFoundry.Net.IIS.Resources.UhuruAspNetEventProvider.config");
                    byte[] file = new byte[stream.Length];
                    stream.Read(file, 0, (int)stream.Length);
                    File.WriteAllBytes(dstCfgFilePath, file);
                    stream.Close();

                    string srcNetlibPath = Assembly.GetAssembly(typeof(Uhuru.Autowiring.SiteConfig)).Location;
                    string dstNetLibPath = Path.Combine(this.sitePath, "bin", "CloudFoundry.Net.IIS.dll");
                    File.Copy(srcNetlibPath, dstNetLibPath);
                }
            }
        }

        public void CommitChanges()
        {
            this.fileStreamWebSiteConfig = File.Open(this.configFilePath, FileMode.Create, FileAccess.Write);
            this.xmlConfigRoot.Save(this.fileStreamWebSiteConfig);
            this.fileStreamWebSiteConfig.Close();
        }

        /// <summary>
        /// creates a new xml node corresponding to a new section
        /// </summary>
        /// <param name="nodeConfig">the configuration info of the new section</param>
        /// <returns>the corresponding xml node</returns>
        public IXPathNavigable CreateNewSection(INodeConfigRewireBase nodeConfig)
        {
            if (nodeConfig == null)
            {
                throw new ArgumentNullException("nodeConfig");
            }

            XmlNode newConfigNode = this.xmlConfigRoot.CreateNode(XmlNodeType.Element, nodeConfig.ConfigSectionName, null);
            return newConfigNode;
        }

        private void RewireIndividualSection(INodeConfigRewireBase node)
        {
            XmlNode firstParent = this.xmlConfigRoot.SelectSingleNode(this.rootConfigNode).SelectSingleNode(this.configParents[node.ConfigParent]);
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