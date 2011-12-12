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

    /// <summary>
    /// A class that acts as a config manager and has a one to one mapping with a web.config file
    /// Individual settings section configurators register with this class
    /// </summary>
    public class SiteConfig : ISiteConfigManager
    {
        /// <summary>
        /// The web.config file stream
        /// </summary>
        private FileStream fileStreamWebSiteConfig;

        /// <summary>
        /// An XMLDocument containing the whole web.config file
        /// </summary>
        private XmlDocument xmlConfigRoot;

        /// <summary>
        /// The root configuration node
        /// </summary>
        private string rootConfigNode;

        /// <summary>
        /// The full path to the web.config file
        /// </summary>
        private string configFilePath;

        /// <summary>
        /// The application root directory path
        /// </summary>
        private string sitePath;

        /// <summary>
        /// If set to <c>true</c>, allow external config file references
        /// </summary>
        private bool allowExternalSource;

        /// <summary>
        /// A collection of all the registered configurators. The key is the hash code of the configurator object ( so we don't allow multiple instances per section )
        /// </summary>
        private Dictionary<int, INodeConfigRewireBase> sectionConfigurators;

        /// <summary>
        /// A collection of key - value pairs containing the textual name for each parent section
        /// </summary>
        private SortedDictionary<ParentSection, string> configParents;

        /// <summary>
        /// Initializes a new instance of the SiteConfig class
        /// </summary>
        /// <param name="webConfigPath">The path to the application root folder</param>
        /// <param name="allowExternalSource">If set to true, each individual configurator can reference an external config file</param>
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

        /// <summary>
        /// Gets or sets a value indicating whether it is allowed to specify an external 'configSource' for any specific section in web.config.
        /// </summary>
        /// <value>
        ///   <c>true</c> if it is allowed; otherwise, <c>false</c>.
        /// </value>
        public bool AllowExternalSource
        {
            get { return this.allowExternalSource; }
            set { }
        }

        /// <summary>
        /// Registers an individual section configurator and add it to the internal collection
        /// </summary>
        /// <param name="nodeConfig">The section configurator to register.</param>
        public void RegisterSectionRewire(INodeConfigRewireBase nodeConfig)
        {
            if (nodeConfig == null)
            {
                throw new ArgumentNullException("nodeConfig");
            }

            this.sectionConfigurators.Add(nodeConfig.GetType().GetHashCode(), nodeConfig);
        }

        /// <summary>
        /// Calls the rewire method on each registered configurator
        /// </summary>
        /// <param name="backupOriginal">if set to <c>true</c>, each configurator backs up the original settings.</param>
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

        /// <summary>
        /// Writes the new settings to the web.config file.
        /// </summary>
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

        /// <summary>
        /// Calls an individual configurator to rewire itself
        /// </summary>
        /// <param name="node">The configurator.</param>
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
