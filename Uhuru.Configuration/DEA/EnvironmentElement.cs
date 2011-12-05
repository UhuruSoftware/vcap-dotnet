using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Uhuru.Configuration.DEA
{
    /// <summary>
    /// This configuration class contains environment variable settings for a runtime.
    /// </summary>
    public class EnvironmentElement : ConfigurationElement
    {
        #region Constructors

        static EnvironmentElement()
        {
            propertyName = new ConfigurationProperty(
                "name",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyValue = new ConfigurationProperty(
                "value",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyName);
            properties.Add(propertyValue);
        }
        
        #endregion

        #region Static Fields

        private static ConfigurationProperty propertyName;
        private static ConfigurationProperty propertyValue;

        private static ConfigurationPropertyCollection properties;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of a Environment variable.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, DefaultValue = null)]
        public string Name
        {
            get
            {
                return (string)base[propertyName];
            }
        }

        /// <summary>
        /// Gets the value of a Environment variable.
        /// </summary>
        [ConfigurationProperty("value", IsRequired = true, DefaultValue = null)]
        public string Value
        {
            get
            {
                return (string)base[propertyValue];
            }
        }

        /// <summary>
        /// Override the Properties collection and return our custom one.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        #endregion
    }
}
