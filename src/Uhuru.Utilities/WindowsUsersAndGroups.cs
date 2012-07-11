// -----------------------------------------------------------------------
// <copyright file="WindowsUsersAndGroups.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This is a helper class for managing Windows Users and Groups.
    /// </summary>
    public static class WindowsUsersAndGroups
    {
        /// <summary>
        /// Creates a Windows user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public static void CreateUser(string userName, string password)
        {
            CreateUser(userName, password, null);
        }

        /// <summary>
        /// Creates a Windows user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="description">The description for the user.</param>
        public static void CreateUser(string userName, string password, string description)
        {
            using (DirectoryEntry localEntry = new DirectoryEntry("WinNT://.,Computer"))
            {
                DirectoryEntries localChildren = localEntry.Children;
                using (DirectoryEntry newUser = localChildren.Add(userName, "User"))
                {
                    if (!string.IsNullOrEmpty(description))
                    {
                        newUser.Properties["Description"].Add(description);
                    }

                    newUser.Invoke("SetPassword", password);
                    newUser.CommitChanges();
                }
            }
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        public static void DeleteUser(string userName)
        {
            using (DirectoryEntry localEntry = new DirectoryEntry("WinNT://.,Computer"))
            {
                DirectoryEntries localChildren = localEntry.Children;
                using (DirectoryEntry userEntry = localChildren.Find(userName, "User"))
                {
                    localChildren.Remove(userEntry);
                }
            }
        }

        /// <summary>
        /// Creates a Windows group.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        public static void CreateGroup(string groupName)
        {
            CreateGroup(groupName, null);
        }

        /// <summary>
        /// Creates a Windows group.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="description">The description for the group.</param>
        public static void CreateGroup(string groupName, string description)
        {
            using (DirectoryEntry localEntry = new DirectoryEntry("WinNT://.,Computer"))
            {
                using (DirectoryEntry newGroup = localEntry.Children.Add(groupName, "Group"))
                {
                    if (!string.IsNullOrEmpty(description))
                    {
                        newGroup.Properties["Description"].Add(description);
                    }

                    newGroup.CommitChanges();
                }
            }
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        public static void DeleteGroup(string groupName)
        {
            using (DirectoryEntry localEntry = new DirectoryEntry("WinNT://.,Computer"))
            {
                DirectoryEntries localChildren = localEntry.Children;
                using (DirectoryEntry groupEntry = localChildren.Find(groupName, "Group"))
                {
                    localChildren.Remove(groupEntry);
                }
            }
        }

        /// <summary>
        /// Adds a user to a group.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="groupName">Name of the group.</param>
        public static void AddUserToGroup(string userName, string groupName)
        {
            using (DirectoryEntry groupEntry = new DirectoryEntry("WinNT://./" + groupName + ",Group"))
            {
                groupEntry.Invoke("Add", new object[] { "WinNT://" + userName + ",User" });
                groupEntry.CommitChanges();
            }
        }

        /// <summary>
        /// Removes a user from a group.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="groupName">Name of the group.</param>
        public static void RemoveUserFromGroup(string userName, string groupName)
        {
            using (DirectoryEntry groupEntry = new DirectoryEntry("WinNT://./" + groupName + ",Group"))
            {
                groupEntry.Invoke("Remove", new object[] { "WinNT://" + userName + ",User" });
                groupEntry.CommitChanges();
            }
        }

        /// <summary>
        /// Determines whether [is user member of group] [the specified user name].
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>
        ///   <c>true</c> if [is user member of group] [the specified user name]; otherwise, <c>false</c>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Returns false for any exception.")]
        public static bool IsUserMemberOfGroup(string userName, string groupName)
        {
            using (DirectoryEntry groupEntry = new DirectoryEntry("WinNT://./" + groupName + ",Group"))
            {
                try
                {
                    return (bool)groupEntry.Invoke("IsMember", new object[] { "WinNT://" + Environment.MachineName + "/" + userName });
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
