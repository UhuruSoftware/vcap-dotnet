// -----------------------------------------------------------------------
// <copyright file="UhuruSection.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration
{
    using System.Configuration;
    using Uhuru.Configuration.DEA;
    using Uhuru.Configuration.Service;
    
    /// <summary>
    /// This class defines an Uhuru section in an application configuration file.
    /// </summary>
    public class UhuruSection : ConfigurationSection
    {
        /// <summary>
        /// Initializes the static members of the UhuruSection class
        /// </summary>
        static UhuruSection()
        {
            propertyDEA = new ConfigurationProperty(
                "dea",
                typeof(DEAElement),
                null);

            propertyService = new ConfigurationProperty(
                "service",
                typeof(ServiceElement),
                null);

            properties = new ConfigurationPropertyCollection();
            properties.Add(propertyDEA);
            properties.Add(propertyService);
        }

        #region Static Fields
        
        private static ConfigurationProperty propertyDEA;
        private static ConfigurationProperty propertyService;
        private static ConfigurationPropertyCollection properties;
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the configuration settings for a DEA.
        /// </summary>
        [ConfigurationProperty("dea")]
        public DEAElement DEA
        {
            get
            {
                return (DEAElement)base[propertyDEA];
            }

            set
            {
                base[propertyDEA] = value;
            }
        }

        /// <summary>
        /// Gets or sets the configuration settings for a System Service
        /// </summary>
        [ConfigurationProperty("service")]
        public ServiceElement Service
        {
            get
            {
                return (ServiceElement)base[propertyService];
            }

            set
            {
                base[propertyService] = value;
            }
        }

        #endregion

        #region GetSection Pattern
        private static UhuruSection section;

        /// <summary>
        /// Gets the configuration section using the default element name.
        /// </summary>
        /// <returns>the default configuration section</returns>
        public static UhuruSection GetSection()
        {
            return GetSection("uhuru");
        }

        /// <summary>
        /// Gets the configuration section using the specified element name.
        /// </summary>
        /// <returns>the configuration section requested</returns>    
        public static UhuruSection GetSection(string definedName)
        {
            if (section == null)
            {
                section = ConfigurationManager.GetSection(definedName) as UhuruSection;
                if (section == null)
                {
                    throw new ConfigurationErrorsException("The <" + definedName + "> section is not defined in your .config file!");
                }
            }

            return section;
        }

        #endregion
    }
}
