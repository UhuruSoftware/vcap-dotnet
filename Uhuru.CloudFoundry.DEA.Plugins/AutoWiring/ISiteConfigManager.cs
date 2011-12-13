// -----------------------------------------------------------------------
// <copyright file="ISiteConfigManager.cs" company="Uhuru Software, Inc.">
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
        /// <returns>Returns a new configuration section node to the configurator</returns>
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
}