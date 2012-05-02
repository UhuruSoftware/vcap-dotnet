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
        /// Initial size of the secondary data file(s)
        /// </summary>
        private static ConfigurationProperty propertyInitialDataSize;

        /// <summary>
        /// Initial size of the log file(s)
        /// </summary>
        private static ConfigurationProperty propertyInitialLogSize;

        /// <summary>
        /// Maximum size of the data file(s)
        /// </summary>
        private static ConfigurationProperty propertyMaxDataSize;
        
        /// <summary>
        /// Maximum size of the log file(s)
        /// </summary>
        private static ConfigurationProperty propertyMaxLogSize;

        /// <summary>
        /// Size by which the data files are set to auto grow
        /// </summary>
        private static ConfigurationProperty propertyDataFileGrowth;

        /// <summary>
        /// Size by which the log files are set to auto grow
        /// </summary>
        private static ConfigurationProperty propertyLogFileGrowth;

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

            propertyInitialDataSize = new ConfigurationProperty(
                "initialDataSize",
                typeof(string),
                "100MB",
                ConfigurationPropertyOptions.None);

            propertyInitialLogSize = new ConfigurationProperty(
                "initialLogSize",
                typeof(string),
                "50MB",
                ConfigurationPropertyOptions.None);

            propertyMaxDataSize = new ConfigurationProperty(
                "maxDataSize",
                typeof(string),
                "1GB",
                ConfigurationPropertyOptions.None);

            propertyMaxLogSize = new ConfigurationProperty(
                "maxLogSize",
                typeof(string),
                "250MB",
                ConfigurationPropertyOptions.None);

            propertyDataFileGrowth = new ConfigurationProperty(
                "dataFileGrowth",
                typeof(string),
                "100MB",
                ConfigurationPropertyOptions.None);

            propertyLogFileGrowth = new ConfigurationProperty(
                "logFileGrowth",
                typeof(string),
                "25MB",
                ConfigurationPropertyOptions.None);
            
            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyHost);
            properties.Add(propertyUser);
            properties.Add(propertyPassword);
            properties.Add(propertyPort);
            properties.Add(propertyLogicalStorageUnits);

            properties.Add(propertyInitialDataSize);
            properties.Add(propertyInitialLogSize);

            properties.Add(propertyMaxDataSize);
            properties.Add(propertyMaxLogSize);

            properties.Add(propertyDataFileGrowth);
            properties.Add(propertyLogFileGrowth);
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
        /// Gets or sets the initial size of the secondary data file(s)
        /// </summary>
        [ConfigurationProperty("initialDataSize", IsRequired = false, DefaultValue = "100MB")]
        public string InitialDataSize
        {
            get
            {
                return (string)base[propertyInitialDataSize];
            }

            set 
			{ 
				base[propertyInitialDataSize] = value; 
			}
        }

        /// <summary>
        /// Gets or sets the initial size of the log file(s)
        /// </summary>
        [ConfigurationProperty("initialLogSize", IsRequired = false, DefaultValue = "50MB")]
        public string InitialLogSize
        {
            get
            {
                return (string)base[propertyInitialLogSize];
            }

            set 
			{ 
				base[propertyInitialLogSize] = value; 
			}
        }

        /// <summary>
        /// Gets or sets the maximum size of the data file(s)
        /// </summary>
        [ConfigurationProperty("maxDataSize", IsRequired = false, DefaultValue = "1GB")]
        public string MaxDataSize
        {
            get
            {
                return (string)base[propertyMaxDataSize];
            }

            set 
			{ 
				base[propertyMaxDataSize] = value; 
			}
        }

        /// <summary>
        /// Gets or sets the maximum size of the log file(s)
        /// </summary>
        [ConfigurationProperty("maxLogSize", IsRequired = false, DefaultValue = "1GB")]
        public string MaxLogSize
        {
            get
            {
                return (string)base[propertyMaxLogSize];
            }

            set 
			{ 
				base[propertyMaxLogSize] = value; 
			}
        }

        /// <summary>
        /// Gets or sets the size by which the data files are set to auto grow
        /// </summary>
        [ConfigurationProperty("dataFileGrowth", IsRequired = false, DefaultValue = "100MB")]
        public string DataFileGrowth
        {
            get
            {
                return (string)base[propertyDataFileGrowth];
            }

            set 
			{ 
				base[propertyDataFileGrowth] = value; 
			}
        }

        /// <summary>
        /// Gets or sets the size by which the log files are set to auto grow
        /// </summary>
        [ConfigurationProperty("logFileGrowth", IsRequired = false, DefaultValue = "25MB")]
        public string LogFileGrowth
        {
            get
            {
                return (string)base[propertyLogFileGrowth];
            }

            set 
			{ 
				base[propertyLogFileGrowth] = value; 
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
