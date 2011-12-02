using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Uhuru.Configuration.Service
{
    public class MsSqlElement : ConfigurationElement
    {
        #region Constructors

        static MsSqlElement()
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

        [ConfigurationProperty("host", IsRequired = true, DefaultValue = "(local)")]
        public string Host
        {
            get
            {
                return (string)base[propertyHost];
            }
        }

        [ConfigurationProperty("user", IsRequired = true, DefaultValue = "sa")]
        public string User
        {
            get
            {
                return (string)base[propertyUser];
            }
        }

        [ConfigurationProperty("password", IsRequired = true, DefaultValue = "sa")]
        public string Password
        {
            get
            {
                return (string)base[propertyPassword];
            }
        }

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
