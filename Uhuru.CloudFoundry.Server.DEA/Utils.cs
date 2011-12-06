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

        public static void SystemCleanup(string AppsDir = "", string AppStateFile = "")
        {
            try
            {
                ExecuteCommand(@"taskkill /im netiis.exe /f /t");
            }
            catch { }
            try
            {
                ExecuteCommand(String.Format(@"netiis -cleanup={0}", AppsDir));
            }
            catch { }
            try
            {
                //delete_untracked_instances does the same thing
                //Directory.Delete(AppsDir, true);
                //Directory.CreateDirectory(AppsDir);
            }
            catch { }

            try
            {
                File.Delete(AppStateFile);
            }
            catch { }
        }


        public static int DateTimeToEpochSeconds(DateTime date)
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public static DateTime DateTimeFromEpochSeconds(int seconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0) + new TimeSpan(0, 0, seconds);
        }

        public static string DateTimeToRubyString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss zzz");
        }

        public static DateTime DateTimeFromRubyString(string date)
        {
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.SetAllDateTimePatterns(new string[] { "yyyy-MM-dd HH:mm:ss zzz" }, 'Y');
            return DateTime.Parse(date, dateFormat);
        }

        public static string GetStack()
        {
            StackTrace stackTrace = new StackTrace();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            StringBuilder sb = new StringBuilder();

            // write call stack method names
            foreach (StackFrame stackFrame in stackFrames)
            {
                sb.AppendLine(stackFrame.GetMethod().Name);   // write method name
            }

            return sb.ToString();
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

        //returns the ip used by the OS to connect to the RouteIPAddress. Pointing to a interface address will return that same address
        public static string GetLocalIpAddress(string RouteIPAddress = "198.41.0.4")
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(RouteIPAddress, 1);
            IPEndPoint ep = (IPEndPoint)udpClient.Client.LocalEndPoint;
            udpClient.Close();
            return ep.Address.ToString();
        }

        public static int GetEphemeralPort()
        {
            TcpListener socket = new TcpListener(IPAddress.Any, 0);
            socket.Start();
            int port = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();
            return port;
        }

        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }


    }

}
