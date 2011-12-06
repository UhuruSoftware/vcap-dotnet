// -----------------------------------------------------------------------
// <copyright file="WindowsVcapUsers.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;

namespace Uhuru.Utilities
{
    using System;
    using System.DirectoryServices;
    
    public class WindowsVcapUsers
    {
        private static string DecorateUser(string id)
        {
            return "UhuruVcap" + id.Substring(0, Math.Min(10, id.Length)); 
        }

        public static string CreateUser(string AppId, string password)
        {
            if (password == null) password = Utilities.Credentials.GenerateCredential();
            string decoreatedUsername = DecorateUser(AppId);
            DirectoryEntry obDirEntry = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries entries = obDirEntry.Children;
            DirectoryEntry obUser = entries.Add(decoratedUsername, "User");
            obUser.Properties["FullName"].Add("Uhuru Vcap Instance " + appId + " user");
            object obRet = obUser.Invoke("SetPassword", password);
            obUser.CommitChanges();
            return decoratedUsername;
        }


        public static void DeleteUser(string AppId)
        {
            string decoreatedUsername = DecorateUser(AppId);
            DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries users = localDirectory.Children;
            DirectoryEntry user = users.Find(appId);
            users.Remove(user);
        }
    }
}
