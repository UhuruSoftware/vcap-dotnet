using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Uhuru.Utilities;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.IO;


namespace CloudFoundry.Net.Test.Unit
{
    public class UtilitiesTests
    {
        [Test]
        public void ProcessInformationTest()
        {
            ProcessInformationEntry[] entries = ProcessInformation.GetProcessInformation(true, true, true, true, true, true, 0);
            Assert.Less(0, entries.Length);
        }


        [Test]
        public void ProcessInformationFilteredTest()
        {
            ProcessInformationEntry[] entries = ProcessInformation.GetProcessInformation(true, true, true, true, true, true, Process.GetCurrentProcess().Id);
            Assert.AreEqual(1, entries.Length);
        }

        [Test]
        public void MonitoringServerTest()
        {
            int port = Helper.GetEphemeralPort();
            MonitoringServer monitoringServer = new MonitoringServer(port);
            
            monitoringServer.VarzRequested += new MonitoringServer.VarzRequestedHandler(monitoringServer_VarzRequested);
            monitoringServer.HealthzRequested += new MonitoringServer.HealthzRequestedHandler(monitoringServer_HealthzRequested);
            monitoringServer.Start();

            Uri url = new Uri("http://localhost:" + port + "/heathz");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.IsTrue(response.ContentType == "text/plaintext");
            string body = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Assert.AreEqual("healthz", body);

            url = new Uri("http://localhost:" + port + "/varz");
            request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            response = (HttpWebResponse)request.GetResponse();
            Assert.IsTrue(response.ContentType == "application/json");
            body = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Assert.AreEqual("varz", body);

            monitoringServer.Stop();
        }

        string monitoringServer_HealthzRequested(object sender, EventArgs e)
        {
            return "healthz";
        }

        string monitoringServer_VarzRequested(object sender, EventArgs e)
        {
            return "varz";
        }
    }
}
