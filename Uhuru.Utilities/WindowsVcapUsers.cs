using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;

namespace Uhuru.Utilities
{
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
            DirectoryEntry obUser = entries.Add(decoreatedUsername, "User");
            obUser.Properties["FullName"].Add("Uhuru Vcap Instance " + AppId + " user");
            object obRet = obUser.Invoke("SetPassword", password);
            obUser.CommitChanges();
            return decoreatedUsername;
        }


        public static void DeleteUser(string AppId)
        {
            string decoreatedUsername = DecorateUser(AppId);
            DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries users = localDirectory.Children;
            DirectoryEntry user = users.Find(AppId);
            users.Remove(user);
        }
        

    }
}
