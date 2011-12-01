using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Xml;
using System.IO;
using System.Text;
using System.Net;


namespace CloudFoundry.Net.Services.DEA
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            InstallArguments arguments = new InstallArguments(this.Context);
            string uhuruConfig = Path.Combine(arguments.TargetDir, "uhuru.config");
            StreamReader reader = new StreamReader(uhuruConfig);
            StringBuilder sb = new StringBuilder();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string newLine = line;
                if (newLine.Contains('='))
                {
                    switch (line.Substring(0, line.IndexOf('=')).Trim())
                    {
                        case "baseDir":
                            {
                                newLine = line.Substring(0, line.IndexOf('=') + 1) + "\"" + arguments.DropletPath + "\"";
                                if (!Directory.Exists(arguments.DropletPath))
                                {
                                    Directory.CreateDirectory(arguments.DropletPath);
                                }
                                break;
                            }
                        case "localRoute":
                            {
                                string ip = "";
                                foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                                {
                                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                    {
                                        ip = address.ToString();
                                    }
                                }
                                newLine = line.Substring(0, line.IndexOf('=') + 1) + "\"" + ip + "\"";
                                break;
                            }
                        case "messageBus":
                            {
                                if (arguments.CloudControllerIp != "")
                                {
                                    newLine = line.Substring(0, line.IndexOf('=') + 1) + "\"nats://" + arguments.CloudControllerUserName + ":"
                                        + arguments.CloudControllerPassword + "@" + arguments.CloudControllerIp + ":" + arguments.CloudControllerPort + "/" + "\"";
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
            StreamWriter writer = new StreamWriter(uhuruConfig);
            writer.Write(sb.ToString());
            writer.Flush();
            writer.Close();
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
                public const string dropletPath = "dropletPath";
                public const string targetDir = "TARGETDIR";
                public const string cloudControllerIp = "cloudControllerIp";
                public const string cloudControllerPort = "cloudControllerPort";
                public const string cloudControllerUserName = "cloudControllerUserName";
                public const string cloucControllerPassword = "cloudControllerPassword";
            }

            private string _dropletPath;
            private string _targetDir;
            private string _cloudControllerIp;
            private string _cloudControllerPort;
            private string _cloudControllerUserName;
            private string _cloudControllerPassword;

            public string DropletPath { get { return _dropletPath; } }
            public string TargetDir { get { return _targetDir; } }
            public string CloudControllerIp { get { return _cloudControllerIp; } }
            public string CloudControllerPort { get { return _cloudControllerPort; } }
            public string CloudControllerUserName { get { return _cloudControllerUserName; } }
            public string CloudControllerPassword { get { return _cloudControllerPassword; } }

            public InstallArguments(InstallContext context)
            {
                _targetDir = context.Parameters[Argument.targetDir].TrimEnd('\\');

                if (context.Parameters[Argument.dropletPath] != "")
                    _dropletPath = context.Parameters[Argument.dropletPath].TrimEnd('\\');
                else
                    _dropletPath = Path.Combine(_targetDir, "droplets");
                _cloudControllerPort = context.Parameters[Argument.cloudControllerPort];
                _cloudControllerIp = context.Parameters[Argument.cloudControllerIp];
                if (context.Parameters[Argument.cloudControllerUserName] != "")
                    _cloudControllerUserName = context.Parameters[Argument.cloudControllerUserName];
                else
                    _cloudControllerUserName = "nats";
                if (context.Parameters[Argument.cloucControllerPassword] != "")
                    _cloudControllerPassword = context.Parameters[Argument.cloucControllerPassword];
                else
                    _cloudControllerPassword = "nats";
                if (!Directory.Exists(_dropletPath))
                    Directory.CreateDirectory(_dropletPath);
            }
        }
    }
}
