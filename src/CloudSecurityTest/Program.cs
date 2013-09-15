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
            FilesystemTest test = new FilesystemTest();
            //DiskQuotaTest test = new DiskQuotaTest();
            //TCPPortListenTest test = new TCPPortListenTest();
            //HTTPPortListenTest test = new HTTPPortListenTest();
            //NetworkOutboundBandwidthTest test = new NetworkOutboundBandwidthTest();

            Console.WindowHeight = 60;
            Console.WindowWidth = 100;
            Console.BufferHeight = 9999;
            Console.BufferWidth = 100;

            //WalkDirectoryTree(new DirectoryInfo(@"c:\"));

            test.RunTest();

            Console.WriteLine("Press Enter to end");
            Console.ReadLine();
        }
    }
}
