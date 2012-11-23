// -----------------------------------------------------------------------
// <copyright file="ZipUtilities.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.IO;
    using System.Reflection;
    using SevenZip;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ZipUtilities
    {
        /// <summary>
        /// The lock for SevenZipSharp initialization
        /// </summary>
        private static readonly object zlibLock = new object();

        /// <summary>
        /// Flag if the SevenZipShparp library as initalized.
        /// </summary>
        private static bool zlibInitalized = false;

        /// <summary>
        /// Compresses a file using the .zip format.
        /// </summary>
        /// <param name="sourceDir">The directory to compress.</param>
        /// <param name="fileName">The name of the archive to be created.</param>
        public static void ZipFile(string sourceDir, string fileName)
        {
            SetupZlib();

            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.ArchiveFormat = OutArchiveFormat.Zip;
            compressor.CompressDirectory(sourceDir, fileName);
        }

        /// <summary>
        /// Compresses files using gzip format
        /// </summary>
        /// <param name="files">The files.</param>
        /// <param name="destination">The destination.</param>
        public static void GZipFiles(string[] files, string destination)
        {
            SetupZlib();

            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.ArchiveFormat = OutArchiveFormat.GZip;
            compressor.CompressFiles(destination, files);           
        }

        /// <summary>
        /// Extracts data from a .zip archive.
        /// </summary>
        /// <param name="targetDir">The directory to put the extracted data in.</param>
        /// <param name="zipFile">The file to extract data from.</param>
        public static void UnzipFile(string targetDir, string zipFile)
        {
            SetupZlib();

            using (SevenZipExtractor extractor = new SevenZipExtractor(zipFile))
            {
                extractor.ExtractArchive(targetDir);
            }
        }

        /// <summary>
        /// Setups the zlib library; gets the proper 32 or 64 bit library as a stream from a resource, and loads it.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zlib", Justification = "Zlib is a spelled correctly")]
        public static void SetupZlib()
        {
            if (zlibInitalized)
            {
                return;
            }

            lock (zlibLock)
            {
                if (zlibInitalized)
                {
                    return;
                }

                Stream stream = null;
                Assembly asm = Assembly.GetExecutingAssembly();
                string libraryPath = string.Empty;

                if (IntPtr.Size == 8)
                {
                    libraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SevenZipExtractor)).Location), @"7z64.dll");
                    stream = asm.GetManifestResourceStream("Uhuru.CloudFoundry.ServiceBase.lib.7z64.dll");
                }
                else
                {
                    libraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SevenZipExtractor)).Location), @"7z86.dll");
                    stream = asm.GetManifestResourceStream("Uhuru.CloudFoundry.ServiceBase.lib.7z86.dll");
                }

                if (!File.Exists(libraryPath))
                {
                    byte[] myAssembly = new byte[stream.Length];
                    stream.Read(myAssembly, 0, (int)stream.Length);
                    File.WriteAllBytes(libraryPath, myAssembly);
                    stream.Close();
                }

                SevenZipCompressor.SetLibraryPath(libraryPath);
                SevenZipExtractor.SetLibraryPath(libraryPath);

                zlibInitalized = true;
            }
        }
    }
}
