using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.Isolation
{
    public class DirectoryAcl
    {
        private readonly static object openDirLock = new object();
        private static string[] openDirs = new string[0];

        public static string[] OpenDirs
        {
            get
            {
                return openDirs;
            }
        }

        public static void TakeOwnership(string user, string directory)
        {
            string command = string.Format(@"takeown /R /D Y /S localhost /U {0} /F ""{1}""", user, directory);

            int ret = Command.ExecuteCommand(command);

            if (ret != 0)
            {
                throw new Exception(@"take ownership failed.");
            }
        }

        public static void AddCreateSubdirDenyRule(string user, string directory, bool recursive = false)
        {
            string command = string.Format(@"icacls ""{0}"" /deny {1}:(AD) /c{2}", directory.Replace("\\", "/"), user, recursive ? " /t" : string.Empty);

            int ret = Command.ExecuteCommand(command);

            if (ret != 0)
            {
                throw new Exception(@"icacls command denying subdir creation failed.");
            }
        }

        public static void AddCreateFileDenyRule(string user, string directory, bool recursive = false)
        {
            string command = string.Format(@"icacls ""{0}"" /deny {1}:(W) /c{2}", directory.Replace("\\", "/"), user, recursive ? " /t" : string.Empty);
            int ret = Command.ExecuteCommand(command);

            if (ret != 0)
            {
                throw new Exception(@"icacls command denying file creation failed.");
            }
        }

        public static void InitOpenDirectoriesList()
        {
            lock (openDirLock)
            {
                string[] result = new string[0];
                ProcessPrison prison = new ProcessPrison();

                var ppci = new ProcessPrisonCreateInfo();
                ppci.TotalPrivateMemoryLimit = 128 * 1024 * 1024;
                ppci.DiskQuotaBytes = -1;
                ppci.PrisonHomePath = @"C:\Users\SpyUser";
                ppci.NetworkOutboundRateLimitBitsPerSecond = 0;
                prison.Create(ppci);

                using (new UserImpersonator(prison.WindowsUsername, ".", prison.WindowsPassword, true))
                {
                    result = GetOpenDirectories(new DirectoryInfo(@"c:\")).ToArray();
                }

                prison.Destroy();

                openDirs = result;
            }
        }

        private static HashSet<string> GetOpenDirectories(System.IO.DirectoryInfo root)
        {
            HashSet<string> result = new HashSet<string>();
            System.IO.DirectoryInfo[] subDirs = null;

            if (root.Name.StartsWith("uhurusec_"))
            {
                result.Add(root.FullName);
                return result;
            }

            try
            {
                string adir = string.Format("uhurusec_{0}", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(Path.Combine(root.FullName, adir));
                result.Add(root.FullName);
                Directory.Delete(Path.Combine(root.FullName, adir));
            }
            catch
            {
            }

            try
            {
                    string adir = string.Format("uhurusec_{0}", Guid.NewGuid().ToString("N"));
                File.WriteAllText(Path.Combine(root.FullName, adir + ".txt"), "test");
                result.Add(root.FullName);
                File.Delete(Path.Combine(root.FullName, adir + ".txt"));
            }
            catch
            {
            }

            try
            {
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    foreach (string subdir in GetOpenDirectories(dirInfo))
                    {
                        result.Add(subdir);
                    }
                }
            }
            catch { }

            return result;
        }
    }
}
