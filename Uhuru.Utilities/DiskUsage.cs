using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Uhuru.Utilities
{
    /// <summary>
    /// This class contains disk usage information.
    /// </summary>
    public class DiskUsageEntry
    {
        string readableSize;
        long size;
        string directory;

        /// <summary>
        /// Public constructor used to initialize the fields of this class.
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
        /// Directory size as a human readable string.
        /// </summary>
        public string ReadableSize
        {
            get
            {
                return readableSize;
            }
        }

        /// <summary>
        /// Directory size in kilobytes.
        /// </summary>
        public long Size
        {
            get
            {
                return size;
            }
        }

        /// <summary>
        /// The directory path.
        /// </summary>
        public string Directory
        {
            get
            {
                return directory;
            }
        }
    }

    /// <summary>
    /// This class is used to get disk usage information for a directory.
    /// </summary>
    public static class DiskUsage
    {

        /// <summary>
        /// Gets disk usage information for a directory.
        /// </summary>
        /// <param name="directory">Specifies directory to in which to look for objects</param>
        /// <param name="pattern">Pattern used to filter objects</param>
        /// <param name="summary">Only return summary of directory, no recursion</param>
        /// <returns>An array of DiskUsageEntry objects.</returns>
        public static DiskUsageEntry[] GetDiskUsage(string directory, string pattern, bool summary)
        {
            SortedList<string, long> allObjects = new SortedList<string, long>();

            if (pattern == null)
            {
                if (!summary)
                {
                    string[] allFiles = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
                    string[] allDirectories = Directory.GetDirectories(directory, "*", SearchOption.AllDirectories);

                    foreach (string file in allFiles)
                    {
                        allObjects.Add(file, GetFileSize(file));
                    }

                    foreach (string dir in allDirectories)
                    {
                        allObjects.Add(dir, GetDirectorySize(dir, false));
                    }
                }
                else
                {
                    allObjects.Add(directory, GetDirectorySize(directory, true));
                }
            }
            else
            {
                if (!summary)
                {
                    string[] allFiles = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
                    string[] allDirectories = Directory.GetDirectories(directory, pattern, SearchOption.AllDirectories);

                    foreach (string file in allFiles)
                    {
                        allObjects.Add(file, GetFileSize(file));
                    }

                    foreach (string dir in allDirectories)
                    {
                        allObjects.Add(dir, GetDirectorySize(dir, false));
                    }
                }
                else
                {
                    string[] allFiles = Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly);
                    string[] allDirectories = Directory.GetDirectories(directory, pattern, SearchOption.TopDirectoryOnly);

                    foreach (string file in allFiles)
                    {
                        try
                        {
                            allObjects.Add(file, GetFileSize(file));
                        }
                        catch (IOException)
                        {
                        }
                    }

                    foreach (string dir in allDirectories)
                    {
                        try
                        {
                            allObjects.Add(dir, GetDirectorySize(dir, true));
                        }
                        catch (IOException)
                        {
                        }
                    }
                }
            }

            List<DiskUsageEntry> result = new List<DiskUsageEntry>(allObjects.Count);

            foreach (string obj in allObjects.Keys)
            {
                DiskUsageEntry entry = new DiskUsageEntry(GetReadableForm(allObjects[obj]), allObjects[obj], obj);
                result.Add(entry);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Writes disk usage information to a file.
        /// </summary>
        /// <param name="fileName">The file in which to write th data.</param>
        /// <param name="readable">Boolean value specifying whether to include the human readable size.</param>
        /// <param name="directory">The directory for which to retrieve disk usage.</param>
        /// <param name="pattern">The pattern of the directories to include.</param>
        /// <param name="summary">Boolean value specifying whether to include information about child directories.</param>
        public static void WriteDiskUsageToFile(string fileName, bool readable, string directory, string pattern, bool summary)
        {
            DiskUsageEntry[] entries = GetDiskUsage(directory, pattern, summary);

            StringBuilder fileOutput = new StringBuilder();

            foreach (DiskUsageEntry entry in entries)
            {
                string size = readable ? entry.ReadableSize : entry.Size.ToString(CultureInfo.InvariantCulture);
                fileOutput.AppendFormat(CultureInfo.InvariantCulture, "{0}\t{1}", size, entry.Directory);
            }

            File.WriteAllText(fileName, fileOutput.ToString());
        }

        /// <summary>
        /// Gets a directory size, in kilobytes.
        /// </summary>
        /// <param name="directory">A string specifying the path of the directory.</param>
        /// <param name="recurse">A boolean vallue specifying whether to include child directories.</param>
        /// <returns>The size of the directory in bytes.</returns>
        private static long GetDirectorySize(string directory, bool recurse)
        {
            string[] a = Directory.GetFiles(directory, "*.*",
                recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            long b = 0;
            foreach (string name in a)
            {
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            return (long)Math.Ceiling(b / 1024.0f);
        }

        private static long GetFileSize(string file)
        {
            FileInfo info = new FileInfo(file);
            return info.Length;
        }

        private static string GetReadableForm(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };

            int order = 0;
            while (size >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                size = size / 1024;
            }

            string result = String.Format(CultureInfo.InvariantCulture, "{0:0.##}{1}", size, sizes[order]);

            return result;
        }
    }
}
