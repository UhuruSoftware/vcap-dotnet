// -----------------------------------------------------------------------
// <copyright file="MsSqlOptions.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService
{
    /// <summary>
    /// This is a class containing information about connecting to an MS Sql Server.
    /// </summary>
    public class MSSqlOptions
    {
        /// <summary>
        /// Gets or sets the host for connecting to the service.
        /// </summary>
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user for connecting to the SQL Server.
        /// </summary>
        public string User
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the password for connection to the SQL Server.
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port for connection to the SQL Server.
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the list of drives for sql file storage
        /// </summary>
        public string LogicalStorageUnits
        {
            get;
            set;
        }
    }
}
