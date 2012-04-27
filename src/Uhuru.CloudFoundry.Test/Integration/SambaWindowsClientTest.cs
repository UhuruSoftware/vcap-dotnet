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
    public class SambaWindowsClientTest
    {
        [TestMethod]
        public void TestLink()
        {
/*
            //make dir
            string tempPath1 = Path.GetTempPath();
            string tempPath2 = Path.GetTempPath();
            string persistentItemFile=System.IO.Path.GetRandomFileName();
            string filepath = System.IO.Path.Combine(tempPath1, persistentItemFile);
            
            //string persistentItemDir = "temp_folder";

            //create dir

//            if (System.IO.Directory.Exists(tempPath1)) { System.IO.Directory.Delete(tempPath1,true); };
//            if (System.IO.Directory.Exists(tempPath2)) { System.IO.Directory.Delete(tempPath2,true); };

            if (!System.IO.Directory.Exists(tempPath1)) { System.IO.Directory.CreateDirectory(tempPath1); };
            if (!System.IO.Directory.Exists(tempPath1)) { System.IO.Directory.CreateDirectory(tempPath1); };

            // create file

            if (System.IO.File.Exists(filepath)) { System.IO.File.Delete(filepath); };

            using (System.IO.FileStream fs = System.IO.File.Create(filepath))
            {
                for (byte i = 0; i < 100; i++)
                {
                    fs.WriteByte(i);
                }
            }


            //make link to file
            SambaWindowsClient.Link(tempPath1, filepath, tempPath2);

            //test if you can access the link to file



            //make link to dir
            //test if you can access the link to dir

            //write to file
            //assert if the content written is the same as the file content

            //test for no overwritten files

            //remove dir
            //remove file
 */
        }
    }
}
