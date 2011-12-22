using Uhuru.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Uhuru.CloudFoundry.Test.Unit
{
    
    
    /// <summary>
    ///This is a test class for DiskUsageTest and is intended
    ///to contain all DiskUsageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DiskUsageTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetDiskUsage
        ///</summary>
        [TestMethod()]
        [TestCategory("Unit")]
        public void GetDiskUsageTest()
        {

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            // This is the structure we are creating

            //  tempdir
            //      dir1 (2)
            //          file1 (2KB)
            //      dir2 (28 KB)
            //          dir3 (24 KB)
            //              dir4 (16KB)
            //                  file4 (16KB)
            //              file3 (8KB)
            //          file2 (4KB)
            //      file0 (1KB)

            string dir1 = Path.Combine(tempDir, "dir1");
            string dir2 = Path.Combine(tempDir, "dir2");
            string dir3 = Path.Combine(dir2, "dir3");
            string dir4 = Path.Combine(dir3, "dir4");

            Directory.CreateDirectory(dir1);
            Directory.CreateDirectory(dir4);

            string file0 = Path.Combine(tempDir, "file0");
            string file1 = Path.Combine(dir1, "file1");
            string file2 = Path.Combine(dir2, "file2");
            string file3 = Path.Combine(dir3, "file3");
            string file4 = Path.Combine(dir4, "file4");

            File.WriteAllBytes(file0, new byte[1024 * 1024]);
            File.WriteAllBytes(file1, new byte[2048]);
            File.WriteAllBytes(file2, new byte[4096]);
            File.WriteAllBytes(file3, new byte[8192]);
            File.WriteAllBytes(file4, new byte[16384]);

            DiskUsageEntry[] entries = DiskUsage.GetDiskUsage(tempDir, true);
            
            Assert.AreEqual(3, entries.Length);

            Assert.AreEqual(2, entries[0].SizeKB);
            Assert.AreEqual(28, entries[1].SizeKB);
            Assert.AreEqual(1024, entries[2].SizeKB);

            entries = DiskUsage.GetDiskUsage(tempDir, false);

            Assert.AreEqual(9, entries.Length);

            Assert.AreEqual(2, entries[0].SizeKB);    //dir1
            Assert.AreEqual(2, entries[1].SizeKB);   //file1
            Assert.AreEqual(28, entries[2].SizeKB);   //dir2
            Assert.AreEqual(24, entries[3].SizeKB);   //dir3
            Assert.AreEqual(16, entries[4].SizeKB);   //dir4
            Assert.AreEqual(16, entries[5].SizeKB);    //file4
            Assert.AreEqual(8, entries[6].SizeKB);    //file3
            Assert.AreEqual(4, entries[7].SizeKB);   //file2
            Assert.AreEqual(1024, entries[8].SizeKB);    //file0

            Assert.AreEqual("16KB", entries[4].ReadableSize);
            Assert.AreEqual("1MB", entries[8].ReadableSize);
        }
    }
}
