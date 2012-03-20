// -----------------------------------------------------------------------
// <copyright file="DEAElement.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.DEA
{
    using System.Configuration;
    
    /// <summary>
    /// This is a configuration class that defines settings for the DEA component.
    /// </summary>
    public class DEAElement : ConfigurationElement
    {
        /// <summary>
        /// Base Directory configuration property.
        /// </summary>
        private static ConfigurationProperty propertyBaseDir;

        /// <summary>
        /// Local route configuration property.
        /// </summary>
        private static ConfigurationProperty propertyLocalRoute;

        /// <summary>
        /// Filer port configuration property.
        /// </summary>
        private static ConfigurationProperty propertyFilerPort;

        /// <summary>
        /// Status port configuration property.
        /// </summary>
        private static ConfigurationProperty propertyStatusPort;

        /// <summary>
        /// Heart beat interval configuration property.
        /// </summary>
        private static ConfigurationProperty propertyHeartBeatInterval;
        
        /// <summary>
        /// Message bus configuration property.
        /// </summary>
        private static ConfigurationProperty propertyMessageBus;
       
        /// <summary>
        /// Muti-tenant configuration property.
        /// </summary>
        private static ConfigurationProperty propertyMultiTenant;
       
        /// <summary>
        /// Maximum memory configuration property.
        /// </summary>
        private static ConfigurationProperty propertyMaxMemory;
      
        /// <summary>
        /// "Is secure" configuration property.
        /// </summary>
        private static ConfigurationProperty propertySecure;
      
        /// <summary>
        /// Enforec usage limit configuration property.
        /// </summary>
        private static ConfigurationProperty propertyEnforceUsageLimit;
      
        /// <summary>
        /// Disable directory cleanup configuration property.
        /// </summary>
        private static ConfigurationProperty propertyDisableDirCleanup;
      
        /// <summary>
        /// Force http sharing configuration property.
        /// </summary>
        private static ConfigurationProperty propertyForceHttpSharing;
      
        /// <summary>
        /// Runtimes configuration property.
        /// </summary>
        private static ConfigurationProperty propertyRuntimes;
        
        /// <summary>
        /// Configuration properties collection.
        /// </summary>
        private static ConfigurationPropertyCollection properties;

        /// <summary>
        /// Initializes static members of the DEAElement class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Code is cleaner this way.")]
        static DEAElement()
        {
            propertyBaseDir = new ConfigurationProperty(
                "baseDir",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            propertyLocalRoute = new ConfigurationProperty(
                "localRoute",
                typeof(string),
                null);

            propertyFilerPort = new ConfigurationProperty(
                "filerPort",
                typeof(int),
                12345,
                ConfigurationPropertyOptions.IsRequired);

            propertyStatusPort = new ConfigurationProperty(
                "statusPort",
                typeof(int),
                0);

            propertyHeartBeatInterval = new ConfigurationProperty(
                "heartBeatInterval",
                typeof(int),
                10000);

            propertyMessageBus = new ConfigurationProperty(
                "messageBus",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            propertyMultiTenant = new ConfigurationProperty(
                "multiTenant",
                typeof(bool),
                true);

            propertyMaxMemory = new ConfigurationProperty(
                "maxMemory",
                typeof(int),
                2048,
                ConfigurationPropertyOptions.IsRequired);

            propertySecure = new ConfigurationProperty(
                "secure",
                typeof(bool),
                true);

            propertyEnforceUsageLimit = new ConfigurationProperty(
                "enforceUlimit",
                typeof(bool),
                true);

            propertyDisableDirCleanup = new ConfigurationProperty(
                "disableDirCleanup",
                typeof(bool),
                false);

            propertyForceHttpSharing = new ConfigurationProperty(
                "forceHttpSharing",
                typeof(bool),
                false);

            propertyRuntimes = new ConfigurationProperty(
             "runtimes",
             typeof(RuntimeCollection),
             new RuntimeCollection());

            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyBaseDir);
            properties.Add(propertyLocalRoute);
            properties.Add(propertyFilerPort);
            properties.Add(propertyStatusPort);
            properties.Add(propertyHeartBeatInterval);
            properties.Add(propertyDisableDirCleanup);
            properties.Add(propertyMessageBus);
            properties.Add(propertyMultiTenant);
            properties.Add(propertyMaxMemory);
            properties.Add(propertySecure);
            properties.Add(propertyEnforceUsageLimit);
            properties.Add(propertyForceHttpSharing);
            properties.Add(propertyRuntimes);
        }

        /// <summary>
        /// Gets or sets the base directory where all applications are staged and hosted
        /// </summary>
        [ConfigurationProperty("baseDir", IsRequired = true, DefaultValue = null)]
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
        /// Gets or sets the local route.
        /// The local route is the IP address of a well known server on your network,
        /// used to choose the right ip address (think of hosts that have multiple nicks
        /// and IP addresses assigned to them) of the host running the DEA. A default
        /// value of null should work in most cases.
        /// </summary>
        [ConfigurationProperty("localRoute", IsRequired = false, DefaultValue = null)]
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
        /// Gets or sets the port for accessing the files of running applications.
        /// </summary>
        [ConfigurationProperty("filerPort", IsRequired = true, DefaultValue = 12345)]
        public int FilerPort
        {
            get
            {
                return (int)base[propertyFilerPort];
            }

            set
            {
                base[propertyFilerPort] = value;
            }
        }

        /// <summary>
        /// Gets or sets the port for accessing /healthz and /varz info.
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
        /// Gets or sets the time interval to send heartbeat messages to the message bus, in milliseconds.
        /// </summary>
        [ConfigurationProperty("heartBeatInterval", IsRequired = false, DefaultValue = 10000)]
        public int HeartbeatInterval
        {
            get
            {
                return (int)base[propertyHeartBeatInterval];
            }

            set
            {
                base[propertyHeartBeatInterval] = value;
            }
        }
         
        /// <summary>
        /// Gets or sets the NATS message bus URI.
        /// </summary>
        [ConfigurationProperty("messageBus", IsRequired = true, DefaultValue = null)]
        public string MessageBus
        {
            get
            {
                return (string)base[propertyMessageBus];
            }

            set
            {
                base[propertyMessageBus] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether more than one application is allowed to run per DEA.
        /// </summary>
        [ConfigurationProperty("multiTenant", IsRequired = false, DefaultValue = true)]
        public bool Multitenant
        {
            get
            {
                return (bool)base[propertyMultiTenant];
            }

            set
            {
                base[propertyMultiTenant] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount of memory allocated to this DEA. In a multi tenant setup, this
        /// memory is divided amongst all applications managed by this DEA.
        /// </summary>
        [ConfigurationProperty("maxMemory", IsRequired = true, DefaultValue = 2048)]
        public int MaxMemory
        {
            get
            {
                return (int)base[propertyMaxMemory];
            }

            set
            {
                base[propertyMaxMemory] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the environment for running applications in a multi tenant setup is secure or not.
        /// </summary>
        [ConfigurationProperty("secure", IsRequired = false, DefaultValue = true)]
        public bool Secure
        {
            get
            {
                return (bool)base[propertySecure];
            }

            set
            {
                base[propertySecure] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ulimit-based resource isolation in a multi tenant setup is provided or not.
        /// </summary>
        [ConfigurationProperty("enforceUlimit", IsRequired = false, DefaultValue = true)]
        public bool EnforceUsageLimit
        {
            get
            {
                return (bool)base[propertyEnforceUsageLimit];
            }

            set
            {
                base[propertyEnforceUsageLimit] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether disabling the cleanup of droplet instances after stopping them is activated or not.
        /// </summary>
        [ConfigurationProperty("disableDirCleanup", IsRequired = false, DefaultValue = false)]
        public bool DisableDirCleanup
        {
            get
            {
                return (bool)base[propertyDisableDirCleanup];
            }

            set
            {
                base[propertyDisableDirCleanup] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the droplets are to be downloaded over http even when
        /// there is a shared directory containing the droplet or not.
        /// </summary>
        [ConfigurationProperty("forceHttpSharing", IsRequired = false, DefaultValue = false)]
        public bool ForceHttpSharing
        {
            get
            {
                return (bool)base[propertyForceHttpSharing];
            }

            set
            {
                base[propertyForceHttpSharing] = value;
            }
        }

        /// <summary>
        /// Gets the runtimes configuration for the dea.
        /// </summary>
        [ConfigurationProperty("runtimes", IsRequired = false, DefaultValue = null)]
        public RuntimeCollection Runtimes
        {
            get
            {
                return (RuntimeCollection)base[propertyRuntimes];
            }
        }

        /// <summary>
        /// Overrides the Properties collection and returns our custom one.
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