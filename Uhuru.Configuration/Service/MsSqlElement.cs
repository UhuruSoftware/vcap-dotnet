using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Uhuru.Configuration.Service
{
    /// <summary>
    /// This configuration class defines settings for the MS Sql Server Node component.
    /// </summary>
    public class MSSqlElement : ConfigurationElement
    {
        #region Constructors

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static MSSqlElement()
        {
            propertyHost = new ConfigurationProperty(
                "host",
                typeof(string),
                "(local)",
                ConfigurationPropertyOptions.IsRequired);

            propertyUser = new ConfigurationProperty(
                "user",
                typeof(string),
                "sa",
              ConfigurationPropertyOptions.IsRequired);

            propertyPassword = new ConfigurationProperty(
                "password",
                typeof(string),
                "sa",
                ConfigurationPropertyOptions.IsRequired);

            propertyPort = new ConfigurationProperty(
                "port",
                typeof(int),
                1433,
                ConfigurationPropertyOptions.IsRequired);
            
            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyHost);
            properties.Add(propertyUser);
            properties.Add(propertyPassword);
            properties.Add(propertyPort);
        }

        #endregion

        #region Static Fields

        private static ConfigurationProperty propertyHost;
        private static ConfigurationProperty propertyUser;
        private static ConfigurationProperty propertyPassword;
        private static ConfigurationProperty propertyPort;

        private static ConfigurationPropertyCollection properties;
        #endregion

        #region Properties

        /// <summary>
        /// Override the Properties collection and return our custom one.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Gets the host of the target SQL Server.
        /// </summary>
        [ConfigurationProperty("host", IsRequired = true, DefaultValue = "(local)")]
        public string Host
        {
            get
            {
                return (string)base[propertyHost];
            }
        }

        /// <summary>
        /// Gets the user for connecting to the target SQL Server.
        /// </summary>
        [ConfigurationProperty("user", IsRequired = true, DefaultValue = "sa")]
        public string User
        {
            get
            {
                return (string)base[propertyUser];
            }
        }

        /// <summary>
        /// Gets the password for connecting to the target SQL Server.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true, DefaultValue = "sa")]
        public string Password
        {
            get
            {
                return (string)base[propertyPassword];
            }
        }

        /// <summary>
        /// Gets the port for connecting to the target SQL Server.
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true, DefaultValue = 1433)]
        public int Port
        {
            get
            {
                return (int)base[propertyPort];
            }
        }

        #endregion
    }
}
