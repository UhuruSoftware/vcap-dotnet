using Uhuru.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Uhuru.CloudFoundry.DEA.DirectoryServer;
using System.Threading;
using System.Text;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using Uhuru.Configuration;

namespace Uhuru.CloudFoundry.Test.Unit.DirectoryServer
{
    [TestClass()]
    public class DirectoryServerTest
    {
        private class MockDeaClient : IDeaClient
        {
            private static string path;

            public static void SetResponse(string path)
            {
                MockDeaClient.path = path;
            }

            public PathLookupResponse LookupPath(Uri path)
            {
                return new PathLookupResponse()
                {
                    Error = null,
                    Path = Path.Combine(MockDeaClient.path, Path.GetDirectoryName(path.LocalPath).Substring(1), Path.GetFileName(path.LocalPath))
                };
            }
        }

        public static string DownloadString(string uri)
        {
            HttpWebResponse response = null;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            response = (HttpWebResponse)request.GetResponse();
            Encoding responseEncoding = Encoding.GetEncoding(response.CharacterSet);
            using (StreamReader sr = new StreamReader(response.GetResponseStream(), responseEncoding))
            {
                return sr.ReadToEnd();
            }
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [DeploymentItem("uhuruTest.config")]
        public void TestServerDirectoryDirList()
        {
            string pathQuery = Guid.NewGuid().ToString();
            string tempDir = Path.Combine(Path.GetTempPath(), pathQuery);

            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(Path.Combine(tempDir, "dir1"));
            Directory.CreateDirectory(Path.Combine(tempDir, "dir2"));

            string loremIpsum = "Lorem ipsum dolor sit amet";

            File.WriteAllText(Path.Combine(tempDir, "file1.txt"), loremIpsum);

            StringBuilder sb = new StringBuilder(loremIpsum.Length * 1024);
            for (int i = 0; i < 1024; i++)
            {
                sb.Append(loremIpsum);
            }

            File.WriteAllText(Path.Combine(tempDir, "file2.txt"), sb.ToString());

            MockDeaClient.SetResponse(Path.GetTempPath());

            MockDeaClient client = new MockDeaClient();
            Uhuru.CloudFoundry.DEA.DirectoryServer.DirectoryServer server = new DEA.DirectoryServer.DirectoryServer("127.0.0.1", DirectoryConfiguration.ReadConfig(), client);
            server.Start();

            string output = DownloadString(string.Format("http://127.0.0.1:{0}/{1}", DirectoryConfiguration.ReadConfig().DirectoryServer.V2Port, pathQuery));

            server.Stop();

            Assert.IsFalse(string.IsNullOrWhiteSpace(output));

            string expectedOutput = string.Format(@"dir1/                                        -
dir2/                                        -
file1.txt                                  {0}B
file2.txt                               {1}K
", loremIpsum.Length, ((1024.0 * loremIpsum.Length) / 1024).ToString("0.00"));

            Assert.IsTrue(output == expectedOutput);
        }

        [TestMethod()]
        [TestCategory("Unit")]
        [DeploymentItem("uhuruTest.config")]
        public void TestServerDirectoryFileDump()
        {
            string pathQuery = Guid.NewGuid().ToString();
            string tempDir = Path.Combine(Path.GetTempPath(), pathQuery);

            Directory.CreateDirectory(tempDir);

            string loremIpsum = "Lorem ipsum dolor sit amet";

            StringBuilder sb = new StringBuilder(loremIpsum.Length * 1024);
            for (int i = 0; i < 102400; i++)
            {
                sb.Append(loremIpsum);
            }

            File.WriteAllText(Path.Combine(tempDir, "file.txt"), sb.ToString());

            MockDeaClient.SetResponse(tempDir);

            MockDeaClient client = new MockDeaClient();
            Uhuru.CloudFoundry.DEA.DirectoryServer.DirectoryServer server = new DEA.DirectoryServer.DirectoryServer("127.0.0.1", DirectoryConfiguration.ReadConfig(), client);
            server.Start();

            string output = DownloadString(string.Format("http://127.0.0.1:{0}/{1}", DirectoryConfiguration.ReadConfig().DirectoryServer.V2Port, "file.txt"));

            server.Stop();

            Assert.IsFalse(string.IsNullOrWhiteSpace(output));

            Assert.IsTrue(output == sb.ToString());
        }


        [TestMethod()]
        [TestCategory("Unit")]
        [DeploymentItem("uhuruTest.config")]
        public void TestServerDirectoryFileTail()
        {
            string pathQuery = Guid.NewGuid().ToString();
            string tempDir = Path.Combine(Path.GetTempPath(), pathQuery);

            Directory.CreateDirectory(tempDir);

            string loremIpsum = "Lorem ipsum dolor sit amet";

            string filePath = Path.Combine(tempDir, "file.txt");

            File.WriteAllText(filePath, loremIpsum);

            MockDeaClient.SetResponse(tempDir);

            MockDeaClient client = new MockDeaClient();
            DEAElement config = DirectoryConfiguration.ReadConfig();
            Uhuru.CloudFoundry.DEA.DirectoryServer.DirectoryServer server = new DEA.DirectoryServer.DirectoryServer("127.0.0.1", config, client);
            config.DirectoryServer.StreamingTimeoutMS = 5000;
            server.Start();

            Random rnd = new Random();

            string returnBytes = string.Empty;
            string sentBytes = string.Empty;
            int readCount = 0;

            string appendChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            Thread newThread = new Thread(() =>
            {
                byte[] randomBytes = new byte[rnd.Next(100)];
                rnd.NextBytes(randomBytes);

                File.AppendAllText(filePath, ASCIIEncoding.ASCII.GetString(randomBytes));

                HttpWebResponse response = null;
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://127.0.0.1:{0}/{1}?tail", DirectoryConfiguration.ReadConfig().DirectoryServer.V2Port, "file.txt"));
                response = (HttpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();

                byte[] responseBytes = new byte[200];

                int read;

                do 
                {
                    read = responseStream.Read(responseBytes, 0, responseBytes.Length);
                    returnBytes += ASCIIEncoding.ASCII.GetString(responseBytes, 0, read);
                    readCount ++;
                }
                while (read != 0);
            });

            Thread writerThread = new Thread(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(50);

                    string toWrite = string.Empty;

                    for (int j = 0; j < rnd.Next(100); j++)
                    {
                        toWrite += appendChars[rnd.Next(appendChars.Length)];
                    }


                    File.AppendAllText(filePath, toWrite);
                    sentBytes += toWrite;
                }
            });

            newThread.Start();
            Thread.Sleep(1000);
            writerThread.Start();

            writerThread.Join();
            newThread.Join();

            server.Stop();

            Assert.AreEqual(sentBytes, returnBytes);
            Assert.IsTrue(readCount > 1);
        }
    }
}
