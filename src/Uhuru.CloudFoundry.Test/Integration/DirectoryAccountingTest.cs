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
    public class DirectoryAccountingTest
    {
        [TestMethod, TestCategory("Integration"), Description("Create VHD should create a file with the expected size.")]
        public void DirectorySizeTest1()
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
            //      file0 (1MB)

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
            string file5 = Path.Combine(dir4, "file5");

            File.WriteAllBytes(file0, new byte[1024 * 1024]);
            File.WriteAllBytes(file1, new byte[2048]);
            File.WriteAllBytes(file2, new byte[4096]);
            File.WriteAllBytes(file3, new byte[8192]);
            File.WriteAllBytes(file4, new byte[16384]);

            DirectoryAccounting da = new DirectoryAccounting();

            long dirSize = da.GetDirectorySize(tempDir);
            long KB = 1024;

            Assert.IsTrue(dirSize >= 2*KB + 28*KB + 1024*KB);

            da.SetDirectoryQuota(tempDir, 2 * KB + 28 * KB + 1024 * KB + 10 * KB);

            bool exceptionRaised = false;
            try
            {
                File.WriteAllBytes(file5, new byte[16384]);
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("There is not enough space on the disk"))
                {
                    exceptionRaised = true;
                }
            }

            Assert.IsTrue(exceptionRaised);

            da.RemoveDirectoryQuota(tempDir);
            File.WriteAllBytes(file5, new byte[16384]);

            Directory.Delete(tempDir, true);
        }

    }
}
