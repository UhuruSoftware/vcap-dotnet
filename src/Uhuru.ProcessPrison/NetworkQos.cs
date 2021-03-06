﻿using System;
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
        public static void CreateOutboundThrottlePolicy(string ruleName, string windowsUsername, long bitsPerSecond)
        {
            var StandardCimv2 = new ManagementScope(@"root\StandardCimv2");

            using (ManagementClass netqos = new ManagementClass("MSFT_NetQosPolicySettingData"))
            {
                netqos.Scope = StandardCimv2;

                using (ManagementObject newInstance = netqos.CreateInstance())
                {
                    newInstance["Name"] = ruleName;
                    newInstance["UserMatchCondition"] = windowsUsername;

                    // ThrottleRateAction is in bytesPerSecond according to the WMI docs.
                    // Acctualy the units are bits per second, as documented in the PowerShell cmdlet counterpart.
                    newInstance["ThrottleRateAction"] = bitsPerSecond;

                    newInstance.Put();
                }
            }
        }

        /// <summary>
        /// Sets the limit for the upload network data rate. This limit is applied for a specific server URL passing through HTTP.sys.
        /// This rules are applicable to IIS, IIS WHC and IIS Express. This goes hand in hand with URL Acls.
        /// This method is not reentrant. Remove the policy first after creating it again.
        /// </summary>
        public static void CreateOutboundThrottlePolicy(string ruleName, int urlPort, long bitsPerSecond)
        {
            var StandardCimv2 = new ManagementScope(@"root\StandardCimv2");

            using (ManagementClass netqos = new ManagementClass("MSFT_NetQosPolicySettingData"))
            {
                netqos.Scope = StandardCimv2;

                using (ManagementObject newInstance = netqos.CreateInstance())
                {
                    newInstance["Name"] = ruleName;
                    newInstance["URIMatchCondition"] = String.Format("http://*:{0}/", urlPort);
                    newInstance["URIRecursiveMatchCondition"] = true;
                    
                    // ThrottleRateAction is in bytesPerSecond according to the WMI docs.
                    // Acctualy the units are bits per second, as documented in the PowerShell cmdlet counterpart.
                    newInstance["ThrottleRateAction"] = bitsPerSecond;

                    newInstance.Put();
                }
            }
        }

        public static void RemoveOutboundThrottlePolicy(string ruleName)
        {
            var wql = string.Format("SELECT * FROM MSFT_NetQosPolicySettingData WHERE Name = \"{0}\"", ruleName);
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\StandardCimv2", wql))
            {
                // should only iterate once
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    queryObj.Delete();
                    queryObj.Dispose();
                }
            }
        }
    }
}
