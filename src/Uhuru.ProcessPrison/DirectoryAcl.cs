using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.Isolation
{
    public class DirectoryAcl
    {
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
    }
}
