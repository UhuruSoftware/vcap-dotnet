// -----------------------------------------------------------------------
// <copyright file="DEAUtilities.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using SevenZip;
    using System.Text;
    using System.Net;
    using System.Security.Cryptography;
    using System.Web;
    using System.Collections.Specialized;
    using System.Collections.Generic;

    /// <summary>
    /// A class containing a set of file- and process-related methods. 
    /// </summary>
    public sealed class DEAUtilities
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
        /// Prevents a default instance of the <see cref="DEAUtilities"/> class from being created.
        /// </summary>
        private DEAUtilities()
        { 
        }

        /// <summary>
        /// Archive a directory using the .tar format.
        /// </summary>
        /// <param name="sourceDir">The directory to compress.</param>
        /// <param name="fileName">The name of the archive to be created.</param>
        public static void TarDirectory(string sourceDir, string fileName)
        {
            SetupZlib();

            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.ArchiveFormat = OutArchiveFormat.Tar;
            compressor.CompressDirectory(sourceDir, fileName);
        }

        /// <summary>
        /// Compresses a file using the .gz format.
        /// </summary>
        /// <param name="sourceFile">The file to compress.</param>
        /// <param name="fileName">The name of the archive to be created.</param>
        public static void GzipFile(string sourceFile, string fileName)
        {
            SetupZlib();

            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.ArchiveFormat = OutArchiveFormat.GZip;
            compressor.CompressFiles(fileName, sourceFile);
        }

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
        /// Converts a Ruby date string into a DateTime.
        /// </summary>
        /// <param name="date">The string to convert.</param>
        /// <returns>The converted data.</returns>
        public static DateTime DateTimeFromRubyString(string date)
        {
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.SetAllDateTimePatterns(new string[] { "yyyy-MM-dd HH:mm:ss zzz" }, 'Y');
            return DateTime.Parse(date, dateFormat);
        }
        
        /// <summary>
        /// Returns the number of cores on the current machine.
        /// </summary>
        /// <returns>The number of cores on the current machine.</returns>
        public static int NumberOfCores()
        {
            // todo: stefi: maybe this is not a precise way to get the number of physical cores of a machine
            return Environment.ProcessorCount;
        }
        
        /// <summary>
        /// Tries to write a file to a directory to make sure writing is allowed.
        /// </summary>
        /// <param name="directory"> The directory to write in.</param>
        public static void EnsureWritableDirectory(string directory)
        {
            string testFile = Path.Combine(directory, string.Format(CultureInfo.InvariantCulture, Strings.NatsMessageDeaSentinel, Process.GetCurrentProcess().Id));
            File.WriteAllText(testFile, string.Empty);
            File.Delete(testFile);
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
                    stream = asm.GetManifestResourceStream("Uhuru.CloudFoundry.DEA.lib.7z64.dll");                    
                }
                else
                {
                    libraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SevenZipExtractor)).Location), @"7z86.dll");
                    stream = asm.GetManifestResourceStream("Uhuru.CloudFoundry.DEA.lib.7z86.dll");
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

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static string HttpUploadFile(string url, FileInfo file, string paramName, string contentType, string authorization)
        {
            string boundary = Guid.NewGuid().ToString("N");
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.Headers[HttpRequestHeader.Authorization] = authorization;

            // diable this to allow streaming big files, without beeing out of memory.
            request.AllowWriteStreamBuffering = false;

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            byte[] trailerBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

            request.ContentLength = boundaryBytes.Length + headerBytes.Length + trailerBytes.Length + file.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                requestStream.Write(headerBytes, 0, headerBytes.Length);

                FileStream fileStream = file.OpenRead();

                // fileStream.CopyTo(requestStream, 1024 * 1024);

                int bufferSize = 1024 * 1024;

                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                    requestStream.Flush();
                }
                fileStream.Close();


                requestStream.Write(trailerBytes, 0, trailerBytes.Length);
                requestStream.Close();

            }

            using (var respnse = request.GetResponse())
            {
                Stream responseStream = respnse.GetResponseStream();
                StreamReader responseReader = new StreamReader(responseStream);
                return responseReader.ReadToEnd();
            }
        }

        public static Uri GetHmacedUri(string uri, string key, string[] paramsToVerify)
        {
            UriBuilder result = new UriBuilder(uri);
            NameValueCollection param = HttpUtility.ParseQueryString(result.Query);
            NameValueCollection verifiedParams = HttpUtility.ParseQueryString(string.Empty);
            foreach (string str in paramsToVerify)
            {
                verifiedParams[str] = HttpUtility.UrlEncode(param[str]);
            }

            string pathAndQuery = result.Path + "?" + verifiedParams.ToString();
            
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(key);                        
            HMACSHA512 hmacsha512 = new HMACSHA512(keyByte);
            byte[] computeHash = hmacsha512.ComputeHash(encoding.GetBytes(pathAndQuery));
            string hash = BitConverter.ToString(computeHash).Replace("-", string.Empty).ToLower();
            verifiedParams["hmac"] = hash;
            result.Query = verifiedParams.ToString();
            return result.Uri;
        }

        public static bool VerifyHmacedUri(string uri, string key, string[] paramsToVerify)
        {
            UriBuilder result = new UriBuilder(uri);
            NameValueCollection param = HttpUtility.ParseQueryString(result.Query);
            NameValueCollection verifiedParams = HttpUtility.ParseQueryString(string.Empty);
            foreach (string str in paramsToVerify)
            {
                verifiedParams[str] = param[str];
            }

            string pathAndQuery = result.Path + "?" + verifiedParams.ToString();

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(key);
            HMACSHA512 hmacsha512 = new HMACSHA512(keyByte);
            byte[] computeHash = hmacsha512.ComputeHash(encoding.GetBytes(pathAndQuery));
            string hash = BitConverter.ToString(computeHash).Replace("-", string.Empty).ToLower();
            StringComparer comparer = StringComparer.InvariantCultureIgnoreCase;
            if (comparer.Compare(hash, param["hmac"]) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
