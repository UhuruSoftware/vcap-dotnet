using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.DEA.Plugins;
using Uhuru.CloudFoundry.DEA.PluginBase;
using System.Threading;
using Uhuru.Utilities;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Uhuru.CloudFoundry.Test.Integration
{
    [TestClass]
    [DeploymentItem("log4net.config")]
    public class FileServerTest
    {

        
        public FileServerTest()
        {
            MethodInfo getSyntax = typeof(UriParser).GetMethod("GetSyntax", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo flagsField = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            if (getSyntax != null && flagsField != null)
            {
                foreach (string scheme in new[] { "http", "https" })
                {
                    UriParser parser = (UriParser)getSyntax.Invoke(null, new object[] { scheme });
                    if (parser != null)
                    {
                        int flagsValue = (int)flagsField.GetValue(parser);
                        // Clear the CanonicalizeAsFilePath attribute

                        flagsValue = (int)flagsField.GetValue(parser);

                        if ((flagsValue & 0x800000) != 0)
                        {
                            flagsField.SetValue(parser, flagsValue & ~0x800000);
                        }

                    }
                }
            }
        }

        /// <summary>
        ///A test for ConfigureApplication
        ///</summary>
        [TestMethod()]
        [TestCategory("Integration")]
        public void TC001_FileServerTest()
        {
            string user = Credentials.GenerateCredential();
            string password = Credentials.GenerateCredential();

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempPath);
            
            File.WriteAllText(Path.Combine(tempPath, "test.txt"), "this is a test");

            int port = NetworkInterface.GrabEphemeralPort();

            FileServer fs = new FileServer(port, tempPath, "foobar", user, password);

            fs.Start();

            WebClient client = new WebClient();
            NetworkCredential credentials = new NetworkCredential(user, password);
            client.Credentials = credentials;
            byte[] data = client.DownloadData(String.Format("http://{0}:{1}/foobar/test.txt", "localhost", port));

            ASCIIEncoding encoding = new ASCIIEncoding();
            string retrievedContents = encoding.GetString(data);

            Assert.IsTrue(retrievedContents.Contains("this is a test"));

            //Thread.Sleep(6000000);

            fs.Stop();
        }


        /// <summary>
        ///Another test for ConfigureApplication
        ///</summary>
        [TestMethod()]
        [TestCategory("Integration")]
        public void TC002_FileServerTest()
        {
            string user = Credentials.GenerateCredential();
            string password = Credentials.GenerateCredential();

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string dirPath = Path.Combine(tempPath, "testdir");
            Directory.CreateDirectory(dirPath);

            Directory.CreateDirectory(Path.Combine(dirPath, "testDir2"));
            File.WriteAllText(Path.Combine(dirPath, "test.txt"), "this is a test");
            File.WriteAllText(Path.Combine(dirPath, "test2.txt"), "this is a test");


            int port = NetworkInterface.GrabEphemeralPort();

            FileServer fs = new FileServer(port, tempPath, "foobar", user, password);

            fs.Start();

            WebClient client = new WebClient();
            NetworkCredential credentials = new NetworkCredential(user, password);
            client.Credentials = credentials;
            byte[] data = client.DownloadData(String.Format("http://{0}:{1}/foobar/testdir", "localhost", port));

            ASCIIEncoding encoding = new ASCIIEncoding();
            string retrievedContents = encoding.GetString(data);

            Assert.IsTrue(retrievedContents.Contains("testDir2                                     -\r\ntest.txt                                   14B\r\ntest2.txt                                  14B\r\n"));

            fs.Stop();
        }

        /// <summary>
        ///Another test for ConfigureApplication
        ///</summary>
        [TestMethod()]
        [TestCategory("Integration")]
        public void TC003_SecurityFileServerTest()
        {
            string user = Credentials.GenerateCredential();
            string password = Credentials.GenerateCredential();

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempPath);

            Directory.CreateDirectory(Path.Combine(tempPath, "testDir"));
            File.WriteAllText(Path.Combine(tempPath, "test.txt"), "this is a test");
            File.WriteAllText(Path.Combine(tempPath, "test2.txt"), "this is a test");


            int port = NetworkInterface.GrabEphemeralPort();

            FileServer fs = new FileServer(port, tempPath, "foobar", user, password);

            fs.Start();

            WebClient client = new WebClient();
            NetworkCredential credentials = new NetworkCredential(user, password);
            client.Credentials = credentials;


            byte[] gooddata = client.DownloadData(String.Format("http://{0}:{1}/foobar/test.txt", "localhost", port));

            ASCIIEncoding encoding = new ASCIIEncoding();
            string retrievedContents = encoding.GetString(gooddata);

            Assert.IsTrue(retrievedContents.Contains("this is a test"));

            Trace.WriteLine(String.Format("http://{2}:{3}@{0}:{1}/foobar/../", 
                NetworkInterface.GetLocalIPAddress(), port, user, password));

            try
            {
                Uri dl = new Uri(String.Format("http://{0}:{1}/foobar/useless/../../", "localhost", port));

                byte[] data = client.DownloadData(dl);
            }
            catch
            {
                return;
            }
            finally
            {
                fs.Stop();
            }

            Assert.Fail();
        }

        /// <summary>
        ///Another test for ConfigureApplication
        ///</summary>
        [TestMethod()]
        [TestCategory("Integration")]
        public void TC004_FileNamesWithSpacesFileServerTest()
        {
            string user = Credentials.GenerateCredential();
            string password = Credentials.GenerateCredential();

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempPath);

            Directory.CreateDirectory(Path.Combine(tempPath, "testDir with space"));
            File.WriteAllText(Path.Combine(tempPath, "test with space.txt"), "this is a test");
            File.WriteAllText(Path.Combine(tempPath, "test2.txt"), "this is a test");


            int port = NetworkInterface.GrabEphemeralPort();

            FileServer fs = new FileServer(port, tempPath, "foobar", user, password);

            fs.Start();

            WebClient client = new WebClient();
            NetworkCredential credentials = new NetworkCredential(user, password);
            client.Credentials = credentials;


            byte[] gooddata = client.DownloadData(String.Format("http://{0}:{1}/foobar/test with space.txt", "localhost", port));

            ASCIIEncoding encoding = new ASCIIEncoding();
            string retrievedContents = encoding.GetString(gooddata);

            Assert.IsTrue(retrievedContents.Contains("this is a test"));

            Trace.WriteLine(String.Format("http://{2}:{3}@{0}:{1}/foobar/../",
                NetworkInterface.GetLocalIPAddress(), port, user, password));

            try
            {
                Uri dl = new Uri(String.Format("http://{0}:{1}/foobar/useless/../../", "localhost", port));

                byte[] data = client.DownloadData(dl);
            }
            catch
            {
                return;
            }
            finally
            {
                fs.Stop();
            }

            Assert.Fail();
        }

    }
}