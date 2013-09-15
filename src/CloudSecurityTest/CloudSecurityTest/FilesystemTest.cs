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


    class FilesystemTest : SecurityTest, ISecurityTest
    {

        public bool IsLowerScoreBetter
        {
            get { return true; }
        }

        public string Description
        {
            get { return "Test if an app can create directories where it shouldn't."; }
        }

        public string Metric
        {
            get { return "Number of invalid directories created."; }
        }

        public void RunTest()
        {
            string dir = Guid.NewGuid().ToString("N");

            string[] directories = new string[] {
                @"c:\windows\",
                @"c:\windows\system32\",
                @"c:\",
                @"c:\Users\",
                @"c:\Users\Public\",
                @"c:\Program Files\",
                @"c:\Program Files (x86)\",
                @"c:\inetpub\",
                @"c:\Users\Public\Documents\",
                @"c:\Users\Public\Downloads\",
                @"C:\Users\Default\",
                @"C:\ProgramData\",
                @"C:\boot\",
                @"C:\recovery\",
                @"C:\ProgramData\Microsoft"
            };

            int score = 0;
            foreach (string parent in directories)
            {
                try
                {
                    Message(@"Trying to create dir in '{0}'", parent);
                    Directory.CreateDirectory(Path.Combine(parent, dir));
                    score += 1;
                    Directory.Delete(Path.Combine(parent, dir));
                }
                catch { }

                Score(score);

                try
                {
                    Message(@"Trying to create file in '{0}'", parent);
                    File.WriteAllText(Path.Combine(parent, dir + ".txt"), "test");
                    score += 1;
                    File.Delete(Path.Combine(parent, dir));
                }
                catch { }

                Score(score);


                // test create empty file

                // iterate through also everything

            }
        }
    }
}
