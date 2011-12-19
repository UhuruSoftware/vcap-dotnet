// -----------------------------------------------------------------------
// <copyright file="INodeConfigRewireBase.cs" company="Uhuru Software, Inc.">
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
        /// <c>true</c> if this configurator references an external source; otherwise, <c>false</c>.
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
}