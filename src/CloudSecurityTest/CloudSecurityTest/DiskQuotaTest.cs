using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudSecurityTest
{


    class DiskQuotaTest : SecurityTest, ISecurityTest
    {

        public bool IsLowerScoreBetter
        {
            get { return true; }
        }

        public string Description
        {
            get { return "Test if an app can go beyond a 128MB disk quota."; }
        }

        public string Metric
        {
            get { return "Megabytes written beyond 128MB"; }
        }

        public void RunTest()
        {
            string dir = Path.Combine(@"c:\users\", Environment.UserName);
            string fileName = Path.Combine(dir, Guid.NewGuid().ToString("N"));

            for (int size = 1; size < 256; size++)
            {
                byte[] content = new byte[1024 * 1024];

                File.AppendAllText(fileName, ASCIIEncoding.ASCII.GetString(content));
                Message("Written {0}MB", size);
                Score(Math.Max(0, size - 128));
            }
        }
    }
}
