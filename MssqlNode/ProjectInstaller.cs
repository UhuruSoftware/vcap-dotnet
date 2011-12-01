using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Text;

namespace mssqlnode
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            AfterInstall += new InstallEventHandler(ProjectInstaller_AfterInstall);
            AfterRollback += new InstallEventHandler(ProjectInstaller_AfterRollback);
            AfterUninstall += new InstallEventHandler(ProjectInstaller_AfterUninstall);
            BeforeInstall += new InstallEventHandler(ProjectInstaller_BeforeInstall);
            BeforeRollback += new InstallEventHandler(ProjectInstaller_BeforeRollback);
            BeforeUninstall += new InstallEventHandler(ProjectInstaller_BeforeUninstall);
        }

        void ProjectInstaller_BeforeUninstall(object sender, InstallEventArgs e)
        {
            //occures before uninstallation
            //code that has to be run before uninstallation goes here
        }

        void ProjectInstaller_BeforeRollback(object sender, InstallEventArgs e)
        {
            //occures before rollback
            //code that has to be run before rollback goes here
        }

        void ProjectInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {
            //occures before installation
            //code that has to be run before installation goes here
        }

        void ProjectInstaller_AfterUninstall(object sender, InstallEventArgs e)
        {
            //occures after uninstallation
            //code that has to be run after uninstallation goes here
        }

        void ProjectInstaller_AfterRollback(object sender, InstallEventArgs e)
        {
            //occures after rollback
            //code that has to be run after rollback goes here
        }

        void ProjectInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            //occures after installation
            //code that has to be run after installation goes here
        }

        string assemblyPath = null;

        private void serviceProcessInstallerMssqlNode_BeforeInstall(object sender, InstallEventArgs e)
        {
            if (assemblyPath == null)
            {
                assemblyPath = Context.Parameters["assemblypath"];
            }
            string winDEACmd = "mssqlNodeCmd";
            Context.Parameters["assemblypath"] = "\"" + assemblyPath + "\" " + winDEACmd;
        }

        

        // overrides the base installer. code can be written before or after base.Install(); just don't remove that line!
        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);

            //Debugger.Launch();
            

            string tempFolder = Path.GetTempPath();
            string rubyInstellerPath = tempFolder + "rubyInstaller.exe";
            string rubyDevkitInstallerPath = tempFolder + "rubyDevkitInstaller.exe";         
            InstallArguments arguments = new InstallArguments(this.Context);

            if (IntPtr.Size == 8)
                        SevenZip.SevenZipExtractor.SetLibraryPath(arguments.TargetDir + @"\lib\7z64.dll");
                    else
                        SevenZip.SevenZipExtractor.SetLibraryPath(arguments.TargetDir + @"\lib\7z86.dll");

            WebClient webClient = new WebClient();
            webClient.Proxy = arguments.Proxy;
            string configFile = Directory.GetFiles(arguments.TargetDir, "*.exe.config")[0];

            if (!Directory.Exists(arguments.MssqlNodePath))
                Directory.CreateDirectory(arguments.MssqlNodePath);

            SevenZip.SevenZipExtractor extractor = new SevenZip.SevenZipExtractor(arguments.TargetDir + @"\services.zip");
            extractor.ExtractArchive(arguments.MssqlNodePath);


            
            if (arguments.InstallDevKit || arguments.InstallRuby)
            {
                string setupConfigFile = tempFolder + "setupresources.xml";
                //webClient.DownloadFile(arguments.OnlineConfig, setupConfigFile);
                //XmlDocument xml = new XmlDocument();
                //xml.Load(setupConfigFile);

                if (arguments.InstallRuby)
                {
                    //string rubyInstallerUrl = xml.GetElementsByTagName("rubyInstaller")[0].InnerText;
                    //webClient.DownloadFile(rubyInstallerUrl, rubyInstellerPath);
                    webClient.DownloadFile(@"http://rubyforge.org/frs/download.php/75127/rubyinstaller-1.9.2-p290.exe", rubyInstellerPath);
                    Process rubyInstaller = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = rubyInstellerPath;
                    startInfo.Arguments = "/verysilent /dir=\"" + arguments.RubyPath + "\" /tasks=\"assocfiles,modpath\"";
                    rubyInstaller.StartInfo = startInfo;
                    rubyInstaller.Start();
                    rubyInstaller.WaitForExit();
                    if (File.Exists(rubyInstellerPath))
                        File.Delete(rubyInstellerPath);
                    UpdateKey(configFile, "rubyPath", arguments.RubyPath.TrimEnd('\\') + @"\bin\ruby.exe");
                }

                if (arguments.InstallDevKit)
                {
                    //string rubyDevkitInstallerUrl = xml.GetElementsByTagName("rubyDevkitInstaller")[0].InnerText;
                    //webClient.DownloadFile(rubyDevkitInstallerUrl, rubyDevkitInstallerPath);
                    webClient.DownloadFile(@"http://github.com/downloads/oneclick/rubyinstaller/DevKit-tdm-32-4.5.1-20101214-1400-sfx.exe", rubyDevkitInstallerPath);
                

                    
                    SevenZip.SevenZipExtractor ext = new SevenZip.SevenZipExtractor(rubyDevkitInstallerPath);
                    ext.ExtractArchive(arguments.DevKitPath);
                    if (File.Exists(rubyDevkitInstallerPath))
                        File.Delete(rubyDevkitInstallerPath);
                    UpdateKey(configFile, "rubyDevkitPath", arguments.DevKitPath.TrimEnd('\\'));
                }
            }
            
            
            UpdateKey(configFile, "mssqlNodeCmd", arguments.MssqlNodePath + @"\services\mssql\bin\mssql_node");
            UpdateKey(configFile, "mssqlNodeCmdDir", arguments.MssqlNodePath + @"\services");

            

            string deaYmlFile = arguments.MssqlNodePath + @"\services\mssql\config\mssql_node.yml";

            StreamReader reader = new StreamReader(deaYmlFile);
            StringBuilder sb = new StringBuilder();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string newLine = line;
                if (newLine.Contains(':'))
                {
                    switch (line.Substring(0, line.IndexOf(':')).Trim())
                    {
                            /*
                        
                        case "local_route":
                            {
                                string ip = "";
                                foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                                {
                                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                    {
                                        ip = address.ToString();
                                    }
                                }
                                newLine = line.Substring(0, line.IndexOf(':') + 1) + " " + ip;
                                break;
                            }
                        
                        
                             */
                        case "mbus":
                            {
                                if (arguments.CloudControllerIp != "")
                                {
                                    newLine = line.Substring(0, line.IndexOf(':') + 1) + " nats://nats:nats@" + arguments.CloudControllerIp;
                                    if(arguments.CloudControllerPort != "") newLine += ":" + arguments.CloudControllerPort;
                                    newLine += "/";
                                }

                                if (arguments.MbusUrl != "")
                                {
                                    newLine = line.Substring(0, line.IndexOf(':') + 1) + " " + arguments.MbusUrl;
                                }

                                break;
                            }
                        case "host":
                            {
                                if (arguments.DbConnectionStr != "")
                                {
                                    newLine = line.Substring(0, line.IndexOf(':') + 1) + " \"" + arguments.DbConnectionStr + "\"";
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                sb.AppendLine(newLine);
            }
            reader.Close();
            StreamWriter writer = new StreamWriter(deaYmlFile);
            writer.Write(sb.ToString());
            writer.Flush();
            writer.Close();


            
            
            StreamWriter cmdWriter = new StreamWriter(arguments.TargetDir + @"\installGems.bat");
            cmdWriter.WriteLine("call \"" + arguments.DevKitPath + "\\devkitvars.bat\"");
            cmdWriter.WriteLine("call \"" + arguments.RubyPath + "\\bin\\setrbvars.bat\"");
            cmdWriter.WriteLine("call \"" + arguments.TargetDir + "\\geminstall.cmd\"");
            cmdWriter.WriteLine("cd \"" + arguments.MssqlNodePath + @"\windea\wingems\winevent\ext\winevent""");
            cmdWriter.WriteLine(@"call ""installWinevent.cmd""");
            cmdWriter.Flush();
            cmdWriter.Close();

            Process gemInstall = new Process();
            ProcessStartInfo gemInstallStartInfo = new ProcessStartInfo();
            gemInstallStartInfo.CreateNoWindow = true;
            //gemInstallStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            gemInstallStartInfo.FileName = arguments.TargetDir + @"\installGems.bat";
            gemInstallStartInfo.WorkingDirectory = arguments.MssqlNodePath + @"\services\mssql\vendor\cache";
            gemInstall.StartInfo = gemInstallStartInfo;
            gemInstall.Start();
            gemInstall.WaitForExit();
             
        }

        // rollback is called when an exception is thrown during installation
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        //overrides the uninstaller. code that does the cleanup on uninstall goes here
        public override void Uninstall(IDictionary savedState)
        {
            UninstallArguments arguments = new UninstallArguments(this.Context);
            base.Uninstall(savedState);
        }

        // occures after installation. stores information for correct uninstall later
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        private void UpdateKey(string fileName, string key, string newValue)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");

            // Attempt to locate the requested setting.
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == key)
                    childNode.Attributes["value"].Value = newValue;
            }
            xmlDoc.Save(fileName);
        }

        private class InstallArguments
        {
            private class Argument
            {
                public const string installRuby = "installRuby";
                public const string installDevKit = "installDevKit";
                public const string rubyPath = "rubyPath";
                public const string devKitPath = "devKitPath";
                public const string winDeaPath = "winDeaPath";
                public const string mssqlNodePath = "mssqlNodePath";
                public const string dropletPath = "dropletPath";
                public const string proxy = "proxy";
                public const string targetDir = "TARGETDIR";
                public const string cloudControllerIp = "cloudControllerIp";
                public const string cloudControllerPort = "cloudControllerPort";
                public const string onlineConfig = "onlineConfig";
                public const string dbConnectionStr = "dbConnectionStr";
                public const string mbusUrl = "mbusUrl";

            }

            private bool _installRuby;
            private bool _installDevKit;
            private string _rubyPath;
            private string _devKitPath;
            private string _winDeaPath;
            private string _mssqlNodePath;
            private string _dropletPath;
            private string _targetDir;
            private WebProxy _proxy;
            private string _cloudControllerIp;
            private string _cloudControllerPort;
            private string _onlineConfig;
            private string _dbConnectionStr;
            private string _mbusUrl;

            public bool InstallRuby { get { return _installRuby; } }
            public bool InstallDevKit { get { return _installDevKit; } }
            public string RubyPath { get { return _rubyPath; } }
            public string DevKitPath { get { return _devKitPath; } }
            public string WinDEAPath { get { return _winDeaPath; } }
            public string MssqlNodePath { get { return _mssqlNodePath; } }
            public string DropletPath { get { return _dropletPath; } }
            public string TargetDir { get { return _targetDir; } }
            public WebProxy Proxy { get { return _proxy; } }
            public string CloudControllerIp { get { return _cloudControllerIp; } }
            public string CloudControllerPort { get { return _cloudControllerPort; } }
            public string OnlineConfig { get { return _onlineConfig; } }
            public string DbConnectionStr { get { return _dbConnectionStr; } }
            public string MbusUrl { get { return _mbusUrl; } }

            public InstallArguments(InstallContext context)
            {
                _installRuby = context.IsParameterTrue(Argument.installRuby);
                _installDevKit = context.IsParameterTrue(Argument.installDevKit);
                _targetDir = context.Parameters[Argument.targetDir].TrimEnd('\\');

                if (!_installRuby)
                    if (context.Parameters[Argument.rubyPath] == "")
                        throw new Exception("Ruby path is required!");

                if (!_installDevKit)
                    if (context.Parameters[Argument.devKitPath] == "")
                        throw new Exception("Ruby DevKit path is required!");

                if (context.Parameters[Argument.rubyPath] != "")
                    _rubyPath = context.Parameters[Argument.rubyPath].TrimEnd('\\');
                else
                    _rubyPath = @"C:\ruby";

                if (context.Parameters[Argument.devKitPath] != "")
                    _devKitPath = context.Parameters[Argument.devKitPath].TrimEnd('\\');
                else
                    _devKitPath = @"C:\ruby_devkit";

                

                if (context.Parameters[Argument.mssqlNodePath] != "")
                    _mssqlNodePath = context.Parameters[Argument.mssqlNodePath].TrimEnd('\\');
                else
                    _mssqlNodePath = _targetDir + @"\services";


                if (context.Parameters[Argument.proxy] != null && context.Parameters[Argument.proxy] != string.Empty)
                    _proxy = new WebProxy(context.Parameters[Argument.targetDir]);
                else
                    _proxy = null;

                _cloudControllerPort = context.Parameters[Argument.cloudControllerPort];
                _cloudControllerIp = context.Parameters[Argument.cloudControllerIp];
                _dbConnectionStr = context.Parameters[Argument.dbConnectionStr];
                _mbusUrl = context.Parameters[Argument.mbusUrl];

                if (context.Parameters[Argument.onlineConfig] != "")
                    _onlineConfig = context.Parameters[Argument.onlineConfig];
                else
                    _onlineConfig = "http://uhurucloud.net/setupresources.xml";


            }
        }

        private class UninstallArguments
        {
            private class Argument
            {
                public const string uninstallRuby = "uninstallRuby";
                public const string uninstallDevKit = "uninstallDevKit";

            }

            private bool _uninstallRuby;
            private bool _uninstallDevKit;

            public bool UninstallRuby { get { return _uninstallRuby; } }
            public bool UninstallDevKit { get { return _uninstallDevKit; } }

            public UninstallArguments(InstallContext context)
            {
                _uninstallRuby = context.IsParameterTrue(Argument.uninstallRuby);
                _uninstallDevKit = context.IsParameterTrue(Argument.uninstallDevKit);
            }
        }
    }
}
