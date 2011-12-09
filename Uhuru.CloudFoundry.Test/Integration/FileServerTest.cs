using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.DEA.Plugins;
using Uhuru.CloudFoundry.Server.DEA.PluginBase;
using System.Threading;
using Uhuru.Utilities;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace Uhuru.CloudFoundry.Test.Integration
{
    [TestClass]
    [DeploymentItem("log4net.config")]
    public class FileServerTest
    {

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
            byte[] data = client.DownloadData(String.Format("http://{0}:{1}/foobar/", "localhost", port));

            ASCIIEncoding encoding = new ASCIIEncoding();
            string retrievedContents = encoding.GetString(data);

            Assert.IsTrue(retrievedContents.Contains("testDir                                      -\r\ntest.txt                                   14B\r\ntest2.txt                                  14B\r\n"));

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


           // byte[] gooddata = client.DownloadData(String.Format("http://{0}:{1}/foobar/test.txt", "localhost", port));

            //ASCIIEncoding encoding = new ASCIIEncoding();
          //  string retrievedContents = encoding.GetString(gooddata);

           // Assert.IsTrue(retrievedContents.Contains("this is a test"));

            Trace.WriteLine(String.Format("http://{2}:{3}@{0}:{1}/foobar/../", "localhost", port, user, password));

            Thread.Sleep(123982346);

            try
            {
                byte[] data = client.DownloadData(String.Format("http://{0}:{1}/foobar/../", "localhost", port));
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