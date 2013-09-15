using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSecurityTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //MemoryTest test = new MemoryTest();
            //CPUTest test = new CPUTest();
            //FilesystemTest test = new FilesystemTest();
            //DiskQuotaTest test = new DiskQuotaTest();
            //TCPPortListenTest test = new TCPPortListenTest();
            //HTTPPortListenTest test = new HTTPPortListenTest();
            //NetworkOutboundBandwidthTest test = new NetworkOutboundBandwidthTest();

            WalkDirectoryTree(new DirectoryInfo(@"c:\"));

            //test.RunTest();
        }


        static void WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder 
            try
            {
                string adir = Guid.NewGuid().ToString("N");

                Directory.CreateDirectory(Path.Combine(root.FullName, adir));
                Console.WriteLine(root.FullName);
                Directory.Delete(Path.Combine(root.FullName, adir));
            }
            catch
            {
            }

            try
            {
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo);
                }
            }
            catch { }
        }
    }
}
