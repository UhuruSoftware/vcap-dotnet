using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Uhuru.Configuration.Service
{
    public class ServiceElement : ConfigurationElement
    {
        #region Constructors

        static ServiceElement()
        {
            propertyNodeId = new ConfigurationProperty(
                "nodeId",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyMigrationNfs = new ConfigurationProperty(
                "migrationNfs",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyMBus = new ConfigurationProperty(
                "mbus",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyIndex = new ConfigurationProperty(
                "index",
                typeof(int),
                0,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyZInterval = new ConfigurationProperty(
                "zInterval",
                typeof(int),
                30000,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyMaxDbSize = new ConfigurationProperty(
                "maxDbSize",
                typeof(int),
                20,
                ConfigurationPropertyOptions.IsRequired
           );

            propertyMaxLongQuery = new ConfigurationProperty(
                "maxLongQuery",
                typeof(int),
                3,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyMaxLongTx = new ConfigurationProperty(
                "maxLongTx",
                typeof(int),
                30,
                ConfigurationPropertyOptions.IsRequired
           );

            propertyLocalDb = new ConfigurationProperty(
                "localDb",
                typeof(string),
                "localServiceDb.xml",
                ConfigurationPropertyOptions.IsRequired
            );

            propertyBaseDir = new ConfigurationProperty(
                "baseDir",
                typeof(string),
                ".\\",
                ConfigurationPropertyOptions.IsRequired
                );

            propertyLocalRoute = new ConfigurationProperty(
                "localRoute",
                typeof(string),
                "198.41.0.4",
                ConfigurationPropertyOptions.IsRequired
                );

            propertyAvailableStorage = new ConfigurationProperty(
                "availableStorage",
                typeof(int),
                1024,
                ConfigurationPropertyOptions.IsRequired
                );

            propertyMsSql = new ConfigurationProperty(
              "mssql",
              typeof(MsSqlElement),
              null,
              ConfigurationPropertyOptions.None
              );

            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyNodeId);
            properties.Add(propertyMigrationNfs);
            properties.Add(propertyMBus);
            properties.Add(propertyIndex);
            properties.Add(propertyZInterval);
            properties.Add(propertyMaxDbSize);
            properties.Add(propertyMaxLongQuery);
            properties.Add(propertyMaxLongTx);
            properties.Add(propertyLocalDb);
            properties.Add(propertyBaseDir);
            properties.Add(propertyLocalRoute);
            properties.Add(propertyAvailableStorage);
            properties.Add(propertyMsSql);
        }

        #endregion

        #region Static Fields

        private static ConfigurationProperty propertyNodeId;
        private static ConfigurationProperty propertyMigrationNfs;
        private static ConfigurationProperty propertyMBus;
        private static ConfigurationProperty propertyIndex;
        private static ConfigurationProperty propertyZInterval;
        private static ConfigurationProperty propertyMaxDbSize;
        private static ConfigurationProperty propertyMaxLongQuery;
        private static ConfigurationProperty propertyMaxLongTx;
        private static ConfigurationProperty propertyLocalDb;
        private static ConfigurationProperty propertyBaseDir;
        private static ConfigurationProperty propertyLocalRoute;
        private static ConfigurationProperty propertyAvailableStorage;
        private static ConfigurationProperty propertyMsSql;


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

        [ConfigurationProperty("nodeId", IsRequired = true, DefaultValue = null)]
        public string NodeId
        {
            get
            {
                return (string)base[propertyNodeId];
            }
        }

        [ConfigurationProperty("migrationNfs", IsRequired = true, DefaultValue = null)]
        public string MigrationNfs
        {
            get
            {
                return (string)base[propertyMigrationNfs];
            }
        }

        [ConfigurationProperty("mbus", IsRequired = true, DefaultValue = null)]
        public string MBus
        {
            get
            {
                return (string)base[propertyMBus];
            }
        }

        [ConfigurationProperty("index", IsRequired = true, DefaultValue = 0)]
        public int Index
        {
            get
            {
                return (int)base[propertyIndex];
            }
        }

        [ConfigurationProperty("zInterval", IsRequired = true, DefaultValue = 30000)]
        public int ZInterval
        {
            get
            {
                return (int)base[propertyZInterval];
            }
        }

        [ConfigurationProperty("maxDbSize", IsRequired = true, DefaultValue = 20)]
        public int MaxDbSize
        {
            get
            {
                return (int)base[propertyMaxDbSize];
            }
        }

        [ConfigurationProperty("maxLongQuery", IsRequired = true, DefaultValue = 3)]
        public int MaxLongQuery
        {
            get
            {
                return (int)base[propertyMaxLongQuery];
            }
        }

        [ConfigurationProperty("maxLongTx", IsRequired = true, DefaultValue = 30)]
        public int MaxLongTx
        {
            get
            {
                return (int)base[propertyMaxLongTx];
            }
        }

        [ConfigurationProperty("baseDir", IsRequired = true, DefaultValue = ".\\")]
        public string BaseDir
        {
            get
            {
                return (string)base[propertyBaseDir];
            }
        }

        [ConfigurationProperty("availableStorage", IsRequired = true, DefaultValue = 1024)]
        public int AvailableStorage
        {
            get
            {
                return (int)base[propertyAvailableStorage];
            }
        }

        [ConfigurationProperty("localDb", IsRequired = true, DefaultValue = "localServiceDb.xml")]
        public string LocalDb
        {
            get
            {
                return (string)base[propertyLocalDb];
            }
        }

        [ConfigurationProperty("localRoute", IsRequired = true, DefaultValue = "198.41.0.4")]
        public string LocalRoute
        {
            get
            {
                return (string)base[propertyLocalRoute];
            }
        }

        [ConfigurationProperty("mssql", IsRequired = false, DefaultValue = null)]
        public MsSqlElement MsSql
        {
            get
            {
                return (MsSqlElement)base[propertyMsSql];
            }
        }

        #endregion
    }
}