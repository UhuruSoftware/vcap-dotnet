// -----------------------------------------------------------------------
// <copyright file="HealthMonRewire.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.AutoWiring
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using Uhuru.CloudFoundry.DEA.Plugins;

    /// <summary>
    /// A concrete configurator for handling the ASP.NET health monitoring reconfiguration
    /// </summary>
    public class HealthMonRewire : INodeConfigRewireBase
    {
        /// <summary>
        /// The name of the external configuration file to be referenced
        /// </summary>
        private string healthMonExtSource = "UhuruAspNetEventProvider.config";

        /// <summary>
        /// The configuration manager instance to register with
        /// </summary>
        private ISiteConfigManager configmanager;

        /// <summary>
        /// Gets or sets the name of the config section.
        /// </summary>
        /// <value>
        /// The name of the config section.
        /// </value>
        public string ConfigSectionName
        {
            get { return "healthMonitoring"; }
            set { }
        }

        /// <summary>
        /// Gets or sets the config parent node in web.config.
        /// </summary>
        /// <value>
        /// The config parent.
        /// </value>
        public ParentSection ConfigParent
        {
            get { return ParentSection.SystemWeb; }
            set { }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this configurator references an external config file.
        /// </summary>
        /// <value>
        /// <c>true</c> if this configurator references an external source; otherwise, <c>false</c>.
        /// </value>
        public bool HasExternalSource
        {
            get { return true; }
            set { }
        }

        /// <summary>
        /// Gets or sets name of the external configuration file.
        /// </summary>
        /// <value>
        /// The external configuration file.
        /// </value>
        public string ExternalSource
        {
            get { return this.healthMonExtSource; }
            set { }
        }

        /// <summary>
        /// Registers with the specified site config manager.
        /// </summary>
        /// <param name="siteConfigManager">The site config manager.</param>
        public void Register(ISiteConfigManager siteConfigManager)
        {
            if (siteConfigManager == null)
            {
                throw new ArgumentNullException("siteConfigManager");
            }

            this.configmanager = siteConfigManager;
            siteConfigManager.RegisterSectionRewire(this);
        }

        /// <summary>
        /// Rewires the configuration section.
        /// </summary>
        /// <param name="configNode">The original configuration node, received from the site manager it registered with</param>
        /// <param name="createNewIfNotPresent">if set to <c>true</c> request a new node from the site manager, if none exists.</param>
        /// <returns>
        /// A node containing the new settings
        /// </returns>
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
                    throw new ArgumentException(Strings.CreationOfNewConfigSection);
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
                throw new NotImplementedException(Strings.LoggingAutoWiringOnly);
            }

            return tempConfig;
        }
    }
}