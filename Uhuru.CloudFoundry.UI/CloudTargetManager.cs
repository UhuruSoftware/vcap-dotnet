using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using Microsoft.Win32;

namespace Uhuru.CloudFoundry.UI
{
    public class CloudTargetManager
    {
        public CloudTarget[] GetTargets()
        {
            Dictionary<string, string[]> targets = GetValuesFromCloudTargets();
            List<CloudTarget> result = new List<CloudTarget>(targets.Count);

            foreach (string key in targets.Keys)
            {
                Guid targetId = new Guid(key);
                string targetUrl = targets[key][0];
                string username = targets[key][1];
                string encryptedPassword = targets[key][2];

                CloudTarget newTarget = new CloudTarget(username, encryptedPassword, targetUrl, targetId);

                result.Add(newTarget);
            }

            return result.ToArray();
        }

        public void SaveTarget(CloudTarget target)
        {
            if (GetValuesFromCloudTargets().ContainsKey(target.TargetId.ToString()))
            {
                throw new InvalidOperationException("Specified target ID already exists in the collection!");
            }

            AddValueToCloudTargets(target.TargetId.ToString(), new string[] {
                target.TargetUrl,
                target.Username,
                target.EncryptedPassword});
        }

        public void RemoveTarget(CloudTarget target)
        {
            if (!GetValuesFromCloudTargets().ContainsKey(target.TargetId.ToString()))
            {
                throw new InvalidOperationException("Specified target ID does not exist!");
            }

            RemoveValueFromCloudTargets(target.TargetId.ToString());
        }

        private RegistryKey SetupUhuruKey()
        {
            RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey uhuruSubKey = softwareKey.OpenSubKey("Uhuru", true);
            if (uhuruSubKey == null)
            {
                softwareKey.CreateSubKey("Uhuru");
                uhuruSubKey = softwareKey.OpenSubKey("Uhuru", true);
            }

            RegistryKey uhuruCloudTargetsSubKey = uhuruSubKey.OpenSubKey("CloudTargets", true);
            if (uhuruCloudTargetsSubKey == null)
            {
                uhuruSubKey.CreateSubKey("CloudTargets");
                uhuruCloudTargetsSubKey = uhuruSubKey.OpenSubKey("CloudTargets", true);
            }
            return uhuruCloudTargetsSubKey;
        }

        private void AddValueToCloudTargets(string valueName, string[] value)
        {
            RegistryKey cloudTargets = SetupUhuruKey();
            cloudTargets.SetValue(valueName, value, RegistryValueKind.MultiString);
        }

        private void RemoveValueFromCloudTargets(string valueName)
        {
            RegistryKey cloudTargets = SetupUhuruKey();
            cloudTargets.DeleteValue(valueName);
        }

        private Dictionary<string, string[]> GetValuesFromCloudTargets()
        {
            RegistryKey cloudTargets = SetupUhuruKey();
            string[] allValues = cloudTargets.GetValueNames();
            Dictionary<string, string[]> result = new Dictionary<string, string[]>(allValues.Length);
            foreach (string value in allValues)
            {
                string[] strings = (string[])cloudTargets.GetValue(value, null);
                if (strings != null)
                {
                    result.Add(value, strings);
                }
            }
            return result;
        }
    }
}
