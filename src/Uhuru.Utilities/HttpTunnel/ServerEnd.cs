// -----------------------------------------------------------------------
// <copyright file="ServerEnd.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.HttpTunnel
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.ServiceModel;

    /// <summary>
    /// This class implements the server part of an HTTP tunnel.
    /// It is used in conjunction with a WCF service.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class ServerEnd : ITunnel, IDisposable
    {
        /// <summary>
        /// The host used to connect to the actual server.
        /// </summary>
        private string host;

        /// <summary>
        /// The port for the actual server.
        /// </summary>
        private int port;
        
        /// <summary>
        /// Table containing all the TCP connections for a TCP tunnel.
        /// </summary>
        private Dictionary<Guid, TcpClient> clients;

        /// <summary>
        /// UDP client used for a UDP tunnel.
        /// </summary>
        private UdpClient udpClient;

        /// <summary>
        /// Specifies the type of tunnel (TCP or UDP).
        /// </summary>
        private TunnelProtocolType protocol;

        /// <summary>
        /// Initializes the server end of the tunnel with connection information.
        /// </summary>
        /// <param name="host">The hostname on which to connect (if it's a TCP connection).</param>
        /// <param name="port">The port on which to connect.</param>
        /// <param name="protocol">The protocol to use.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", Justification = "This method is used instead of a constructor, because the WCF host creates an instance of this class")]
        public void Initialize(string host, int port, TunnelProtocolType protocol)
        {
            this.host = host;
            this.port = port;
            this.protocol = protocol;

            switch (protocol)
            {
                case TunnelProtocolType.Tcp:
                    {
                        this.clients = new Dictionary<Guid, TcpClient>();
                    }

                    break;
                case TunnelProtocolType.UdpIncoming:
                    {
                        this.udpClient = new UdpClient();
                        this.udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        this.udpClient.Client.Bind(new IPEndPoint(IPAddress.Parse(host), port));
                    }

                    break;
                case TunnelProtocolType.UdpOutgoing:
                    {
                        this.udpClient = new UdpClient();
                        this.udpClient.Connect(new IPEndPoint(IPAddress.Parse(host), port));
                    }

                    break;
            }
        }

        /// <summary>
        /// Creates a TCP connection on the server.
        /// </summary>
        /// <returns>
        /// A GUID used to identify the connection for future send/receive or for close.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Client is added to collection that is properly disposed.")]
        public Guid OpenConnection()
        {
            if (this.protocol == TunnelProtocolType.Tcp)
            {
                TcpClient client = new TcpClient(this.host, this.port);
                Guid connectionId = Guid.NewGuid();
                this.clients.Add(connectionId, client);
                return connectionId;
            }
            else
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Closes a TCP connection on the server.
        /// </summary>
        /// <param name="connectionId">The connection id to be closed.</param>
        public void CloseConnection(Guid connectionId)
        {
            if (!this.clients.ContainsKey(connectionId))
            {
                return;
            }

            TcpClient client = this.clients[connectionId];
            client.Close();
            this.clients.Remove(connectionId);
        }

        /// <summary>
        /// Gets data from the server.
        /// </summary>
        /// <param name="connectionId">The connection id for which to get data.</param>
        /// <returns>
        /// A <see cref="DataPackage"/> containing the available data (if any).
        /// </returns>
        public DataPackage ReceiveData(Guid connectionId)
        {
            if (this.protocol == TunnelProtocolType.Tcp)
            {
                if (!this.clients.ContainsKey(connectionId))
                {
                    DataPackage data = new DataPackage();
                    data.Data = new byte[0];
                    data.HasData = false;
                    return data;
                }

                TcpClient client = this.clients[connectionId];
                NetworkStream stream = client.GetStream();

                if (stream.DataAvailable)
                {
                    byte[] data = new byte[DataPackage.BufferSize];
                    int readCount = stream.Read(data, 0, data.Length);
                    byte[] returnData = new byte[readCount];
                    Array.Copy(data, returnData, readCount);

                    DataPackage returnDataInfo = new DataPackage();
                    returnDataInfo.HasData = true;
                    returnDataInfo.Data = returnData;

                    return returnDataInfo;
                }
                else
                {
                    DataPackage returnData = new DataPackage();
                    returnData.HasData = false;
                    returnData.Data = new byte[0];
                    return returnData;
                }
            }
            else
            {
                if (this.protocol == TunnelProtocolType.UdpOutgoing)
                {
                    throw new InvalidOperationException("This is an outgoing UDP tunnel.");
                }

                if (this.udpClient.Available > 0)
                {
                    IPEndPoint remoteEndPoint = null;
                    byte[] data = this.udpClient.Receive(ref remoteEndPoint);
                    DataPackage returnData = new DataPackage();
                    returnData.HasData = true;
                    returnData.Data = data;
                    return returnData;
                }
                else
                {
                    DataPackage returnData = new DataPackage();
                    returnData.HasData = false;
                    returnData.Data = new byte[0];
                    return returnData;
                }
            }
        }

        /// <summary>
        /// Sends data to a server, for a specific connection.
        /// </summary>
        /// <param name="connectionId">The connection id for which to send data.</param>
        /// <param name="data">The data to be sent to the actual server.</param>
        /// <returns>
        /// A DataPackage containing any data that was available on the server after the send was done.
        /// </returns>
        public DataPackage SendData(Guid connectionId, DataPackage data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (this.protocol == TunnelProtocolType.Tcp)
            {
                if (!this.clients.ContainsKey(connectionId))
                {
                    DataPackage dataInfo = new DataPackage();
                    dataInfo.Data = new byte[0];
                    dataInfo.HasData = false;
                    return dataInfo;
                }

                if (data.HasData)
                {
                    NetworkStream stream = this.clients[connectionId].GetStream();
                    stream.Write(data.Data, 0, data.Data.Length);
                }

                return this.ReceiveData(connectionId);
            }
            else
            {
                if (this.protocol == TunnelProtocolType.UdpIncoming)
                {
                    throw new InvalidOperationException("This is an incoming UDP tunnel.");
                }

                if (data.HasData)
                {
                    this.udpClient.Send(data.Data, data.Data.Length);
                }

                DataPackage dataInfo = new DataPackage();
                dataInfo.Data = new byte[0];
                dataInfo.HasData = false;
                return dataInfo;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.udpClient != null)
                {
                    this.udpClient.Close();
                }

                foreach (TcpClient tcpClient in this.clients.Values)
                {
                    if (tcpClient != null)
                    {
                        tcpClient.Close();
                    }
                }
            }
        }
    }
}
