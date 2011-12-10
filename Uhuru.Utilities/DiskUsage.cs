// -----------------------------------------------------------------------
// <copyright file="DiskUsage.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;

    /// <summary>
    /// This class is used to get disk usage information for a directory.
    /// </summary>
    public static class DiskUsage
    {
        /// <summary>
        /// Gets disk usage information for a directory.
        /// </summary>
        /// <param name="directory">Specifies the directory where to look for objects</param>
        /// <param name="pattern">The pattern used to filter objects</param>
        /// <param name="useRecursion">Whether to look recursively in the child directories or not</param>
        /// <returns>An array of DiskUsageEntry objects.</returns>
        public static DiskUsageEntry[] GetDiskUsage(string directory, bool summary)
        {
            Dictionary<string, long> allObjects;

            if (summary)
            {
                string[] allFiles = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly);
                string[] allDirectories = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);


                allObjects = new Dictionary<string, long>(allFiles.Length + allDirectories.Length);

                foreach (string file in allFiles)
                {
                    allObjects.Add(file, GetFileSize(file));
                }

                foreach (string dir in allDirectories)
                {
                    allObjects.Add(dir, GetDirectorySize(dir, true, null, null));
                }
            }
            else
            {
                allObjects = new Dictionary<string, long>();
                GetDirectorySize(directory, true, allObjects, new object());
                allObjects.Remove(directory);
            }

            List<DiskUsageEntry> result = new List<DiskUsageEntry>(allObjects.Count);

            foreach (string obj in allObjects.Keys)
            {
                DiskUsageEntry entry = new DiskUsageEntry(
                    GetReadableForm(allObjects[obj]), allObjects[obj], obj.Replace(directory, ""));
                result.Add(entry);
            }

            return (from du in result orderby du.Directory select du).ToArray();
        }

        /// <summary>
        /// Writes disk usage information to a file.
        /// </summary>
        /// <param name="fileName">The file where to write the data.</param>
        /// <param name="readable">Boolean value specifying whether to include the human readable size or not.</param>
        /// <param name="directory">The directory for which to retrieve disk usage.</param>
        /// <param name="pattern">The pattern of the directories to include.</param>
        /// <param name="summary">Boolean value specifying whether to include information about child directories.</param>
        public static void WriteDiskUsageToFile(string fileName, string directory, string pattern, bool summary)
        {
            DiskUsageEntry[] entries = GetDiskUsage(directory, summary);

            StringBuilder fileOutput = new StringBuilder();

            foreach (DiskUsageEntry entry in entries)
            {
                string size = entry.ReadableSize;
                fileOutput.AppendFormat(CultureInfo.InvariantCulture, "{0}\t{1}", size, entry.Directory);
            }

            File.WriteAllText(fileName, fileOutput.ToString());
        }

        /// <summary>
        /// Gets a directory size, in kilobytes.
        /// </summary>
        /// <param name="directory">A string specifying the path of the directory.</param>
        /// <param name="recurse">A boolean value specifying whether to include child directories.</param>
        /// <returns>The size of the directory, in kilobytes.</returns>

        private static long GetDirectorySize(string directory, bool recurse, Dictionary<string, long> objects, object objectsLock)
        {
            long size = 0;
            string[] fileEntries = Directory.GetFiles(directory, "*");

            foreach (string fileName in fileEntries)
            {
                long fileSize = GetFileSize(fileName);
                if (objects != null)
                {
                    lock (objectsLock)
                    {
                        objects.Add(fileName, fileSize);
                    }
                }
                Interlocked.Add(ref size, fileSize);
            }

            if (recurse)
            {
                string[] subdirEntries = Directory.GetDirectories(directory, "*");

                Parallel.For<double>(0, subdirEntries.Length, () => 0, (i, loop, subtotal) =>
                {
                    subtotal += GetDirectorySize(subdirEntries[i], true, objects, objectsLock);
                    return subtotal;
                },
                    (x) =>
                    {
                        Interlocked.Add(ref size, (long)x);
                    }
                );
            }

            if (objects != null)
            {
                lock (objectsLock)
                {
                    objects.Add(directory, size);
                }
            }

            return size;
        }

        /// <summary>
        /// gets the size, in bytes, of a file
        /// </summary>
        /// <param name="file">the name of the file</param>
        /// <returns>the size of the file, in kilobytes</returns>
        private static long GetFileSize(string file)
        {
            FileInfo info = new FileInfo(file);
            return (long)Math.Ceiling(info.Length / 1024.0);
        }

        /// <summary>
        /// converts a numeric file size into a human readable one
        /// </summary>
        /// <param name="size">the size to convert (in KB) (e.g. 1024)</param>
        /// <returns>a nicely formatted string (e.g. 1024 KB = 1MB)</returns>
        private static string GetReadableForm(long size)
        {
            string[] sizes = { "KB", "MB", "GB" };

            int order = 0;
            while (size >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                size = size / 1024;
            }

            string result = string.Format(CultureInfo.InvariantCulture, "{0:0.##}{1}", size, sizes[order]);

            return result;
        }
    }
}
