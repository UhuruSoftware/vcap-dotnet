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

namespace Uhuru.CloudFoundry.DEA
{
    public delegate void StreamWriterCallback(StreamWriter stream);
    public delegate void ProcessDoneCallback(string output, int statuscode);

    public class Utils
    {
        private static readonly object zLibLock = new object();
        private static bool zLibInitalized = false;

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
                    stream = asm.GetManifestResourceStream("CloudFoundry.Net.lib.7z64.dll");
                }
                else
                {
                    stream = asm.GetManifestResourceStream("CloudFoundry.Net.lib.7z86.dll");
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

        public static void ZipFile(string sourceDir, string fileName)
        {
            SetupZlib();

            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.ArchiveFormat = OutArchiveFormat.Zip;
            compressor.CompressDirectory(sourceDir, fileName);
        }

        public static void UnzipFile(string targetDir, string zipFile)
        {
            SetupZlib();

            using (SevenZipExtractor extractor = new SevenZipExtractor(zipFile))
            {
                extractor.ExtractArchive(targetDir);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static string RunCommandAndGetOutput(string command, string arguments)
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
                return result;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static string RunCommandAndGetOutputAndErrors(string command, string arguments)
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
                result += process.StandardError.ReadToEnd();
                return result;
            }
        }

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


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static int ExecuteCommand(string Command)
        {
            ProcessStartInfo pi = new ProcessStartInfo("cmd", "/c " + Command);
            pi.CreateNoWindow = true;
            pi.UseShellExecute = false;
            Process p = Process.Start(pi);
            p.WaitForExit();
            return p.ExitCode;
        }

     

        public static DateTime DateTimeFromRubyString(string date)
        {
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.SetAllDateTimePatterns(new string[] { "yyyy-MM-dd HH:mm:ss zzz" }, 'Y');
            return DateTime.Parse(date, dateFormat);
        }


        //todo: stefi: maby this is not a precise way to get the number of physical cores of a machine
        public static int NumberOfCores()
        {
            return Environment.ProcessorCount;
        }


        public static void EnsureWritableDirectory(string Directory)
        {
            string testFile = Path.Combine(Directory, String.Format(CultureInfo.InvariantCulture, Strings.NatsMessageDeaSentinel, Process.GetCurrentProcess().Id));
            File.WriteAllText(testFile, "");
            File.Delete(testFile);
        }
       
    }

}
