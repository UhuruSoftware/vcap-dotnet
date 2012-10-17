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
            string password = "password1234!";

            string decoratedUsername = Uhuru.Utilities.WindowsVCAPUsers.CreateDecoratedUser(username, password);
            string decoratedUsername2 = Uhuru.Utilities.WindowsVCAPUsers.CreateDecoratedUser(username + "2", password);

            int port = Uhuru.Utilities.NetworkInterface.GrabEphemeralPort();

            FtpUtilities.CreateFtpSite(name, testDir, port);

            FtpUtilities.AddUserAccess(name, decoratedUsername);
            FtpUtilities.DeleteUserAccess(name, decoratedUsername);

            Assert.IsTrue(FtpUtilities.Exists(name));

            FtpUtilities.DeleteFtpSite(name);

            Assert.IsFalse(FtpUtilities.Exists(name));

            Uhuru.Utilities.WindowsVCAPUsers.DeleteDecoratedBasedUser(username);
            Uhuru.Utilities.WindowsVCAPUsers.DeleteDecoratedBasedUser(username + "2");
        }
    }
}
