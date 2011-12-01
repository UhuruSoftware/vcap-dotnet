using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using CloudFoundry.Net.Configuration.DEA;

namespace CloudFoundry.Net.Configuration
{
    public class UhuruSection : ConfigurationSection
    {


        static UhuruSection()
        {
            propertyDEA = new ConfigurationProperty(
                "dea",
                typeof(DEAElement),
                null,
                ConfigurationPropertyOptions.IsRequired);

            properties = new ConfigurationPropertyCollection();
            properties.Add(propertyDEA);
        }

        #region Static Fields
        private static ConfigurationProperty propertyDEA;

        private static ConfigurationPropertyCollection properties;
        #endregion


        #region Properties

        [ConfigurationProperty("dea")]
        public DEAElement DEA
        {
            get
            {
                return (DEAElement)base[propertyDEA];
            }
        }

        #endregion


        #region GetSection Pattern
        private static UhuruSection section;

        /// <summary>
        /// Gets the configuration section using the default element name.
        /// </summary>
        public static UhuruSection GetSection()
        {
            return GetSection("uhuru");
        }

        /// <summary>
        /// Gets the configuration section using the specified element name.
        /// </summary>
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
