using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Uhuru.CloudFoundry.DEA.Configuration.DEA
{
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


        // Base directory where all applications are staged and hosted
        [ConfigurationProperty("baseDir", IsRequired = true, DefaultValue = null)]
        public string BaseDir
        {
            get
            {
                return (string)base[propertyBaseDir];
            }
        }

        // Local_route is the IP address of a well known server on your network, it
        // is used to choose the right ip address (think of hosts that have multiple nics
        // and IP addresses assigned to them) of the host running the DEA. Default
        // value of nil, should work in most cases.
        [ConfigurationProperty("localRoute", IsRequired = false, DefaultValue = "198.41.0.4")]
        public string LocalRoute
        {
            get
            {
                return (string)base[propertyLocalRoute];
            }
        }

        // Port for accessing the files of running applications
        [ConfigurationProperty("filerPort", IsRequired = false, DefaultValue = 12345)]
        public int FilerPort
        {
            get
            {
                return (int)base[propertyFilerPort];
            }
        }


        //Time interval to send heartbeat messages to the message bus in milliseconds
        [ConfigurationProperty("heartBeatInterval", IsRequired = false, DefaultValue = 10000)]
        public int HeartBeatInterval
        {
            get
            {
                return (int)base[propertyHeartBeatInterval];
            }
        }


        // NATS message bus URI
        [ConfigurationProperty("messageBus", IsRequired = true, DefaultValue = null)]
        public string MessageBus
        {
            get
            {
                return (string)base[propertyMessageBus];
            }
        }

        // Allow more than one application to run per DEA
        [ConfigurationProperty("multiTenant", IsRequired = false, DefaultValue = true)]
        public bool MultiTenant
        {
            get
            {
                return (bool)base[propertyMultiTenant];
            }
        }

        // Maximum memory allocated to this DEA. In a multi tenant setup, this
        // memory is divided amongst all applications managed by this DEA.
        [ConfigurationProperty("maxMemory", IsRequired = true, DefaultValue = 2048)]
        public int MaxMemory
        {
            get
            {
                return (int)base[propertyMaxMemory];
            }
        }

        // Secure environment for running applications in a multi tenant setup.
        [ConfigurationProperty("secure", IsRequired = false, DefaultValue = true)]
        public bool Secure
        {
            get
            {
                return (bool)base[propertySecure];
            }
        }

        // Provide ulimit based resource isolation in a multi tenant setup.
        [ConfigurationProperty("enforceUlimit", IsRequired = false, DefaultValue = true)]
        public bool EnforceUlimit
        {
            get
            {
                return (bool)base[propertyEnforceUlimit];
            }
        }

        // Option to disable the cleanup of droplet instances after stopping them
        [ConfigurationProperty("disableDirCleanup", IsRequired = false, DefaultValue = false)]
        public bool DisableDirCleanup
        {
            get
            {
                return (bool)base[propertyDisableDirCleanup];
            }
        }

        //Force droplets to be downloaded over http even when
        //there is a shared directory containing the droplet.
        [ConfigurationProperty("forceHttpSharing", IsRequired = false, DefaultValue = false)]
        public bool ForceHttpSharing
        {
            get
            {
                return (bool)base[propertyForceHttpSharing];
            }
        }

        //Force droplets to be downloaded over http even when
        //there is a shared directory containing the droplet.
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
    }
}
