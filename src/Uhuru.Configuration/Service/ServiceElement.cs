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
        /// <summary>
        /// Gets or sets the node id for the service.
        /// </summary>
        [ConfigurationProperty("nodeId", IsRequired = true, DefaultValue = null)]
        public string NodeId
        {
            get
            {
                return (string)base["nodeId"];
            }

            set
            {
                base["nodeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the plan for the service.
        /// </summary>
        [ConfigurationProperty("plan", IsRequired = false, DefaultValue = "free")]
        public string Plan
        {
            get
            {
                return (string)base["plan"];
            }

            set
            {
                base["plan"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the path used for migration.
        /// </summary>
        [ConfigurationProperty("migrationNfs", IsRequired = false, DefaultValue = "")]
        public string MigrationNFS
        {
            get
            {
                return (string)base["migrationNfs"];
            }

            set
            {
                base["migrationNfs"] = value;
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
                return (string)base["mbus"];
            }

            set
            {
                base["mbus"] = value;
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
                return (int)base["index"];
            }

            set
            {
                base["index"] = value;
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
                return (int)base["statusPort"];
            }

            set
            {
                base["statusPort"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval at which to update the Healthz and Varz
        /// </summary>
        [ConfigurationProperty("zInterval", IsRequired = false, DefaultValue = 30000)]
        public int ZInterval
        {
            get
            {
                return (int)base["zInterval"];
            }

            set
            {
                base["zInterval"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the base directory for the service.
        /// </summary>
        [ConfigurationProperty("baseDir", IsRequired = false, DefaultValue = ".\\")]
        public string BaseDir
        {
            get
            {
                return (string)base["baseDir"];
            }

            set
            {
                base["baseDir"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of available capacity for this service.
        /// </summary>
        [ConfigurationProperty("capacity", IsRequired = false, DefaultValue = 200)]
        public int Capacity
        {
            get
            {
                return (int)base["capacity"];
            }

            set
            {
                base["capacity"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the local database file in which to save the list of provisioned services.
        /// </summary>
        [ConfigurationProperty("localDb", IsRequired = false, DefaultValue = "localServiceDb.xml")]
        public string LocalDB
        {
            get
            {
                return (string)base["localDb"];
            }

            set
            {
                base["localDb"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the IP address of a well known server on your network; it
        /// is used to choose the right ip address (think of hosts that have multiple nics
        /// and IP addresses assigned to them) of the host running the DEA. Default
        /// value of null, should work in most cases.
        /// </summary>
        [ConfigurationProperty("localRoute", IsRequired = false, DefaultValue = "198.41.0.4")]
        public string LocalRoute
        {
            get
            {
                return (string)base["localRoute"];
            }

            set
            {
                base["localRoute"] = value;
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
                return (MSSqlElement)base["mssql"];
            }

            set
            {
                base["mssql"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the configuration settings for an Uhurufs system service.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Uhurufs", Justification = "No hungarian notation"),
        ConfigurationProperty("uhurufs", IsRequired = false, DefaultValue = null)]
        public UhurufsElement Uhurufs
        {
            get
            {
                return (UhurufsElement)base["uhurufs"];
            }

            set
            {
                base["uhurufs"] = value;
            }
        }

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