// -----------------------------------------------------------------------
// <copyright file="WindowsVcapUsers.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.DirectoryServices;

    /// <summary>
    /// This is a helper class for creating Windows Users.
    /// </summary>
    public static class WindowsVCAPUsers
    {
        /// <summary>
        /// A prefix that is appended to all created Windows users.
        /// </summary>
        private const string UserDecoration = "UhuruVcap_";

        /// <summary>
        /// Creates a user based on an id. The created user has a prefix added to it.
        /// </summary>
        /// <param name="id">An id for the username.</param>
        /// <param name="password">A password for the user. Make sure it's strong.</param>
        /// <returns>The final username of the newly created Windows User.</returns>
        public static string CreateDecoratedUser(string id, string password)
        {
            if (password == null)
            {
                password = Utilities.Credentials.GenerateCredential();
            }

            string decoratedUsername = DecorateUser(id);
            using (DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString()))
            {
                DirectoryEntries entries = directoryEntry.Children;
                DirectoryEntry user = entries.Add(decoratedUsername, "User");
                user.Properties["FullName"].Add("Uhuru Vcap Instance " + id + " user");
                user.Invoke("SetPassword", password);
                user.CommitChanges();
            }

            return decoratedUsername;
        }

        /// <summary>
        /// Creates a user based on an id. The created user has a prefix added to it.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="password">A password for the user. Make sure it's strong.</param>
        public static void CreateUser(string userName, string password)
        {
            if (password == null)
            {
                password = Utilities.Credentials.GenerateCredential();
            }

            string decoratedUsername = userName;
            using (DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString()))
            {
                DirectoryEntries entries = directoryEntry.Children;
                DirectoryEntry user = entries.Add(decoratedUsername, "User");
                user.Properties["FullName"].Add("Uhuru Vcap Instance " + userName + " user");
                user.Invoke("SetPassword", password);
                user.CommitChanges();
            }
        }

        /// <summary>
        /// Deletes a windows user based on an Id.
        /// </summary>
        /// <param name="id">The id that was used to create the user.</param>
        public static void DeleteDecoratedBasedUser(string id)
        {
            string decoratedUsername = DecorateUser(id);
            using (DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString()))
            {
                DirectoryEntries users = localDirectory.Children;
                DirectoryEntry user = users.Find(decoratedUsername);
                users.Remove(user);
            }
        }

        /// <summary>
        /// Deletes a windows user based on an Id.
        /// </summary>
        /// <param name="userName">The username.</param>
        public static void DeleteUser(string userName)
        {
            string decoratedUsername = userName;
            using (DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString()))
            {
                DirectoryEntries users = localDirectory.Children;
                DirectoryEntry user = users.Find(decoratedUsername);
                users.Remove(user);
            }
        }

        /// <summary>
        /// Returns a string that is unique for a given user.
        /// </summary>
        /// <param name="id"> The id of the user. </param>
        /// <returns> The unique string.</returns>
        public static string DecorateUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id paramater cannot be null or empty", "id");
            }

            return UserDecoration + id.Substring(0, Math.Min(10, id.Length));
        }
    }
}
