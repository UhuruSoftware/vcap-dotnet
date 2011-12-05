using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Uhuru.Configuration.Service
{
    /// <summary>
    /// This configuration class contains settings for a service component.
    /// </summary>
    public class ServiceElement : ConfigurationElement
    {
        #region Constructors

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
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
              typeof(MSSqlElement),
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

        /// <summary>
        /// Node id for the service.
        /// </summary>
        [ConfigurationProperty("nodeId", IsRequired = true, DefaultValue = null)]
        public string NodeId
        {
            get
            {
                return (string)base[propertyNodeId];
            }
            set
            {
                base[propertyNodeId] = value;
            }
        }

        /// <summary>
        /// Network file system used for migration.
        /// </summary>
        [ConfigurationProperty("migrationNfs", IsRequired = true, DefaultValue = null)]
        public string MigrationNFS
        {
            get
            {
                return (string)base[propertyMigrationNfs];
            }
            set
            {
                base[propertyMigrationNfs] = value;
            }
        }

        /// <summary>
        /// NATS message bus URI
        /// </summary>
        [ConfigurationProperty("mbus", IsRequired = true, DefaultValue = null)]
        public string MBus
        {
            get
            {
                return (string)base[propertyMBus];
            }
            set
            {
                base[propertyMBus] = value;
            }
        }

        /// <summary>
        /// Index of the service node.
        /// </summary>
        [ConfigurationProperty("index", IsRequired = true, DefaultValue = 0)]
        public int Index
        {
            get
            {
                return (int)base[propertyIndex];
            }
            set
            {
                base[propertyIndex] = value;
            }
        }

        /// <summary>
        /// Gets the interval at which to update the Healthz and Varz
        /// </summary>
        [ConfigurationProperty("zInterval", IsRequired = true, DefaultValue = 30000)]
        public int ZInterval
        {
            get
            {
                return (int)base[propertyZInterval];
            }
            set
            {
                base[propertyZInterval] = value;
            }
        }

        /// <summary>
        /// Gets the maximum database size.
        /// </summary>
        [ConfigurationProperty("maxDbSize", IsRequired = true, DefaultValue = 20)]
        public int MaxDBSize
        {
            get
            {
                return (int)base[propertyMaxDbSize];
            }
            set
            {
                base[propertyMaxDbSize] = value;
            }
        }

        /// <summary>
        /// Gets the maximum duration for a query in seconds.
        /// </summary>
        [ConfigurationProperty("maxLongQuery", IsRequired = true, DefaultValue = 3)]
        public int MaxLengthyQuery
        {
            get
            {
                return (int)base[propertyMaxLongQuery];
            }
            set
            {
                base[propertyMaxLongQuery] = value;
            }
        }

        /// <summary>
        /// Gets the maximum duration for a query in seconds.
        /// </summary>
        [ConfigurationProperty("maxLongTx", IsRequired = true, DefaultValue = 30)]
        public int MaxLengthyTX
        {
            get
            {
                return (int)base[propertyMaxLongTx];
            }
            set
            {
                base[propertyMaxLongTx] = value;
            }
        }

        /// <summary>
        /// Gets the base directory for the service.
        /// </summary>
        [ConfigurationProperty("baseDir", IsRequired = true, DefaultValue = ".\\")]
        public string BaseDir
        {
            get
            {
                return (string)base[propertyBaseDir];
            }
            set
            {
                base[propertyBaseDir] = value;
            }
        }

        /// <summary>
        /// Gets the amount of available storage for this service, in megabytes.
        /// </summary>
        [ConfigurationProperty("availableStorage", IsRequired = true, DefaultValue = 1024)]
        public int AvailableStorage
        {
            get
            {
                return (int)base[propertyAvailableStorage];
            }
            set
            {
                base[propertyAvailableStorage] = value;
            }
        }

        /// <summary>
        /// Gets the local database file in which to save the list of provisioned services.
        /// </summary>
        [ConfigurationProperty("localDb", IsRequired = true, DefaultValue = "localServiceDb.xml")]
        public string LocalDB
        {
            get
            {
                return (string)base[propertyLocalDb];
            }
            set
            {
                base[propertyLocalDb] = value;
            }
        }

        /// <summary>
        /// This is the IP address of a well known server on your network, it
        /// is used to choose the right ip address (think of hosts that have multiple nics
        /// and IP addresses assigned to them) of the host running the DEA. Default
        /// value of null, should work in most cases.
        /// </summary>
        [ConfigurationProperty("localRoute", IsRequired = true, DefaultValue = "198.41.0.4")]
        public string LocalRoute
        {
            get
            {
                return (string)base[propertyLocalRoute];
            }
            set
            {
                base[propertyLocalRoute] = value;
            }
        }

        /// <summary>
        /// Gets configuration settings for an MS Sql Server system service.
        /// </summary>
        [ConfigurationProperty("mssql", IsRequired = false, DefaultValue = null)]
        public MSSqlElement MSSql
        {
            get
            {
                return (MSSqlElement)base[propertyMsSql];
            }
            set
            {
                base[propertyMsSql] = value;
            }
        }

        #endregion

        #region Overrides

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

        #endregion
    }
}