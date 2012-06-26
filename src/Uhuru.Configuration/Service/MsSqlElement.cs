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
        /// Gets or sets the host of the target SQL Server.
        /// </summary>
        [ConfigurationProperty("host", IsRequired = false, DefaultValue = "(local)")]
        public string Host
        {
            get
            {
                return (string)base["host"];
            }

            set
            {
                base["host"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the user for connecting to the target SQL Server.
        /// </summary>
        [ConfigurationProperty("user", IsRequired = true)]
        public string User
        {
            get
            {
                return (string)base["user"];
            }

            set
            {
                base["user"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the password for connecting to the target SQL Server.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get
            {
                return (string)base["password"];
            }

            set
            {
                base["password"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the port for connecting to the target SQL Server.
        /// </summary>
        [ConfigurationProperty("port", IsRequired = false, DefaultValue = 1433)]
        public int Port
        {
            get
            {
                return (int)base["port"];
            }

            set
            {
                base["port"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum database size. Size in MB.
        /// </summary>
        [ConfigurationProperty("maxDbSize", IsRequired = false, DefaultValue = 20L)]
        public long MaxDBSize
        {
            get
            {
                return (long)base["maxDbSize"];
            }

            set
            {
                base["maxDbSize"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum duration for a query in seconds.
        /// </summary>
        [ConfigurationProperty("maxLongQuery", IsRequired = false, DefaultValue = 3)]
        public int MaxLengthyQuery
        {
            get
            {
                return (int)base["maxLongQuery"];
            }

            set
            {
                base["maxLongQuery"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum duration for a transaction in seconds.
        /// </summary>
        [ConfigurationProperty("maxLongTx", IsRequired = false, DefaultValue = 30)]
        public int MaxLengthTX
        {
            get
            {
                return (int)base["maxLongTx"];
            }

            set
            {
                base["maxLongTx"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum duration for a query in seconds.
        /// </summary>
        [ConfigurationProperty("maxUserConns", IsRequired = false, DefaultValue = 20)]
        public int MaxUserConnections
        {
            get
            {
                return (int)base["maxUserConns"];
            }

            set
            {
                base["maxUserConns"] = value;
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
                return (string)base["logicalStorageUnits"];
            }

            set
            {
                base["logicalStorageUnits"] = value;
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
                return (string)base["initialDataSize"];
            }

            set
            {
                base["initialDataSize"] = value;
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
                return (string)base["initialLogSize"];
            }

            set
            {
                base["initialLogSize"] = value;
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
                return (string)base["maxDataSize"];
            }

            set
            {
                base["maxDataSize"] = value;
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
                return (string)base["maxLogSize"];
            }

            set
            {
                base["maxLogSize"] = value;
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
                return (string)base["dataFileGrowth"];
            }

            set
            {
                base["dataFileGrowth"] = value;
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
                return (string)base["logFileGrowth"];
            }

            set
            {
                base["logFileGrowth"] = value;
            }
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
