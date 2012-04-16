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

            string testDir = Path.Combine(@"G:\TestFTP", name);

            Directory.CreateDirectory(testDir);

            string username = Uhuru.Utilities.Credentials.GenerateCredential();
            string password = "password1234!";

            string decoratedUsername = Uhuru.Utilities.WindowsVCAPUsers.CreateUser(username, password);

            FtpUtilities.CreateFtpSite(name, testDir, decoratedUsername);

            FtpUtilities.DeleteFtpSite(name);

            Uhuru.Utilities.WindowsVCAPUsers.DeleteUser(username);
        }
    }
}
