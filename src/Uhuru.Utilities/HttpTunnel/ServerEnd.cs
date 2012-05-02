// -----------------------------------------------------------------------
// <copyright file="ServerEnd.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.HttpTunnel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This class implements the server part of an HTTP tunnel.
    /// It is used in conjunction with a WCF service.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ServerEnd : ITunnel, IDisposable
    {
        /// <summary>
        /// Regex matching a server reply to a passv command.
        /// </summary>
        private const string PassvReplyRegex = @"227.*\(\d*,\d*,\d*,\d*,\d*,\d*\)";

        /// <summary>
        /// Regex matching the IP and port parts of a server reply to a passv command.
        /// </summary>
        private const string PassvReplyIPAndPortRegex = @"\(\d*,\d*,\d*,\d*,\d*,\d*\)";

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
        /// Specifies the type of tunnel (TCP or UDP or FTP).
        /// </summary>
        private TunnelProtocolType protocol;

        /// <summary>
        /// List of ports opened by the server to facilitate file transfers.
        /// </summary>
        private List<int> ftpPassivePorts;

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
                case TunnelProtocolType.Ftp:
                    {
                        this.ftpPassivePorts = new List<int>();
                        this.clients = new Dictionary<Guid, TcpClient>();
                    }

                    break;
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
        /// <param name="remotePort">Port to connect to on the real server. Only used for FTP connections.</param>
        /// <returns>
        /// A GUID used to identify the connection for future send/receive or for close.
        /// </returns>
        public Guid OpenConnection(int remotePort)
        {
            if (this.protocol == TunnelProtocolType.Tcp)
            {
                TcpClient client = null;
                try
                {
                    client = new TcpClient(this.host, this.port);
                    Guid connectionId = Guid.NewGuid();
                    this.clients.Add(connectionId, client);
                    return connectionId;
                }
                catch
                {
                    if (client != null)
                    {
                        client.Close();
                    }

                    throw;
                }
            }
            else if (this.protocol == TunnelProtocolType.Ftp)
            {
                if (remotePort != 0)
                {
                    if (!this.ftpPassivePorts.Contains(remotePort))
                    {
                        throw new WebFaultException<string>("Specified port is invalid.", HttpStatusCode.Forbidden);
                    }
                }

                TcpClient client = null;

                try
                {
                    client = new TcpClient(this.host, remotePort == 0 ? this.port : remotePort);
                    Guid connectionId = Guid.NewGuid();
                    this.clients.Add(connectionId, client);
                    return connectionId;
                }
                catch
                {
                    if (client != null)
                    {
                        client.Close();
                    }

                    throw;
                }
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
            if (this.protocol == TunnelProtocolType.Tcp || this.protocol == TunnelProtocolType.Ftp)
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

                if (stream.DataAvailable || (this.protocol == TunnelProtocolType.Ftp && (this.port != ((IPEndPoint)client.Client.RemoteEndPoint).Port)))
                {
                    byte[] data = new byte[DataPackage.BufferSize];
                    int readCount = 0;

                    try
                    {   
                        stream.ReadTimeout = 2000;
                        readCount = stream.Read(data, 0, data.Length);
                    }
                    catch (IOException ex)
                    {
                        SocketException socketException = ex.InnerException as SocketException;

                        // the 10060 error means timeout - we have no data right now
                        if (socketException != null && socketException.ErrorCode == 10060)
                        {
                            DataPackage closedData = new DataPackage();
                            closedData.HasData = false;
                            closedData.Data = new byte[0];
                            return closedData;
                        }

                        // the 10054 means connection reset by peer
                        if (socketException != null && socketException.ErrorCode == 10054)
                        {
                            client.Close();
                            this.clients.Remove(connectionId);

                            DataPackage closedData = new DataPackage();
                            closedData.HasData = false;
                            closedData.Data = new byte[0];
                            closedData.IsClosed = true;
                            return closedData;
                        }

                        throw;
                    }

                    if (readCount == 0)
                    {
                        client.Close();
                        this.clients.Remove(connectionId);

                        DataPackage closedData = new DataPackage();
                        closedData.HasData = false;
                        closedData.Data = new byte[0];
                        closedData.IsClosed = true;
                        return closedData;
                    }

                    byte[] returnData = new byte[readCount];
                    Array.Copy(data, returnData, readCount);

                    DataPackage returnDataInfo = new DataPackage();
                    returnDataInfo.HasData = true;

                    if (this.protocol == TunnelProtocolType.Ftp && this.port == ((IPEndPoint)client.Client.RemoteEndPoint).Port)
                    {
                        string stringData = ASCIIEncoding.ASCII.GetString(returnData);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(stringData);

                        if (Regex.IsMatch(stringData, PassvReplyRegex))
                        {
                            string passvReply = Regex.Match(stringData, PassvReplyRegex).Value;
                            string ipAndPort = Regex.Match(passvReply, PassvReplyIPAndPortRegex).Value;
                            ipAndPort = ipAndPort.Substring(1, ipAndPort.Length - 2);

                            string[] parts = ipAndPort.Split(',');

                            byte port1 = byte.Parse(parts[4], CultureInfo.InvariantCulture);
                            byte port2 = byte.Parse(parts[5], CultureInfo.InvariantCulture);

                            this.ftpPassivePorts.Add((port1 * 256) + port2);

                            string tunnelPassvReply = string.Format(CultureInfo.InvariantCulture, "(127,0,0,1,{0},{1})", port1, port2);
                            passvReply = Regex.Replace(passvReply, PassvReplyIPAndPortRegex, tunnelPassvReply);
                            stringData = Regex.Replace(stringData, PassvReplyRegex, passvReply);
                            returnData = ASCIIEncoding.ASCII.GetBytes(stringData);

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(stringData);
                        }
                    }

                    if (this.protocol == TunnelProtocolType.Ftp && this.port != ((IPEndPoint)client.Client.RemoteEndPoint).Port)
                    {
                        string stringData = ASCIIEncoding.ASCII.GetString(returnData);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(stringData);
                    }

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
        /// <param name="alsoRead">True if data should be read from the server after writing (may improve performance in some cases).</param>
        /// <returns>
        /// A DataPackage containing any data that was available on the server after the send was done.
        /// </returns>
        public DataPackage SendData(Guid connectionId, DataPackage data, bool alsoRead)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (this.protocol == TunnelProtocolType.Tcp || this.protocol == TunnelProtocolType.Ftp)
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
                    try
                    {
                        stream.Write(data.Data, 0, data.Data.Length);
                    }
                    catch (SocketException)
                    {
                        TcpClient client = null;

                        try
                        {
                            client = new TcpClient(this.host, ((IPEndPoint)this.clients[connectionId].Client.RemoteEndPoint).Port);
                            this.clients.Remove(connectionId);
                            this.clients.Add(connectionId, client);
                        }
                        catch
                        {
                            if (client != null)
                            {
                                client.Close();
                            }

                            throw;
                        }

                        stream = this.clients[connectionId].GetStream();
                        stream.Write(data.Data, 0, data.Data.Length);
                    }
                }

                if (alsoRead)
                {
                    return this.ReceiveData(connectionId);
                }
                else
                {
                    DataPackage dataInfo = new DataPackage();
                    dataInfo.Data = new byte[0];
                    dataInfo.HasData = false;
                    return dataInfo;
                }
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
