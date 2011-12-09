// -----------------------------------------------------------------------
// <copyright file="SiteConfig.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.AutoWiring
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.XPath;

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

                    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Uhuru.CloudFoundry.DEA.Plugins.AutoWiring.UhuruAspNetEventProvider.config");
                    byte[] file = new byte[stream.Length];
                    stream.Read(file, 0, (int)stream.Length);
                    File.WriteAllBytes(dstCfgFilePath, file);
                    stream.Close();
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
