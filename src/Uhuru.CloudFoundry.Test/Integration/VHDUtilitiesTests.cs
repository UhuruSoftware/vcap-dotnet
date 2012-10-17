using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.FileService;
using System.IO;


namespace Uhuru.CloudFoundry.Test.Integration
{
    [TestClass]
    public class VHDUtilitiesTests
    {
        [TestMethod, TestCategory("Integration"), Description("Create VHD should create a file with the expected size.")]
        public void CreateVHDTest1()
        {
            var tempPath = Path.GetTempPath();
            var tempVhdFile = Path.Combine(tempPath, Guid.NewGuid().ToString() + ".vhd");
            VHDUtilities.CreateVHD(tempVhdFile, 100, true);

            Assert.IsTrue(new FileInfo(tempVhdFile).Length > 100 * 1024 * 1024);

            File.Delete(tempVhdFile);
        }

        [TestMethod, TestCategory("Integration"), Description("Create VHD with fixed size set to false should not allocate the entire disk size.")]
        public void CreateVHDTest2()
        {
            var tempPath = Path.GetTempPath();
            var tempVhdFile = Path.Combine(tempPath, Guid.NewGuid().ToString() + ".vhd");
            VHDUtilities.CreateVHD(tempVhdFile, 1000, false);

            // should use less then 20MB for a 1GB empty VHD.
            Assert.IsTrue(new FileInfo(tempVhdFile).Length < 20 * 1024 * 1024);

            File.Delete(tempVhdFile);
        }

        [TestMethod, TestCategory("Integration"), Description("Mount VHD should allow you write access.")]
        public void MountVHDTest1()
        {
            var tempPath = Path.GetTempPath();

            // n.b. if the extension is not present an error will be thrown by diskpart when selecting the file
            var tempVhdFile = Path.Combine(tempPath, Guid.NewGuid().ToString() + ".vhd");
            var tempMountPath = Path.Combine(tempPath, Guid.NewGuid().ToString());

            VHDUtilities.CreateVHD(tempVhdFile, 100, false);

            Assert.IsFalse(VHDUtilities.IsMountPointPresent(tempMountPath));

            VHDUtilities.MountVHD(tempVhdFile, tempMountPath);

            Assert.IsTrue(VHDUtilities.IsMountPointPresent(tempMountPath));

            File.WriteAllText(Path.Combine(tempMountPath, "test"), "test");

            Assert.IsTrue(File.ReadAllText(Path.Combine(tempMountPath, "test")) == "test" );

            VHDUtilities.UnmountVHD(tempVhdFile);

            Assert.IsTrue(File.Exists(Path.Combine(tempMountPath, "test")) == false);

            VHDUtilities.MountVHD(tempVhdFile, tempMountPath);
            Assert.IsTrue(File.ReadAllText(Path.Combine(tempMountPath, "test")) == "test");
            VHDUtilities.UnmountVHD(tempVhdFile);

            File.Delete(tempVhdFile);
        }

    }
}
