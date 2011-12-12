// -----------------------------------------------------------------------
// <copyright file="DiskUsageEntry.cs" company="Uhuru Software, Inc.">
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
        /// <summary>
        /// Directory size as a human readable string.
        /// </summary>
        private string readableSize;

        /// <summary>
        /// Directory size in kilobytes.
        /// </summary>
        private long sizeKB;

        /// <summary>
        /// The path to the directory.
        /// </summary>
        private string directory;

        /// <summary>
        /// Initializes a new instance of the DiskUsageEntry class.
        /// </summary>
        /// <param name="readableSize">Directory size as a human readable string.</param>
        /// <param name="size">Directory size in kilobytes.</param>
        /// <param name="directory">The directory path.</param>
        public DiskUsageEntry(string readableSize, long size, string directory)
        {
            this.readableSize = readableSize;
            this.sizeKB = size;
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
        public long SizeKB
        {
            get
            {
                return this.sizeKB;
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
