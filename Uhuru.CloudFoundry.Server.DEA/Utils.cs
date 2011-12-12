// -----------------------------------------------------------------------
// <copyright file="Utils.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading;
    using SevenZip;

    public delegate void StreamWriterCallback(StreamWriter stream);
    public delegate void ProcessDoneCallback(string output, int statusCode);

    /// <summary>
    /// A class containing a set of file- and process-related methods. 
    /// </summary>
    public sealed class Utils
    {
        private static readonly object zLibLock = new object();
        private static bool zLibInitalized = false;

        /// <summary>
        /// Private constructor, to supress the need of the compiler to auto-create a public one.
        /// </summary>
        private Utils()
        { 
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
        /// Starts up a new process and executes a command.
        /// </summary>
        /// <param name="command"> The command to execute. </param>
        /// <param name="arguments"> The arguments of the command. </param>
        /// <returns>The output of the executed command.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static string RunCommandAndGetOutput(string command, string arguments)
        {
            return RunCommandAndGetOutput(command, arguments, false);
        }

        /// <summary>
        /// Starts up a new process and executes a command.
        /// </summary>
        /// <param name="command"> The command to execute. </param>
        /// <param name="arguments"> The arguments of the command. </param>
        /// <returns>The output of the executed command, including errors.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static string RunCommandAndGetOutputAndErrors(string command, string arguments)
        {
            return RunCommandAndGetOutput(command, arguments, true);
        }

        /// <summary>
        /// starts a new process and executes a command
        /// </summary>
        /// <param name="shell">The command to be executed.</param>
        /// <param name="arguments">The arguments of the command.</param>
        /// <param name="writerCallback">The callback to process the input.</param>
        /// <param name="doneCallback">The callback to process the output.</param>
        public static void ExecuteCommands(string shell, string arguments, StreamWriterCallback writerCallback, ProcessDoneCallback doneCallback)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object data)
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = shell;
                start.Arguments = arguments;
                start.CreateNoWindow = true;
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.RedirectStandardInput = true;

                string result = String.Empty;

                using (Process process = Process.Start(start))
                {
                    using (StreamWriter writer = process.StandardInput)
                    {
                        writerCallback(writer);
                    }

                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                    process.WaitForExit();
                    doneCallback(result, process.ExitCode);
                }
            }));
        }
        
        /// <summary>
        /// Starts up a cmd shell and executes a command.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <returns>The process' exit code.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static int ExecuteCommand(string command)
        {
            ProcessStartInfo pi = new ProcessStartInfo("cmd", "/c " + command);
            pi.CreateNoWindow = true;
            pi.UseShellExecute = false;
            Process p = Process.Start(pi);
            p.WaitForExit();
            return p.ExitCode;
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
            //todo: stefi: maybe this is not a precise way to get the number of physical cores of a machine
            return Environment.ProcessorCount;
        }
        
        /// <summary>
        /// Tries to write a file to a directory to make sure writing is allowed.
        /// </summary>
        /// <param name="directory"> The directory to write in.</param>
        public static void EnsureWritableDirectory(string directory)
        {
            string testFile = Path.Combine(directory, String.Format(CultureInfo.InvariantCulture, Strings.NatsMessageDeaSentinel, Process.GetCurrentProcess().Id));
            File.WriteAllText(testFile, "");
            File.Delete(testFile);
        }

        /// <summary>
        /// Starts up a new process and executes a command.
        /// </summary>
        /// <param name="command"> The command to execute. </param>
        /// <param name="arguments"> The arguments of the command. </param>
        /// <param name="outputIncludesErrors"> A value indicated whether the errors are to be included in output or not. </param>
        /// <returns>The output of the executed command.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private static string RunCommandAndGetOutput(string command, string arguments, bool outputIncludesErrors)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = command;
            start.Arguments = arguments;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            using (Process process = Process.Start(start))
            {
                string result = process.StandardOutput.ReadToEnd();

                if (outputIncludesErrors)
                {
                    result += process.StandardError.ReadToEnd();
                }

                return result;
            }
        }

        private static void SetupZlib()
        {
            if (zLibInitalized) return;
            lock (zLibLock)
            {
                if (zLibInitalized) return;

                Stream stream = null;
                Assembly asm = Assembly.GetExecutingAssembly();
                string libraryPath = "";

                if (IntPtr.Size == 8)
                {
                    libraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SevenZipExtractor)).Location), @"7z64.dll");
                    stream = asm.GetManifestResourceStream("Uhuru.CloudFoundry.DEA.lib.7z64.dll");
                }
                else
                {
                    stream = asm.GetManifestResourceStream("Uhuru.CloudFoundry.DEA.lib.7z86.dll");
                    libraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SevenZipExtractor)).Location), @"7z86.dll");
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

                zLibInitalized = true;
            }
        }
    }
}
