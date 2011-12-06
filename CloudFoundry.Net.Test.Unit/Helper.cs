using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace CloudFoundry.Net.Test.Unit
{
    class Helper
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

        public static int GetEphemeralPort()
        {
            TcpListener socket = new TcpListener(IPAddress.Any, 0);
            socket.Start();
            int port = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();
            return port;
        }
    }
}
