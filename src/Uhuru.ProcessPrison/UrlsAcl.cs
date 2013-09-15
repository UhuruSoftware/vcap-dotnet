using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.Isolation
{
    public class UrlsAcl
    {
        /// <summary>
        /// Allow access to the URL with the specified port for the specified username.
        /// This will allow IIS HWC and IIS Express to bind and listen to that port.
        /// </summary>
        /// <param name="port">Http port number.</param>
        /// <param name="Username">Windows Local username.</param>
        public static void AddPortAccess(int port, string Username)
        {
            string command = String.Format("netsh http add urlacl url=http://*:{0}/ user={1} listen=yes delegate=no", port.ToString(), Username);

            Logger.Debug("Adding url acl with the following command: {0}", command);

            int ret = Command.ExecuteCommand(command);

            if (ret != 0)
            {
                throw new Exception("netsh http add urlacl command failed.");
            }
        }

        /// <summary>
        /// Remove access for the specified port.
        /// </summary>
        /// <param name="port">Http port number.</param>
        public static void RemovePortAccess(int port, bool ignoreFailure = false)
        {
            string command = String.Format("netsh http delete urlacl url=http://*:{0}/", port.ToString());

            Logger.Debug("Removing url acl with the following command: {0}", command);

            int ret = Command.ExecuteCommand(command);

            if (ret != 0 && !ignoreFailure)
            {
                throw new Exception("netsh http delete urlacl command failed.");
            }
        }
    }
}
