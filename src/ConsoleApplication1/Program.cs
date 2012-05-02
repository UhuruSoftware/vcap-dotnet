using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities.HttpTunnel;
using Uhuru.Utilities;
using System.ServiceModel;

namespace ConsoleApplication1
{
    class Program
    {
        private class TunnelWCFServerTcp
        {
            ServiceHost host;

            public void StartServer(int wcfPort, int tunnelPort)
            {
                Uri baseAddress = new Uri("http://127.0.0.1:" + wcfPort.ToString());

                ServerEnd service = new ServerEnd();

                BasicHttpBinding httpBinding = new BasicHttpBinding();
                httpBinding.ReaderQuotas.MaxArrayLength = DataPackage.BufferSize;
                httpBinding.ReaderQuotas.MaxBytesPerRead = DataPackage.BufferSize * 16;
                httpBinding.MaxReceivedMessageSize = DataPackage.BufferSize * 16;
                httpBinding.MaxBufferPoolSize = DataPackage.BufferSize * 16;
                httpBinding.MaxBufferSize = DataPackage.BufferSize * 16;

                host = new ServiceHost(service, baseAddress);
                host.AddServiceEndpoint(typeof(ITunnel), httpBinding, baseAddress);

                ((ServerEnd)host.SingletonInstance).Initialize("192.168.1.157", tunnelPort, TunnelProtocolType.Ftp);
                host.Open();
            }

            public void Stop()
            {
                if (host != null)
                {
                    host.Close();
                }
            }
        }

        static void Main(string[] args)
        {

            TunnelWCFServerTcp wcfServer = new TunnelWCFServerTcp();
            ClientEnd clientEnd = null;

            // Arrange
            int wcfServerPort = NetworkInterface.GrabEphemeralPort();
            int actualServerPort = 21;
            int clientEndPort = 10021;
            clientEnd = new ClientEnd();

            wcfServer.StartServer(wcfServerPort, actualServerPort);
            clientEnd.Start(new Uri("http://127.0.0.1:" + wcfServerPort.ToString()), clientEndPort, "127.0.0.1", TunnelProtocolType.Ftp);

            Console.ReadLine();
        }
    }
}
