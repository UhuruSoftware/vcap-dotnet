using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudSecurityTest
{


    class HTTPPortListenTest : SecurityTest, ISecurityTest
    {

        public bool IsLowerScoreBetter
        {
            get { return true; }
        }

        public string Description
        {
            get { return "Test if an app can use an HTTP listener (through http.sys) other than the one assigned (9911)."; }
        }

        public string Metric
        {
            get { return "Number of invalid listeners opened."; }
        }

        public void RunTest()
        {
            int[] ports = new int[] { 80, 22, 1, 100, 3389, 9999, 50000, 43000, 0 };

            int score = 0;
            foreach (int port in ports)
            {
                try
                {
                    Message(@"Trying to open port '{0}'", port);


                    HttpListener actualServer = null;

                    try
                    {
                        int actualServerPort = port;
                        actualServer = new HttpListener();
                        actualServer.Prefixes.Add(string.Format("http://*:{0}/", port));
                        actualServer.Start();
                    }
                    finally
                    {
                        if (actualServer != null)
                        {
                            actualServer.Stop();
                        }
                    }

                    score += 1;
                }
                catch { }
                Score(score);
            }
        }
    }
}
