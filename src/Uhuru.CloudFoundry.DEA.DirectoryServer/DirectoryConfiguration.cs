// -----------------------------------------------------------------------
// <copyright file="DirectoryConfiguration.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.DirectoryServer
{
    using System.Configuration;
    using Uhuru.Configuration;

    /// <summary>
    /// Helper class for getting the DEA's configuration element from the config file.
    /// </summary>
    public static class DirectoryConfiguration
    {
        /// <summary>
        /// Gets the DEA config element.
        /// </summary>
        /// <returns>A DEAElement that contains all DEA configuration settings, including the Directory Server.</returns>
        public static DEAElement ReadConfig()
        {
            UhuruSection uhuruSection = (UhuruSection)ConfigurationManager.GetSection("uhuru");
            return uhuruSection.DEA;
        }
    }
}