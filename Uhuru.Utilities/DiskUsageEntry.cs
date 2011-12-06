// -----------------------------------------------------------------------
// <copyright file="DiskUsageEntry.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    /// <summary>
    /// This class contains disk usage information.
    /// </summary>
    public class DiskUsageEntry
    {
        private string readableSize;
        private long size;
        private string directory;

        /// <summary>
        /// Initializes a new instance of the DiskUsageEntry class
        /// </summary>
        /// <param name="readableSize">Directory size as a human readable string.</param>
        /// <param name="size">Directory size in kilobytes.</param>
        /// <param name="directory">The directory path.</param>
        public DiskUsageEntry(string readableSize, long size, string directory)
        {
            this.readableSize = readableSize;
            this.size = size;
            this.directory = directory;
        }

        /// <summary>
        /// Gets the directory size as a human readable string.
        /// </summary>
        public string ReadableSize
        {
            get
            {
                return this.readableSize;
            }
        }

        /// <summary>
        /// Gets the directory size in kilobytes.
        /// </summary>
        public long Size
        {
            get
            {
                return this.size;
            }
        }

        /// <summary>
        /// Gets the directory path.
        /// </summary>
        public string Directory
        {
            get
            {
                return this.directory;
            }
        }
    }
}
