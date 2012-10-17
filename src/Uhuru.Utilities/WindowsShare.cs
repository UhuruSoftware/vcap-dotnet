// -----------------------------------------------------------------------
// <copyright file="WindowsShare.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Text;

    /// <summary>
    /// Windows Share utility class.
    /// </summary>
    public class WindowsShare
    {
        /// <summary>
        /// The name of the share.
        /// </summary>
        private string shareName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsShare"/> class.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        public WindowsShare(string shareName)
        {
            this.shareName = shareName;
        }

        /// <summary>
        /// Creates the share.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>WindwsShare instance.</returns>
        public static WindowsShare CreateShare(string shareName, string folderPath)
        {
            ManagementClass shareClass = null;
            ManagementClass sd = null;
            ManagementBaseObject inParams = null;
            ManagementBaseObject outParams = null;

            try
            {
                sd = new ManagementClass(new ManagementPath("Win32_SecurityDescriptor"), null);

                sd["ControlFlags"] = 0x4;
                sd["DACL"] = new ManagementBaseObject[] { };

                shareClass = new ManagementClass("Win32_Share");

                inParams = shareClass.GetMethodParameters("Create");
                inParams["Name"] = shareName;
                inParams["Path"] = new DirectoryInfo(folderPath).FullName;
                //// inParams["Description"] = description;
                inParams["Type"] = 0x0;  // Type of Disk Drive
                inParams["Access"] = sd;

                outParams = shareClass.InvokeMethod("Create", inParams, null);

                if ((uint)outParams["ReturnValue"] != 0)
                {
                    throw new WindowsShareException("Unable to create share. Win32_Share.Create Error Code: " + outParams["ReturnValue"]);
                }
            }
            catch (Exception ex)
            {
                throw new WindowsShareException("Unable to create share", ex);
            }
            finally
            {
                if (shareClass != null)
                {
                    shareClass.Dispose();
                }

                if (inParams != null)
                {
                    inParams.Dispose();
                }

                if (outParams != null)
                {
                    outParams.Dispose();
                }

                if (sd != null)
                {
                    sd.Dispose();
                }
            }

            return new WindowsShare(shareName);
        }

        /// <summary>
        /// Gets the share by name.
        /// </summary>
        /// <param name="shareName">Name of the share.</param>
        /// <returns>The WindowsShare instance.</returns>
        public static WindowsShare GetShare(string shareName)
        {
            return new WindowsShare(shareName);
        }

        /// <summary>
        /// Gets the shares.
        /// </summary>
        /// <returns>The WindowsShare instances.</returns>
        public static WindowsShare[] GetShares()
        {
            ManagementObjectSearcher searcher = null;

            List<WindowsShare> ret = new List<WindowsShare>();

            try
            {
                searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Share");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    ret.Add(new WindowsShare((string)queryObj["Name"]));
                }
            }
            catch (Exception ex)
            {
                throw new WindowsShareException("Unable to get shares", ex);
            }
            finally
            {
                if (searcher != null)
                {
                    searcher.Dispose();
                }
            }

            return ret.ToArray();
        }

        /// <summary>
        /// Deletes the share.
        /// </summary>
        public void DeleteShare()
        {
            ManagementBaseObject outParams = null;
            ManagementObject shareInstance = null;

            try
            {
                shareInstance = new ManagementObject(@"root\cimv2:Win32_Share.Name='" + this.shareName + "'");
                outParams = shareInstance.InvokeMethod("Delete", null, null);

                if ((uint)outParams["ReturnValue"] != 0)
                {
                    throw new WindowsShareException("Unable to delete share. Win32_Share.Delete Error Code: " + outParams["ReturnValue"]);
                }
            }
            catch (Exception ex)
            {
                throw new WindowsShareException("Unable to delete share: " + this.shareName, ex);
            }
            finally
            {
                if (shareInstance != null)
                {
                    shareInstance.Dispose();
                }

                if (outParams != null)
                {
                    outParams.Dispose();
                }
            }
        }

        /// <summary>
        /// Test if the share exists.
        /// </summary>
        /// <returns>True if the share exists.</returns>
        public bool Exists()
        {
            using (var shareQuery = new ManagementObjectSearcher(@"SELECT * FROM Win32_Share Where Name = '" + this.shareName + "'"))
            {
                return shareQuery.Get().Count > 0;
            }
        }

        /// <summary>
        /// Adds the share permissions.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        public void AddSharePermissions(string accountName)
        {
            ManagementObject trustee = null;
            ManagementObject ace = null;
            ManagementObject win32LogicalSecuritySetting = null;
            ManagementObject share = null;
            ManagementBaseObject getSecurityDescriptorReturn = null;
            ManagementBaseObject securityDescriptor = null;

            try
            {
                //// Not necessary
                //// NTAccount ntAccount = new NTAccount(accountName);
                //// SecurityIdentifier sid = (SecurityIdentifier)ntAccount.Translate(typeof(SecurityIdentifier));
                //// byte[] sidArray = new byte[sid.BinaryLength];
                //// sid.GetBinaryForm(sidArray, 0);

                trustee = new ManagementClass(new ManagementPath("Win32_Trustee"), null);
                trustee["Name"] = accountName;
                //// trustee["SID"] = sidArray;

                ace = new ManagementClass(new ManagementPath("Win32_Ace"), null);
                //// Permissions mask http://msdn.microsoft.com/en-us/library/windows/desktop/aa394186(v=vs.85).aspx
                ace["AccessMask"] = 0x1F01FF;
                //// ace["AccessMask"] = 0x1FF;
                ace["AceFlags"] = 3;
                ace["AceType"] = 0;
                ace["Trustee"] = trustee;

                win32LogicalSecuritySetting = new ManagementObject(@"root\cimv2:Win32_LogicalShareSecuritySetting.Name='" + this.shareName + "'");

                getSecurityDescriptorReturn = win32LogicalSecuritySetting.InvokeMethod("GetSecurityDescriptor", null, null);

                if ((uint)getSecurityDescriptorReturn["ReturnValue"] != 0)
                {
                    throw new WindowsShareException("Unable to add share permission. Error Code: " + getSecurityDescriptorReturn["ReturnValue"]);
                }

                securityDescriptor = getSecurityDescriptorReturn["Descriptor"] as ManagementBaseObject;
                ManagementBaseObject[] dacl = securityDescriptor["DACL"] as ManagementBaseObject[];

                if (dacl == null)
                {
                    dacl = new ManagementBaseObject[] { ace };
                }
                else
                {
                    Array.Resize(ref dacl, dacl.Length + 1);
                    dacl[dacl.Length - 1] = ace;
                }

                securityDescriptor["DACL"] = dacl;

                share = new ManagementObject(@"root\cimv2:Win32_Share.Name='" + this.shareName + "'");
                uint setShareInfoReturn = (uint)share.InvokeMethod("SetShareInfo", new object[] { null, null, securityDescriptor });

                if (setShareInfoReturn != 0)
                {
                    throw new WindowsShareException("Unable to add share permission. Error code: " + setShareInfoReturn.ToString(CultureInfo.CurrentCulture));
                }
            }
            catch (Exception ex)
            {
                throw new WindowsShareException("Unable to add share permission", ex);
            }
            finally
            {
                if (trustee != null)
                {
                    trustee.Dispose();
                }

                if (ace != null)
                {
                    ace.Dispose();
                }

                if (win32LogicalSecuritySetting != null)
                {
                    win32LogicalSecuritySetting.Dispose();
                }

                if (getSecurityDescriptorReturn != null)
                {
                    getSecurityDescriptorReturn.Dispose();
                }

                if (securityDescriptor != null)
                {
                    securityDescriptor.Dispose();
                }

                if (share != null)
                {
                    share.Dispose();
                }
            }
        }

        /// <summary>
        /// Deletes the share permission.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        public void DeleteSharePermission(string accountName)
        {
            if (accountName == null)
            {
                throw new ArgumentNullException("accountName");
            }

            ManagementObject win32LogicalSecuritySetting = null;
            ManagementBaseObject getSecurityDescriptorReturn = null;
            ManagementBaseObject securityDescriptor = null;
            ManagementObject share = null;

            try
            {
                win32LogicalSecuritySetting = new ManagementObject(@"root\cimv2:Win32_LogicalShareSecuritySetting.Name='" + this.shareName + "'");

                getSecurityDescriptorReturn = win32LogicalSecuritySetting.InvokeMethod("GetSecurityDescriptor", null, null);

                if ((uint)getSecurityDescriptorReturn["ReturnValue"] != 0)
                {
                    throw new WindowsShareException("Unable to delete share permission. Error Code: " + getSecurityDescriptorReturn["ReturnValue"]);
                }

                securityDescriptor = getSecurityDescriptorReturn["Descriptor"] as ManagementBaseObject;
                ManagementBaseObject[] dacl = securityDescriptor["DACL"] as ManagementBaseObject[];

                if (dacl == null)
                {
                    throw new WindowsShareException("Unable to delete share permission. Access control not found");
                }
                else
                {
                    List<ManagementBaseObject> newDACL = new List<ManagementBaseObject>();
                    foreach (ManagementBaseObject ac in dacl)
                    {
                        if (((ac["Trustee"] as ManagementBaseObject)["Name"] as string).ToUpperInvariant() != accountName.ToUpperInvariant())
                        {
                            newDACL.Add(ac);
                        }
                    }

                    if (dacl.Count() == newDACL.Count())
                    {
                        throw new WindowsShareException("Unable to delete share permission. Access control not found");
                    }

                    dacl = newDACL.ToArray();
                }

                securityDescriptor["DACL"] = dacl;

                share = new ManagementObject(@"root\cimv2:Win32_Share.Name='" + this.shareName + "'");
                uint setShareInfoReturn = (uint)share.InvokeMethod("SetShareInfo", new object[] { null, null, securityDescriptor });

                if (setShareInfoReturn != 0)
                {
                    throw new WindowsShareException("Unable to delete share permission. Error code:" + setShareInfoReturn.ToString(CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                throw new WindowsShareException("Unable to delete share permission", ex);
            }
            finally
            {
                if (win32LogicalSecuritySetting != null)
                {
                    win32LogicalSecuritySetting.Dispose();
                }

                if (getSecurityDescriptorReturn != null)
                {
                    getSecurityDescriptorReturn.Dispose();
                }

                if (securityDescriptor != null)
                {
                    securityDescriptor.Dispose();
                }

                if (share != null)
                {
                    share.Dispose();
                }
            }
        }

        /*
        public void SetSharePermissions()
        {
        }
        */

        /// <summary>
        /// Gets the share permissions.
        /// </summary>
        /// <returns>The Names of the Trustees.</returns>
        public string[] GetSharePermissions()
        {
            ManagementObject win32LogicalSecuritySetting = null;
            ManagementBaseObject getSecurityDescriptorReturn = null;
            ManagementBaseObject securityDescriptor = null;

            try
            {
                win32LogicalSecuritySetting = new ManagementObject(@"root\cimv2:Win32_LogicalShareSecuritySetting.Name='" + this.shareName + "'");

                getSecurityDescriptorReturn = win32LogicalSecuritySetting.InvokeMethod("GetSecurityDescriptor", null, null);

                if ((uint)getSecurityDescriptorReturn["ReturnValue"] != 0)
                {
                    throw new WindowsShareException("Unable to get share permissions. Error Code: " + getSecurityDescriptorReturn["ReturnValue"]);
                }

                securityDescriptor = getSecurityDescriptorReturn["Descriptor"] as ManagementBaseObject;
                ManagementBaseObject[] dacl = securityDescriptor["DACL"] as ManagementBaseObject[];

                if (dacl == null)
                {
                    return new string[] { };
                }
                else
                {
                    return dacl.Select(ac => ((ac["Trustee"] as ManagementBaseObject)["Name"] as string)).ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new WindowsShareException("Unable to get share permissions", ex);
            }
            finally
            {
                if (win32LogicalSecuritySetting != null)
                {
                    win32LogicalSecuritySetting.Dispose();
                }

                if (getSecurityDescriptorReturn != null)
                {
                    getSecurityDescriptorReturn.Dispose();
                }

                if (securityDescriptor != null)
                {
                    securityDescriptor.Dispose();
                }
            }
        }
    }
}
