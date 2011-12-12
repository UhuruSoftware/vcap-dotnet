// -----------------------------------------------------------------------
// <copyright file="WebConfigRewiring.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.AutoWiring
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using Uhuru.CloudFoundry.DEA.Plugins.AspDotNetLogging;

    /// <summary>
    /// An enumeration specifying the parent node of a settings section that a configurator is responsible for
    /// </summary>
    public enum ParentSection
    {
        /// <summary>
        /// The system.web configurations section
        /// </summary>
        SystemWeb 
    }

    /// <summary>
    /// Interface that exposes functionality for reconfiguring various sections of a web.config file
    /// </summary>
    public interface ISiteConfigManager
    {
        /// <summary>
        /// Gets or sets a value indicating whether any individual reconfigurators are allowed to reference external config files.
        /// </summary>
        /// <value>
        ///   <c>true</c> if external references are allowed; otherwise, <c>false</c>.
        /// </value>
        bool AllowExternalSource
        {
            get;
            set;
        }

        /// <summary>
        /// Registers an individual section configurator and ads it to an internal collection
        /// </summary>
        /// <param name="nodeConfig">The node config.</param>
        void RegisterSectionRewire(INodeConfigRewireBase nodeConfig);

        /// <summary>
        /// Creates a new configuration section on behalf of an individual configurator
        /// </summary>
        /// <param name="nodeConfig">The configurator.</param>
        /// <returns></returns>
        IXPathNavigable CreateNewSection(INodeConfigRewireBase nodeConfig);

        /// <summary>
        /// Calls the rewire method on each registered configurator
        /// </summary>
        /// <param name="backupOriginal">if set to <c>true</c> each individual configurator backs up the original settings.</param>
        void Rewire(bool backupOriginal);

        /// <summary>
        /// Commits the changes.
        /// </summary>
        void CommitChanges();
    }

    /// <summary>
    /// Interface exposing reconfiguration logic for an individual section in a web.config file
    /// </summary>
    public interface INodeConfigRewireBase
    {
        /// <summary>
        /// Gets or sets the name of the config section.
        /// </summary>
        /// <value>
        /// The name of the config section.
        /// </value>
        string ConfigSectionName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the config parent node in web.config.
        /// </summary>
        /// <value>
        /// The config parent.
        /// </value>
        ParentSection ConfigParent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this configurator references an external config file.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this configurator references an external source; otherwise, <c>false</c>.
        /// </value>
        bool HasExternalSource
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets name of the external configuration file.
        /// </summary>
        /// <value>
        /// The external configuration file.
        /// </value>
        string ExternalSource
        {
            get;
            set;
        }

        /// <summary>
        /// Registers with the specified site config manager.
        /// </summary>
        /// <param name="siteConfigManager">The site config manager.</param>
        void Register(ISiteConfigManager siteConfigManager);

        /// <summary>
        /// Rewires the configuration section.
        /// </summary>
        /// <param name="configNode">The original configuration node, received from the site manager it registered with</param>
        /// <param name="createNewIfNotPresent">if set to <c>true</c> request a new node from the site manager, if none exists.</param>
        /// <returns> A node containing the new settings</returns>
        IXPathNavigable RewireConfig(IXPathNavigable configNode, bool createNewIfNotPresent);
    }

    /// <summary>
    /// 
    /// </summary>
    public class HealthMonRewire : INodeConfigRewireBase
    {
        private string healthMonExtSource = "UhuruAspNetEventProvider.config";
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
        /// 	<c>true</c> if this configurator references an external source; otherwise, <c>false</c>.
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
}