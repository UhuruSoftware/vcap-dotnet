// -----------------------------------------------------------------------
// <copyright file="ServiceElement.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.Service
{
    using System.Configuration;
    
    /// <summary>
    /// This configuration class contains settings for a service component.
    /// </summary>
    public class ServiceElement : ConfigurationElement
    {
        #region Static Fields

        /// <summary>
        /// Node ID configuration property.
        /// </summary>
        private static ConfigurationProperty propertyNodeId;

        /// <summary>
        /// Node service plan.
        /// </summary>
        private static ConfigurationProperty propertyPlan;

        /// <summary>
        /// Migration NFS configuration property.
        /// </summary>
        private static ConfigurationProperty propertyMigrationNfs;
       
        /// <summary>
        /// Message bus configuration property.
        /// </summary>
        private static ConfigurationProperty propertyMBus;
       
        /// <summary>
        /// Index configuration property.
        /// </summary>
        private static ConfigurationProperty propertyIndex;

        /// <summary>
        /// Status port configuration property.
        /// </summary>
        private static ConfigurationProperty propertyStatusPort;

        /// <summary>
        /// Z-Interval configuration property.
        /// </summary>
        private static ConfigurationProperty propertyZInterval;
       
        /// <summary>
        /// Maximum DB size configuration property.
        /// </summary>
        private static ConfigurationProperty propertyMaxDbSize;
       
        /// <summary>
        /// Maximum long query configuration property.
        /// </summary>
        private static ConfigurationProperty propertyMaxLongQuery;
        
        /// <summary>
        /// Maximum long transaction configuration property.
        /// </summary>
        private static ConfigurationProperty propertyMaxLongTx;
        
        /// <summary>
        /// Local database configuration property.
        /// </summary>
        private static ConfigurationProperty propertyLocalDb;
        
        /// <summary>
        /// Base directory configuration property.
        /// </summary>
        private static ConfigurationProperty propertyBaseDir;
       
        /// <summary>
        /// Local route configuration property.
        /// </summary>
        private static ConfigurationProperty propertyLocalRoute;
      
        /// <summary>
        /// Available storage configuration property.
        /// </summary>
        private static ConfigurationProperty propertyAvailableStorage;

        /// <summary>
        /// Available capacity configuration property.
        /// </summary>
        private static ConfigurationProperty propertyCapacity;

        /// <summary>
        /// MS SQL Service configuration property.
        /// </summary>
        private static ConfigurationProperty propertyMsSql;
       
        /// <summary>
        /// Configuration properties collection.
        /// </summary>
        private static ConfigurationPropertyCollection properties;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes static members of the ServiceElement class. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Code is cleaner this way")]
        static ServiceElement()
        {
            propertyNodeId = new ConfigurationProperty(
                "nodeId",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            propertyPlan = new ConfigurationProperty(
                "plan",
                typeof(string),
                "free");

            propertyMigrationNfs = new ConfigurationProperty(
                "migrationNfs",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            propertyMBus = new ConfigurationProperty(
                "mbus",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            propertyIndex = new ConfigurationProperty(
                "index",
                typeof(int),
                0,
                ConfigurationPropertyOptions.IsRequired);

            propertyStatusPort = new ConfigurationProperty(
                "statusPort",
                typeof(int),
                0);

            propertyZInterval = new ConfigurationProperty(
                "zInterval",
                typeof(int),
                30000,
                ConfigurationPropertyOptions.IsRequired);

            propertyMaxDbSize = new ConfigurationProperty(
                "maxDbSize",
                typeof(long),
                20L,
                ConfigurationPropertyOptions.IsRequired);

            propertyMaxLongQuery = new ConfigurationProperty(
                "maxLongQuery",
                typeof(int),
                3,
                ConfigurationPropertyOptions.IsRequired);

            propertyMaxLongTx = new ConfigurationProperty(
                "maxLongTx",
                typeof(int),
                30,
                ConfigurationPropertyOptions.IsRequired);

            propertyLocalDb = new ConfigurationProperty(
                "localDb",
                typeof(string),
                "localServiceDb.xml",
                ConfigurationPropertyOptions.IsRequired);

            propertyBaseDir = new ConfigurationProperty(
                "baseDir",
                typeof(string),
                ".\\",
                ConfigurationPropertyOptions.IsRequired);

            propertyLocalRoute = new ConfigurationProperty(
                "localRoute",
                typeof(string),
                "198.41.0.4",
                ConfigurationPropertyOptions.IsRequired);

            propertyAvailableStorage = new ConfigurationProperty(
                "availableStorage",
                typeof(long),
                1024L,
                ConfigurationPropertyOptions.IsRequired);

            propertyCapacity = new ConfigurationProperty(
                "capacity",
                typeof(int),
                200,
                ConfigurationPropertyOptions.IsRequired);

            propertyMsSql = new ConfigurationProperty(
              "mssql",
              typeof(MSSqlElement),
              null,
              ConfigurationPropertyOptions.None);

            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyNodeId);
            properties.Add(propertyPlan);
            properties.Add(propertyMigrationNfs);
            properties.Add(propertyMBus);
            properties.Add(propertyIndex);
            properties.Add(propertyStatusPort);
            properties.Add(propertyZInterval);
            properties.Add(propertyMaxDbSize);
            properties.Add(propertyMaxLongQuery);
            properties.Add(propertyMaxLongTx);
            properties.Add(propertyLocalDb);
            properties.Add(propertyBaseDir);
            properties.Add(propertyLocalRoute);
            properties.Add(propertyAvailableStorage);
            properties.Add(propertyCapacity);
            properties.Add(propertyMsSql);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the node id for the service.
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
        /// Gets or sets the plan for the service.
        /// </summary>
        [ConfigurationProperty("plan", IsRequired = false)]
        public string Plan
        {
            get
            {
                return (string)base[propertyPlan];
            }

            set
            {
                base[propertyPlan] = value;
            }
        }

        /// <summary>
        /// Gets or sets the network file system used for migration.
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
        /// Gets or sets the NATS message bus URI
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
        /// Gets or sets the index of the service node.
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
        /// Gets or sets the status port for /health and /varz
        /// </summary>
        [ConfigurationProperty("statusPort", IsRequired = false, DefaultValue = 0)]
        public int StatusPort
        {
            get
            {
                return (int)base[propertyStatusPort];
            }

            set
            {
                base[propertyStatusPort] = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval at which to update the Healthz and Varz
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
        /// Gets or sets the maximum database size.
        /// </summary>
        [ConfigurationProperty("maxDbSize", IsRequired = true, DefaultValue = 20L)]
        public long MaxDBSize
        {
            get
            {
                return (long)base[propertyMaxDbSize];
            }

            set
            {
                base[propertyMaxDbSize] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum duration for a query in seconds.
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
        /// Gets or sets the maximum duration for a query in seconds.
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
        /// Gets or sets the base directory for the service.
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
        /// Gets or sets the amount of available storage for this service, in megabytes.
        /// </summary>
        [ConfigurationProperty("availableStorage", IsRequired = true, DefaultValue = 1024L)]
        public long AvailableStorage
        {
            get
            {
                return (long)base[propertyAvailableStorage];
            }

            set
            {
                base[propertyAvailableStorage] = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of available capacity for this service.
        /// </summary>
        [ConfigurationProperty("capacity", IsRequired = true, DefaultValue = 200)]
        public int Capacity
        {
            get
            {
                return (int)base[propertyCapacity];
            }

            set
            {
                base[propertyCapacity] = value;
            }
        }

        /// <summary>
        /// Gets or sets the local database file in which to save the list of provisioned services.
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
        /// Gets or sets the IP address of a well known server on your network; it
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
        /// Gets or sets the configuration settings for an MS Sql Server system service.
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

        /// <summary>
        /// Override the Properties collection and return our custom one.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
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