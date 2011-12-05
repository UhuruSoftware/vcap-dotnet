using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Uhuru.Configuration.DEA
{
    /// <summary>
    /// This is a configuration class that defines settings for the DEA component.
    /// </summary>
    public class DEAElement : ConfigurationElement
    {
        #region Constructors
        
        static DEAElement()
        {
            propertyBaseDir = new ConfigurationProperty(
                "baseDir",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyLocalRoute = new ConfigurationProperty(
                "localRoute",
                typeof(string),
                null
            );

            propertyFilerPort = new ConfigurationProperty(
                "filerPort",
                typeof(int),
                12345,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyHeartBeatInterval = new ConfigurationProperty(
                "heartBeatInterval",
                typeof(int),
                10000
            );

            propertyMessageBus = new ConfigurationProperty(
                "messageBus",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyMultiTenant = new ConfigurationProperty(
                "multiTenant",
                typeof(bool),
                true
            );

            propertyMaxMemory = new ConfigurationProperty(
                "maxMemory",
                typeof(int),
                2048,
                ConfigurationPropertyOptions.IsRequired
            );

            propertySecure = new ConfigurationProperty(
                "secure",
                typeof(bool),
                true
            );

            propertyEnforceUlimit = new ConfigurationProperty(
                "enforceUlimit",
                typeof(bool),
                true
            );

            propertyDisableDirCleanup = new ConfigurationProperty(
                "disableDirCleanup",
                typeof(bool),
                false
            );

            
            propertyForceHttpSharing = new ConfigurationProperty(
                "forceHttpSharing",
                typeof(bool),
                false
            );

            propertyRuntimes = new ConfigurationProperty(
             "runtimes",
             typeof(RuntimeCollection),
             new RuntimeCollection()
            );

            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyBaseDir);
            properties.Add(propertyLocalRoute);
            properties.Add(propertyFilerPort);
            properties.Add(propertyHeartBeatInterval);
            properties.Add(propertyDisableDirCleanup);
            properties.Add(propertyMessageBus);
            properties.Add(propertyMultiTenant);
            properties.Add(propertyMaxMemory);
            properties.Add(propertySecure);
            properties.Add(propertyEnforceUlimit);
            properties.Add(propertyForceHttpSharing);
            properties.Add(propertyRuntimes);
        }
        
        #endregion

        #region Static Fields

        private static ConfigurationProperty propertyBaseDir;
        private static ConfigurationProperty propertyLocalRoute;
        private static ConfigurationProperty propertyFilerPort;
        private static ConfigurationProperty propertyHeartBeatInterval;
        private static ConfigurationProperty propertyMessageBus;
        private static ConfigurationProperty propertyMultiTenant;
        private static ConfigurationProperty propertyMaxMemory;
        private static ConfigurationProperty propertySecure;
        private static ConfigurationProperty propertyEnforceUlimit;
        private static ConfigurationProperty propertyDisableDirCleanup;
        private static ConfigurationProperty propertyForceHttpSharing;
        private static ConfigurationProperty propertyRuntimes;

        private static ConfigurationPropertyCollection properties;
        #endregion

        #region Properties


        /// <summary>
        /// Base directory where all applications are staged and hosted
        /// </summary>
        [ConfigurationProperty("baseDir", IsRequired=true, DefaultValue = null)]
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
        /// Local_route is the IP address of a well known server on your network, it
        /// is used to choose the right ip address (think of hosts that have multiple nics
        /// and IP addresses assigned to them) of the host running the DEA. Default
        /// value of null, should work in most cases.
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
        /// Port for accessing the files of running applications
        /// </summary>
        [ConfigurationProperty("filerPort", IsRequired = false, DefaultValue = 12345)]
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
        /// Time interval to send heartbeat messages to the message bus in milliseconds
        /// </summary>
        [ConfigurationProperty("heartBeatInterval", IsRequired = false, DefaultValue = 10000)]
        public int HeartBeatInterval
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
        /// NATS message bus URI
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
        /// Allow more than one application to run per DEA
        /// </summary>
        [ConfigurationProperty("multiTenant", IsRequired = false, DefaultValue = true)]
        public bool MultiTenant
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
        /// Maximum memory allocated to this DEA. In a multi tenant setup, this
        /// memory is divided amongst all applications managed by this DEA.
        /// </summary>
        [ConfigurationProperty("maxMemory", IsRequired = false, DefaultValue = 2048)]
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
        /// Secure environment for running applications in a multi tenant setup.
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
        /// Provide ulimit based resource isolation in a multi tenant setup.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ulimit"), ConfigurationProperty("enforceUlimit", IsRequired = false, DefaultValue = true)]
        public bool EnforceUlimit
        {
            get
            {
                return (bool)base[propertyEnforceUlimit];
            }
            set
            {
                base[propertyEnforceUlimit] = value;
            }
        }

        /// <summary>
        /// Option to disable the cleanup of droplet instances after stopping them
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
        /// Force droplets to be downloaded over http even when
        /// there is a shared directory containing the droplet.
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
        /// Force droplets to be downloaded over http even when
        /// there is a shared directory containing the droplet.
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