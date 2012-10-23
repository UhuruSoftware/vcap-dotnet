using Uhuru.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Uhuru.CloudFoundry.FileService;

namespace Uhuru.CloudFoundry.Test.Integration
{
    
    
    /// <summary>
    ///This is a test class for FTPUtilitiesTest and is intended
    ///to contain all FTPUtilitiesTest Integration Tests
    ///</summary>
    [TestClass()]
    public class FTPUtilitiesTest
    {
        [TestMethod()]
        [TestCategory("Unit")]
        public void CreateVirtualDirectory()
        {
            string name = Guid.NewGuid().ToString("N");

            string testDir = Path.Combine(Path.GetTempPath(), name);

            Directory.CreateDirectory(testDir);

            string username = Uhuru.Utilities.Credentials.GenerateCredential(5);
            string groupname = Uhuru.Utilities.Credentials.GenerateCredential(5);
            string password = "password1234!";

            string decoratedUsername = Uhuru.Utilities.WindowsVCAPUsers.CreateDecoratedUser(username, password);
            WindowsUsersAndGroups.CreateGroup(groupname);


            int port = Uhuru.Utilities.NetworkInterface.GrabEphemeralPort();

            FtpUtilities.CreateFtpSite(name, testDir, port);

            Assert.IsFalse(FtpUtilities.HasUserAccess(name, decoratedUsername));
            Assert.IsFalse(FtpUtilities.HasGroupAccess(name, groupname));

            FtpUtilities.AddUserAccess(name, decoratedUsername);
            FtpUtilities.AddGroupAccess(name, groupname);

            Assert.IsTrue(FtpUtilities.HasUserAccess(name, decoratedUsername));
            Assert.IsTrue(FtpUtilities.HasGroupAccess(name, groupname));

            FtpUtilities.DeleteUserAccess(name, decoratedUsername);
            FtpUtilities.DeleteGroupAccess(name, groupname);

            Assert.IsTrue(FtpUtilities.Exists(name));
            Assert.IsFalse(FtpUtilities.HasUserAccess(name, decoratedUsername));
            Assert.IsFalse(FtpUtilities.HasGroupAccess(name, groupname));


            FtpUtilities.DeleteFtpSite(name);

            Assert.IsFalse(FtpUtilities.Exists(name));

            WindowsUsersAndGroups.DeleteGroup(groupname);
            Uhuru.Utilities.WindowsVCAPUsers.DeleteDecoratedBasedUser(username);
        }
    }
}
