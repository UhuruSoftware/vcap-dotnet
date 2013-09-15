using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudSecurityTest
{


    class NetworkOutboundBandwidthTest : SecurityTest, ISecurityTest
    {

        public bool IsLowerScoreBetter
        {
            get { return true; }
        }

        public string Description
        {
            get { return "Test if an app can upload data at an unbounded rate."; }
        }

        public string Metric
        {
            get { return "Upload speed (KB/s)."; }
        }

        public void RunTest()
        {
            for (int j = 1; j <= 10; j++)
            {
                Message("Test run {0}", j);


                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://10.0.0.136/vladi/uploadtest.txt");
                request.ConnectionGroupName = "MyGroupName";
                request.UseBinary = true;
                request.KeepAlive = true;
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential("jenkins", "uhuruservice1234!");

                // Copy the contents of the file to the request stream.
                request.ContentLength = 1024*1024;

                Stream requestStream = request.GetRequestStream();

                Stopwatch timer = Stopwatch.StartNew();

                for (int i = 0; i < request.ContentLength / 256; i++ )
                {
                    timer.Stop();

                    byte[] data = new byte[256];
                    Random random = new Random();
                    random.NextBytes(data);

                    timer.Start();

                    requestStream.Write(data, 0, data.Length);
                }
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();

                timer.Stop();

                Score((1 / timer.Elapsed.TotalSeconds) * 1024);
            }
        }
    }
}
