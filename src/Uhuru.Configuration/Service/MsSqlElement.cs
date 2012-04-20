// -----------------------------------------------------------------------
// <copyright file="MsSqlElement.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.Service
{
    using System.Configuration;
    
    /// <summary>
    /// This configuration class defines settings for the MS Sql Server Node component.
    /// </summary>
    public class MSSqlElement : ConfigurationElement
    {
        /// <summary>
        /// Host configuration property.
        /// </summary>
        private static ConfigurationProperty propertyHost;

        /// <summary>
        /// User configuration property.
        /// </summary>
        private static ConfigurationProperty propertyUser;

        /// <summary>
        /// Password configuration property.
        /// </summary>
        private static ConfigurationProperty propertyPassword;

        /// <summary>
        /// Port configuration property.
        /// </summary>
        private static ConfigurationProperty propertyPort;

        /// <summary>
        /// List of drives on which the sql storage files will be distributed (eg: C,D,E,F)
        /// </summary>
        private static ConfigurationProperty propertyLogicalStorageUnits;

        /// <summary>
        /// Configuration properties collection.
        /// </summary>
        private static ConfigurationPropertyCollection properties;

        /// <summary>
        /// Initializes static members of the <see cref="MSSqlElement"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Code is cleaner this way")]
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

            propertyLogicalStorageUnits = new ConfigurationProperty(
                "logicalStorageUnits",
                typeof(string),
                "C",
                ConfigurationPropertyOptions.None);
            
            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyHost);
            properties.Add(propertyUser);
            properties.Add(propertyPassword);
            properties.Add(propertyPort);
            properties.Add(propertyLogicalStorageUnits);
        }

        /// <summary>
        /// Gets or sets the host of the target SQL Server.
        /// </summary>
        [ConfigurationProperty("host", IsRequired = true, DefaultValue = "(local)")]
        public string Host
        {
            get
            {
                return (string)base[propertyHost];
            }

            set
            {
                base[propertyHost] = value;
            }
        }

        /// <summary>
        /// Gets or sets the user for connecting to the target SQL Server.
        /// </summary>
        [ConfigurationProperty("user", IsRequired = true, DefaultValue = "sa")]
        public string User
        {
            get
            {
                return (string)base[propertyUser];
            }

            set
            {
                base[propertyUser] = value;
            }
        }

        /// <summary>
        /// Gets or sets the password for connecting to the target SQL Server.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true, DefaultValue = "sa")]
        public string Password
        {
            get
            {
                return (string)base[propertyPassword];
            }

            set
            {
                base[propertyPassword] = value;
            }
        }

        /// <summary>
        /// Gets or sets the port for connecting to the target SQL Server.
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true, DefaultValue = 1433)]
        public int Port
        {
            get
            {
                return (int)base[propertyPort];
            }

            set
            {
                base[propertyPort] = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of drives on which the database storage files will be distributed on
        /// </summary>
        [ConfigurationProperty("logicalStorageUnits", IsRequired = false, DefaultValue = "C")]
        public string LogicalStorageUnits
        {
            get
            {
                return (string)base[propertyLogicalStorageUnits];
            }

            set
            {
                base[propertyLogicalStorageUnits] = value;
            }
        }

        /// <summary>
        /// Override the Properties collection and return our custom one.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
