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
                string[] result;
                ProcessPrison prison = new ProcessPrison();

                var ppci = new ProcessPrisonCreateInfo();
                ppci.TotalPrivateMemoryLimit = 0;
                ppci.DiskQuotaBytes = 0;
                ppci.PrisonHomePath = @"C:\Users\SpyUser";
                ppci.NetworkOutboundRateLimitBitsPerSecond = 0;
                prison.Create(ppci);

                using (new UserImpersonator(prison.WindowsUsername, ".", prison.WindowsPassword, false))
                {
                    result = GetOpenDirectories(new DirectoryInfo(@"c:\")).ToArray();
                }

                prison.Destroy();

                openDirs = result;
            }
        }

        private static List<string> GetOpenDirectories(System.IO.DirectoryInfo root)
        {
            List<string> result = new List<string>();
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder 
            try
            {
                string adir = Guid.NewGuid().ToString("N");
                Directory.CreateDirectory(Path.Combine(root.FullName, adir));
                result.Add(root.FullName);
                Directory.Delete(Path.Combine(root.FullName, adir));
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
                    result.AddRange(GetOpenDirectories(dirInfo));
                }
            }
            catch { }

            return result;
        }
    }
}
