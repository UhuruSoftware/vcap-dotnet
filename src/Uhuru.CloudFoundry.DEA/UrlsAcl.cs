using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    class UrlsAcl
    {
        public static void AddPortAccess(int port, string Username)
        {
            string command = String.Format("netsh http add urlacl url=http://*:{0}/ user={1} listen=yes delegate=no", port.ToString(), Username);

            Logger.Debug("Adding url acl with the following command: {0}", command);

            int ret = DEAUtilities.ExecuteCommand(command);

            if (ret != 0)
            {
                throw new Exception("netsh http add urlacl command failed.");
            }
        }

        public static void RemovePortAccess(int port, string Username)
        {
            string command = String.Format("netsh http delete urlacl url=http://*:{0}/", port.ToString());

            Logger.Debug("Removing url acl with the following command: {0}", command);

            int ret = DEAUtilities.ExecuteCommand(command);

            if (ret != 0)
            {
                throw new Exception("netsh http delete urlacl command failed.");
            }
        }

    }
}
