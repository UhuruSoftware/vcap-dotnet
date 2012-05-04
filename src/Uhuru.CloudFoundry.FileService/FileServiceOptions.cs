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
    }
}
