using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.Test.Integration
{
    [TestClass]
    public class WindowsShareTest
    {

        WindowsShare ws = null;
        string shareName, shareForderPath, testFileContent, tempPath, username, password, decoratedUsername;

        [TestInitialize]
        public void Setup()
        {
            tempPath = Path.GetTempPath();
            shareName = DateTime.Now.Ticks.ToString();
            shareForderPath = Directory.CreateDirectory(Path.Combine(tempPath, shareName)).FullName;
            testFileContent = "this is a test";
            File.WriteAllText(Path.Combine(shareForderPath, "test.txt"), testFileContent);

            ws = WindowsShare.CreateShare(shareName, shareForderPath);


            username = Uhuru.Utilities.Credentials.GenerateCredential();
            password = "ca$hc0w";

            decoratedUsername = Uhuru.Utilities.WindowsVCAPUsers.CreateUser(username, password);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (ws != null)
            {
                ws.DeleteShare();
                ws = null;
            }
            if (username != null)
            {
                Uhuru.Utilities.WindowsVCAPUsers.DeleteUser(username);
            }
        }

        [TestMethod, TestCategory("Integration"), Description("Create a share and read a file from it.")]
        public void CreateShare()
        {
            ws.AddSharePermissions("Everyone");

            string contentsRead = File.ReadAllText(@"\\localhost\" + shareName + @"\test.txt");

            Assert.AreEqual(contentsRead, testFileContent);
        }

        [TestMethod, TestCategory("Integration"), Description("Delete share should not allow reading files from the share.")]
        [ExpectedException(typeof(IOException))]
        public void DeleteShare()
        {

            ws.AddSharePermissions("Everyone");

            ws.DeleteShare();
            ws = null;
            string contentsRead = File.ReadAllText(@"\\localhost\" + shareName + @"\test.txt");
        }

        [TestMethod, TestCategory("Integration"), Description("No permission should be set when the share is fresh and new.")]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public void PermissinDeniedByDefault()
        {
            string contentsRead = File.ReadAllText(@"\\localhost\" + shareName + @"\test.txt");
        }

        [TestMethod, TestCategory("Integration"), Description("The adding and deletion of permissions should work.")]
        public void AddAndDeletePermisions()
        {
            ws.AddSharePermissions(decoratedUsername);

            Assert.AreEqual(ws.GetSharePermissions().Count(), 1);
            Assert.AreEqual(ws.GetSharePermissions()[0], decoratedUsername);

            ws.DeleteSharePermission(decoratedUsername);

            Assert.AreEqual(ws.GetSharePermissions().Count(), 0);
        }

    }
}
