using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.Isolation
{
    class NetworkQos
    {
        /// <summary>
        /// Sets the limit for the upload network data rate. This limit is applied for the specified user.
        /// This method is not reentrant. Remove the policy first after creating it again.
        /// </summary>
        /// <param name="windowsUsername"></param>
        /// <param name="bitsPerSecond"></param>
        public static void CreateOutboundThrottlePolicy(string windowsUsername, long bitsPerSecond)
        {
            string command = String.Format("powershell  -ExecutionPolicy bypass  -Command  New-NetQosPolicy -name {0} -UserMatchCondition {0} -ThrottleRateActionBitsPerSecond {1}", windowsUsername, bitsPerSecond);
            var ret = Command.ExecuteCommand(command);

            if (ret != 0)
            {
                throw new Exception("New-NetQosPolicy command failed.");
            }
        }

        public static void RemoveOutboundThrottlePolicy(string windowsUsername)
        {
            string command = String.Format("powershell  -ExecutionPolicy bypass  -Command  Remove-NetQosPolicy -Name {0} -Confirm:$false", windowsUsername);
            var ret = Command.ExecuteCommand(command);

            if (ret != 0)
            {
                throw new Exception("Remove-NetQosPolicy command failed.");
            }
        }
    }
}
