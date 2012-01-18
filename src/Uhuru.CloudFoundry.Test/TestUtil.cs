using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Xml;
using System.Threading;

namespace Uhuru.CloudFoundry.Test
{
    class TestUtil
    {
        public static string CopyFolderToTemp(string folder)
        {
            string tempFolder = Path.GetTempPath();
            string targetPath = Path.Combine(tempFolder, Guid.NewGuid().ToString());

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            if (Directory.Exists(folder))
            {
                DirectoryInfo source = new DirectoryInfo(folder);
                DirectoryInfo target = new DirectoryInfo(targetPath);

                CopyAll(source, target);
            }

            return targetPath;
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static string GetLocalIp()
        {
            IPHostEntry ipHostEntry;
            string localIP = string.Empty;
            ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in ipHostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            return localIP;
        }

        public static bool TestUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            // Try 5 times to get the URL
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(2000);
                request.AllowAutoRedirect = false;
                try
                {
                    request.GetResponse();
                }
                catch (WebException)
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        public static void UpdateWebConfigKey(string fileName, string key, string newValue)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");

            // Attempt to locate the requested setting.
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == key)
                {
                    childNode.Attributes["value"].Value = newValue;
                    break;
                }
            }
            xmlDoc.Save(fileName);
        }

        public static string GenerateAppName()
        {
            return Guid.NewGuid().ToString().Substring(0, 6);
        }
    }
}
