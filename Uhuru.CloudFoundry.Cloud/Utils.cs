using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using SevenZip;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CloudFoundry.Net
{
    public class Utils
    {
        private static readonly object zLibLock = new object();

        public enum SmallImages
        {
            DefaultImage = 0,
            AllOK = 1,
            Console = 2,
            Modules = 3,
            DbManagement = 4,
            Database = 5,
            Warning = 6,
            Error = 7,
            IIS = 8,
            DotNet = 10,
            MySql = 11,
            MSSQL = 12,
            Java = 13,
            Sinatra, Rails3, Grails, Ruby = 14,
            OtpRebar, Erlang = 15,
            Node_js = 16,
            Redis = 17,
            Spring = 22,
            Lift = 23,
            PostGRE = 24,
            MongoDB = 25,
            RabbitMQ = 26,
            Start = 27,
            Stop = 28,
            BrowseSite = 31,
            Delete = 32,
            VSError = 33,
            VSWarning = 34,
            VSInfo = 35
        }

        public static int GetFrameworkImageIndex(string framework)
        {
            switch (framework)
            {
                case "rails3": return (int)SmallImages.Rails3;
                case "node": return (int)SmallImages.Node_js;
                case "otp_rebar": return (int)SmallImages.OtpRebar;
                case "sinatra": return (int)SmallImages.Sinatra;
                case "net": return (int)SmallImages.DotNet;
                case "spring": return (int)SmallImages.Spring;
                case "grails": return (int)SmallImages.Grails;
                case "lift": return (int)SmallImages.Lift;
                case "java_web": return (int)SmallImages.Java;
                default: return (int)SmallImages.DefaultImage;
            }
        }

        public static int GetRuntimeImageIndex(string framework)
        {
            switch (framework)
            {
                case "ruby18": return (int)SmallImages.Ruby;
                case "ruby19": return (int)SmallImages.Ruby;
                case "node": return (int)SmallImages.Node_js;
                case "erlangR14B02": return (int)SmallImages.Erlang;
                case "iis": return (int)SmallImages.IIS;
                case "java": return (int)SmallImages.Java;
                default: return (int)SmallImages.DefaultImage;
            }
        }

        public static int GetServiceImageIndex(string serviceType)
        {
            switch (serviceType)
            {
                case "mysql": return (int)SmallImages.MySql;
                case "mssql": return (int)SmallImages.MSSQL;
                case "postgresql": return (int)SmallImages.PostGRE;
                case "redis": return (int)SmallImages.Redis;
                case "mongodb": return (int)SmallImages.MongoDB;
                case "rabbitmq": return (int)SmallImages.RabbitMQ;
                default: return (int)SmallImages.DefaultImage;
            }
        }

        public static string GetFramework(string path)
        {
            if (File.Exists(Path.Combine(path, @"config\environment.rb")))
            {
                return "rails3";
            }

            if ((Directory.GetFiles(path, "*.war", SearchOption.AllDirectories)).Length > 0 || File.Exists(Path.Combine(path, @"WEB-INF\web.xml")))
            {
                string war_file = Directory.GetFiles(path, "*.war", SearchOption.AllDirectories).Length > 0 ? Directory.GetFiles(path, "*.war", SearchOption.AllDirectories)[0] : String.Empty;

                string[] contents;

                if (war_file != String.Empty)
                {
                    contents = GetZipFiles(war_file);
                }
                else
                {
                    contents = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Select(file => file.Replace('\\', '/')).ToArray();
                }

                if (contents.Any(file => Regex.IsMatch(file, @"WEB-INF\/lib\/grails-web.*\.jar")))
                {
                    return "grails";
                }
                else if (contents.Any(file => Regex.IsMatch(file, @"WEB-INF\/lib\/lift-webkit.*\.jar")))
                {
                    return "lift";
                }
                else if (contents.Any(file => Regex.IsMatch(file, @"WEB-INF\/classes\/org\/springframework")))
                {
                    return "spring";
                }
                else if (contents.Any(file => Regex.IsMatch(file, @"WEB-INF\/lib\/spring-core.*\.jar")))
                {
                    return "spring";
                }
                else if (contents.Any(file => Regex.IsMatch(file, @"WEB-INF\/lib\/org\.springframework\.core.*\.jar")))
                {
                    return "spring";
                }
                else
                {
                    return "java_web";
                }
            }

            if ((Directory.GetFiles(path, "*.rb", SearchOption.AllDirectories)).Length > 0)
            {
                string matched_file = String.Empty;

                foreach (string fname in Directory.GetFiles(path, "*.rb", SearchOption.AllDirectories))
                {
                    string content = File.ReadAllText(fname);
                    if (Regex.IsMatch(content, @"\s*require[\s\(]*['""]sinatra['""]"))
                    {
                        return "sinatra";
                    }
                }
            }

            if ((Directory.GetFiles(path, "*.js", SearchOption.AllDirectories)).Length > 0)
            {
                if (File.Exists(Path.Combine(path, "server.js")) || File.Exists(Path.Combine(path, "app.js")) || File.Exists(Path.Combine(path, "index.js")) || File.Exists(Path.Combine(path, "main.js")))
                {
                    return "node";
                }
            }

            if ((Directory.GetFiles(path, "*.php", SearchOption.AllDirectories)).Length > 0)
            {
                return "php";
            }

            if (Directory.Exists(Path.Combine(path, "releases")) &&
                Directory.GetDirectories(Path.Combine(path, "releases")).Any(dir => Directory.GetFiles(dir, "*.rel").Length > 0) &&
                Directory.GetDirectories(Path.Combine(path, "releases")).Any(dir => Directory.GetFiles(dir, "*.boot").Length > 0))
            {
                return "otp_rebar";
            }

            if ((Directory.GetFiles(path, "manage.py", SearchOption.AllDirectories)).Length > 0)
            {
                return "django";
            }

            if ((Directory.GetFiles(path, "wsgi.py", SearchOption.AllDirectories)).Length > 0)
            {
                return "wsgi";
            }

            if ((Directory.GetFiles(path, "web.config", SearchOption.AllDirectories)).Length > 0)
            {
                return "net";
            }
            return String.Empty;
        }

        private static string[] GetZipFiles(string zipFile)
        {
            if (IntPtr.Size == 8)
            {
                SevenZipExtractor.SetLibraryPath(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SevenZipCompressor)).Location), @"lib\7z64.dll"));
            }
            else
            {
                SevenZipExtractor.SetLibraryPath(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SevenZipCompressor)).Location), @"lib\7z64.dll"));
            }

            SevenZipExtractor extractor = new SevenZipExtractor(zipFile);
            return extractor.ArchiveFileNames.ToArray();
        }

        public static string GetDefaultUrlForApp(string appName, string targetUrl)
        {
            targetUrl = targetUrl.ToLower().Replace("http://", "").Replace("api.", "");
            return appName + "." + targetUrl;
        }

        public static void OpenLink(string sUrl)
        {
            if (!sUrl.Contains("://"))  //if no prefix has been specified
                sUrl = "http://" + sUrl; //add http by default

            try
            {
                System.Diagnostics.Process.Start(sUrl);
            }
            catch (Exception exc1)
            {
                // System.ComponentModel.Win32Exception is a known exception that occurs when Firefox is default browser.          
                // It actually opens the browser but STILL throws this exception so we can just ignore it.  If not this exception,        
                // then attempt to open the URL in IE instead.        
                if (exc1.GetType().ToString() != "System.ComponentModel.Win32Exception")
                {
                    // sometimes throws exception so we have to just ignore            
                    // this is a common .NET bug that no one online really has a great reason for so now we just need to try to open            
                    // the URL using IE if we can.            
                    try
                    {
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("IExplore.exe", sUrl);
                        System.Diagnostics.Process.Start(startInfo);
                        startInfo = null;
                    }
                    catch (Exception exc2)
                    {                // still nothing we can do so just show the error to the user here.          
                    }
                }
            }
        }

        private static void SetupZlib()
        {
            lock (zLibLock)
            {
                Stream stream = null;
                Assembly asm = Assembly.GetExecutingAssembly();
                string libraryPath = "";

                if (IntPtr.Size == 8)
                {
                    libraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SevenZipExtractor)).Location), @"7z64.dll");
                    stream = asm.GetManifestResourceStream("Uhuru.CloudFoundry.Cloud.lib.7z64.dll");
                }
                else
                {
                    stream = asm.GetManifestResourceStream("Uhuru.CloudFoundry.Cloud.lib.7z86.dll");
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
            }
        }

        public static void ZipFile(string sourceDir, string zipFile)
        {
            SetupZlib();

            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.ArchiveFormat = OutArchiveFormat.Zip;
            compressor.CompressDirectory(sourceDir, zipFile);
        }

        public static void UnZipFile(string targetDir, string zipFile)
        {
            SetupZlib();

            SevenZipExtractor extractor = new SevenZipExtractor(zipFile);
            extractor.ExtractArchive(targetDir);
        }

        public static int GetAppStateImageIndex(App app)
        {
            if (app.RunningInstances == app.Instances)
            {
                return (int)Utils.SmallImages.AllOK;
            }
            else if (app.RunningInstances == "0")
            {
                return (int)Utils.SmallImages.Error;
            }
            else
            {
                return (int)Utils.SmallImages.Warning;
            }
        }

    }
}
