// -----------------------------------------------------------------------
// <copyright file="FileServiceOptions.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService
{
    /// <summary>
    /// This is a class containing information about where to store files.
    /// </summary>
    public class FileServiceOptions
    {
        /// <summary>
        /// Gets or sets the drive that will be used to store files.
        /// </summary>
        public string SharedDrive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use provision credentials for binding].
        /// This is useful when the DEA cannot connect to two different shared with different credentials on the same host
        /// </summary>
        public bool UseProvisionCredentialsForBinding
        {
            get;
            set;
        }
    }
}
