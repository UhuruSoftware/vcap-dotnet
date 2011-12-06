// -----------------------------------------------------------------------
// <copyright file="WindowsVcapUsers.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.DirectoryServices;
    
    public class WindowsVcapUsers
    {
        public static string CreateUser(string appId, string password)
        {
            if (password == null) password = Utilities.Credentials.GenerateCredential();
            string decoreatedUsername = "UhuruVcap" + appId;
            DirectoryEntry obDirEntry = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries entries = obDirEntry.Children;
            DirectoryEntry obUser = entries.Add(decoreatedUsername, "User");
            obUser.Properties["FullName"].Add("Uhuru Vcap Instance " + appId + " user");
            object obRet = obUser.Invoke("SetPassword", password);
            obUser.CommitChanges();
            return decoreatedUsername;
        }

        public static void DeleteUser(string appId)
        {
            string decoreatedUsername = "UhuruVcap" + appId;
            DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries users = localDirectory.Children;
            DirectoryEntry user = users.Find(appId);
            users.Remove(user);
        }
    }
}
