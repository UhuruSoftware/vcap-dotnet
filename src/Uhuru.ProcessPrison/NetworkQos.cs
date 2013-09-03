using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
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
            var StandardCimv2 = new ManagementScope(@"root\StandardCimv2");

            using (ManagementClass netqos = new ManagementClass("MSFT_NetQosPolicySettingData"))
            {
                netqos.Scope = StandardCimv2;

                using (ManagementObject newInstance = netqos.CreateInstance())
                {
                    newInstance["Name"] = windowsUsername;
                    newInstance["UserMatchCondition"] = windowsUsername;

                    // ThrottleRateAction is in bytesPerSecond according to the WMI docs.
                    // Acctualy the units are bits per second, as documented in the PowerShell cmdlet counterpart.
                    newInstance["ThrottleRateAction"] = bitsPerSecond;

                    newInstance.Put();
                }
            }

            //return;

            //string command = String.Format("powershell  -ExecutionPolicy bypass  -Command  New-NetQosPolicy -name {0} -UserMatchCondition {0} -ThrottleRateActionBitsPerSecond {1}", windowsUsername, bitsPerSecond);
            //var ret = Command.ExecuteCommand(command);

            //if (ret != 0)
            //{
            //    throw new Exception("New-NetQosPolicy command failed.");
            //}
        }

        public static void RemoveOutboundThrottlePolicy(string windowsUsername)
        {
            var wql = string.Format("SELECT * FROM MSFT_NetQosPolicySettingData WHERE Name = \"{0}\"", windowsUsername);
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\StandardCimv2", wql))
            {
                // should only iterate once
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    queryObj.Delete();
                    queryObj.Dispose();
                }
            }

            return;

            //string command = String.Format("powershell  -ExecutionPolicy bypass  -Command  Remove-NetQosPolicy -Name {0} -Confirm:$false", windowsUsername);
            //var ret = Command.ExecuteCommand(command);

            //if (ret != 0)
            //{
            //    throw new Exception("Remove-NetQosPolicy command failed.");
            //}
        }
    }
}
