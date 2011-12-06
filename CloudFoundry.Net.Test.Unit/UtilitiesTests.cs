using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.Utilities;
using Uhuru.Utilities.ProcessPerformance;


namespace CloudFoundry.Net.Test.Unit
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod]
        public void ProcessInformationTest()
        {
            ProcessData[] entries = ProcessInformation.GetProcessUsage();
            Assert.IsTrue(0 < entries.Sum(entry => entry.Cpu));
            Assert.IsTrue(0 < entries.Length);
        }


        [TestMethod]
        public void ProcessInformationFilteredTest()
        {
            ProcessData entry = ProcessInformation.GetProcessUsage(Process.GetCurrentProcess().Id);
            Assert.AreNotEqual(null, entry);
        }

        [TestMethod]
        public void MonitoringServerTest()
        {
            string username = "test";
            string password = "test";
            int port = Helper.GetEphemeralPort();
            MonitoringServer monitoringServer = new MonitoringServer(port, "localhost", username, password);
            
            monitoringServer.VarzRequested += new EventHandler<VarzRequestEventArgs>(monitoringServer_VarzRequested);
            monitoringServer.HealthzRequested += new EventHandler<HealthzRequestEventArgs>(monitoringServer_HealthzRequested);
            monitoringServer.Start();

            string credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
          
            Uri url = new Uri("http://localhost:" + port + "/heathz");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Basic " + credentials);
            request.AllowAutoRedirect = false;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.IsTrue(response.ContentType == "text/plaintext");
            string body = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Assert.AreEqual("healthz", body);

            url = new Uri("http://localhost:" + port + "/varz");
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Basic " + credentials);
            request.AllowAutoRedirect = false;
            response = (HttpWebResponse)request.GetResponse();
            Assert.IsTrue(response.ContentType == "application/json");
            body = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Assert.AreEqual("varz", body);

            request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            WebException ex = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
            Assert.AreEqual(HttpStatusCode.Unauthorized, ((HttpWebResponse)ex.Response).StatusCode);
            monitoringServer.Stop();
        }

        [TestMethod]
        public void FileServerTest()
        {
            string username = "test";
            string password = "test";

            int port = Helper.GetEphemeralPort();
            string path = Directory.GetCurrentDirectory();
            string filename = "CloudFoundry.Net.Test.Unit.dll.config";
            
            FileServer fileServer = new FileServer(port, path, "/test", username, password);
            fileServer.Start();

            Uri url = new Uri("http://localhost:" + port + "/test/" + filename);
            string credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Basic " + credentials);
            request.AllowAutoRedirect = false;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsTrue(response.ContentType == "application/octet-stream");

            request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            WebException ex = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
            Assert.AreEqual(HttpStatusCode.Unauthorized, ((HttpWebResponse)ex.Response).StatusCode);

            fileServer.Stop();
        }

        void monitoringServer_HealthzRequested(object sender, HealthzRequestEventArgs e)
        {
            e.HealthzMessage = "healthz";
        }

        void monitoringServer_VarzRequested(object sender, VarzRequestEventArgs e)
        {
            e.VarzMessage = "varz";
        }
    }
}
