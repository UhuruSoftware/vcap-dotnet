// -----------------------------------------------------------------------
// <copyright file="ClientEnd.cs" company="Uhuru Software, Inc.">
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
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// This is an enum detailing the possible states of a tunnel client end.
    /// </summary>
    public enum TunnelClientEndState
    {
        /// <summary>
        /// The client end object has been created, but it was never started.
        /// </summary>
        None,

        /// <summary>
        /// The client end is started and can be used for communication.
        /// </summary>
        Started,

        /// <summary>
        /// The client end has been stopped by a call to the <see cref="ClientEnd.Stop"/> method.
        /// </summary>
        Stopped,

        /// <summary>
        /// The server end of the tunnel went away.
        /// </summary>
        ServerUnavailable
    }

    /// <summary>
    /// This class can connect to a remote HTTP Tunnel endpoint and facilitate TCP/IP communication between a client.
    /// </summary>
    public class ClientEnd
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
        /// An object used for locking when closing connections.
        /// </summary>
        private readonly object closeLock = new object();

        /// <summary>
        /// Indicates whether the client is supposed to be closing.
        /// </summary>
        private volatile bool closing = false;

        /// <summary>
        /// Indicates the state of the client end tunnel.
        /// </summary>
        private TunnelClientEndState state = TunnelClientEndState.None;

        /// <summary>
        /// This list contains all the threads that are used to read/write to the actual client.
        /// </summary>
        private List<Thread> connectionThreads = new List<Thread>();

        /// <summary>
        /// This is the thread that runs the tunnel listener.
        /// </summary>
        private List<Thread> tunnelRunners = new List<Thread>();

        /// <summary>
        /// The protocol of this tunnel.
        /// </summary>
        private TunnelProtocolType protocol;

        /// <summary>
        /// Gets the state of the client.
        /// </summary>
        public TunnelClientEndState State
        {
            get
            {
                return this.state;
            }
        }

        /// <summary>
        /// Starts the client end of the tunnel.
        /// </summary>
        /// <param name="remoteHttpUrl">The remote HTTP URL of the WCF service hosting the server end of the tunnel.</param>
        /// <param name="localPort">The local port to open.</param>
        /// <param name="localIP">The local IP on which to listen for connections (if it's a TCP tunnel).</param>
        /// <param name="tunnelProtocol">The protocol to use (TCP or UDP).</param>
        public void Start(Uri remoteHttpUrl, int localPort, string localIP, TunnelProtocolType tunnelProtocol)
        {
            this.protocol = tunnelProtocol;

            switch (tunnelProtocol)
            {
                case TunnelProtocolType.Ftp:
                case TunnelProtocolType.Tcp:
                    {
                        this.StartTCPTunnel(remoteHttpUrl, localPort, localIP, 0);
                    }

                    break;
                case TunnelProtocolType.UdpIncoming:
                    {
                        this.StartUDPTunnel(remoteHttpUrl, localPort, localIP, false);
                    }

                    break;
                case TunnelProtocolType.UdpOutgoing:
                    {
                        this.StartUDPTunnel(remoteHttpUrl, localPort, localIP, true);
                    }

                    break;
            }

            this.state = TunnelClientEndState.Started;
        }

        /// <summary>
        /// Stops the client end of the tunnel.
        /// </summary>
        public void Stop()
        {
            this.closing = true;
            if (this.tunnelRunners != null)
            {
                foreach (Thread tunnelRunner in this.tunnelRunners)
                {
                    if ((tunnelRunner.ThreadState & ThreadState.Unstarted) == 0)
                    {
                        tunnelRunner.Join(5000);
                    }

                    if ((tunnelRunner.ThreadState & ThreadState.Stopped) == 0)
                    {
                        tunnelRunner.Abort();
                    }
                }
            }

            foreach (Thread thread in this.connectionThreads)
            {
                if ((thread.ThreadState & ThreadState.Unstarted) == 0)
                {
                    thread.Join(500);
                }

                if ((thread.ThreadState & ThreadState.Stopped) == 0)
                {
                    thread.Abort();
                }
            }

            this.state = TunnelClientEndState.Stopped;
        }

        /// <summary>
        /// Starts a TCP tunnel.
        /// </summary>
        /// <param name="remoteHttpUrl">The remote HTTP URL.</param>
        /// <param name="localPort">The local port.</param>
        /// <param name="localIp">The local IP.</param>
        /// <param name="remotePort">The remote port that the tunnel is intended for. Only used for FTP protocol.</param>
        private void StartTCPTunnel(Uri remoteHttpUrl, int localPort, string localIp, int remotePort)
        {
            TcpListener listener;
            EndpointAddress endpointAddress = new EndpointAddress(remoteHttpUrl);
            Dictionary<Guid, TcpClient> clients = new Dictionary<Guid, TcpClient>();

            listener = new TcpListener(IPAddress.Parse(localIp), localPort);
            listener.Start();
            Thread newThread = new Thread(
            new ThreadStart(
                delegate
                {
                    BasicHttpBinding basicBinding = new BasicHttpBinding();
                    basicBinding.ReaderQuotas.MaxArrayLength = DataPackage.BufferSize;
                    basicBinding.ReaderQuotas.MaxBytesPerRead = DataPackage.BufferSize * 16;
                    basicBinding.MaxReceivedMessageSize = DataPackage.BufferSize * 16;
                    basicBinding.MaxBufferPoolSize = DataPackage.BufferSize * 16;
                    basicBinding.MaxBufferSize = DataPackage.BufferSize * 16;

                    using (ChannelFactory<ITunnel> channelFactory = new ChannelFactory<ITunnel>(basicBinding, endpointAddress))
                    {
                        ITunnel wcfTunnelChannel = channelFactory.CreateChannel(endpointAddress);

                        while (!this.closing)
                        {
                            TcpClient acceptedClient = listener.AcceptTcpClient();
                            Guid serverConnectionId = Guid.Empty;

                            try
                            {
                                serverConnectionId = wcfTunnelChannel.OpenConnection(remotePort);
                            }
                            catch (CommunicationException ex)
                            {
                                this.state = TunnelClientEndState.ServerUnavailable;
                                Logger.Warning(TunnelErrorMessages.CouldNotOpenConnection, ex.ToString());
                                return;
                            }

                            clients[serverConnectionId] = acceptedClient;

                            System.Threading.Thread threadReader = new System.Threading.Thread(
                                new System.Threading.ParameterizedThreadStart(delegate(object objConnectionId)
                                {
                                    Guid connectionId = (Guid)objConnectionId;
                                    TcpClient client = clients[connectionId];
                                    while (!this.closing && client != null && client.Connected)
                                    {
                                        byte[] buffer = new byte[DataPackage.BufferSize];
                                        int readCount = 0;

                                        try
                                        {
                                            readCount = client.GetStream().Read(buffer, 0, buffer.Length);
                                        }
                                        catch (SocketException socketException)
                                        {
                                            // we have closed the connection
                                            if (socketException.ErrorCode == 10004)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                throw;
                                            }
                                        }
                                        catch (IOException ex)
                                        {
                                            SocketException socketException = ex.InnerException as SocketException;

                                            // the 10054 means connection reset by peer; 10004 was closed by us
                                            if (socketException != null && (socketException.ErrorCode == 10054 || socketException.ErrorCode == 10004))
                                            {
                                                lock (this.closeLock)
                                                {
                                                    if (client != null)
                                                    {
                                                        client.Close();
                                                        client = null;
                                                    }
                                                }

                                                try
                                                {
                                                    wcfTunnelChannel.CloseConnection(connectionId);
                                                }
                                                catch (CommunicationException comEx)
                                                {
                                                    this.state = TunnelClientEndState.ServerUnavailable;
                                                    Logger.Warning(TunnelErrorMessages.CouldNotCloseConnection, comEx.ToString());
                                                    return;
                                                }

                                                break;
                                            }
                                            else
                                            {
                                                throw;
                                            }
                                        }

                                        if (readCount == 0)
                                        {
                                            lock (this.closeLock)
                                            {
                                                if (client != null)
                                                {
                                                    client.Close();
                                                    client = null;
                                                }
                                            }

                                            try
                                            {
                                                wcfTunnelChannel.CloseConnection(connectionId);
                                            }
                                            catch (CommunicationException comEx)
                                            {
                                                this.state = TunnelClientEndState.ServerUnavailable;
                                                Logger.Warning(TunnelErrorMessages.CouldNotCloseConnection, comEx.ToString());
                                                return;
                                            }

                                            break;
                                        }

                                        byte[] sendData = new byte[readCount];
                                        Array.Copy(buffer, sendData, readCount);

                                        DataPackage sendDataPackage = new DataPackage();
                                        sendDataPackage.Data = sendData;
                                        sendDataPackage.HasData = true;

                                        DataPackage receiveData = null;
                                        try
                                        {
                                            // this improves performance on ftp uploads
                                            bool alsoRead = !(this.protocol == TunnelProtocolType.Ftp && remotePort != 0);
                                            receiveData = wcfTunnelChannel.SendData(connectionId, sendDataPackage, alsoRead);
                                        }
                                        catch (CommunicationException comEx)
                                        {
                                            this.state = TunnelClientEndState.ServerUnavailable;
                                            Logger.Warning(TunnelErrorMessages.CouldNotSendData, comEx.ToString());
                                            return;
                                        }

                                        if (receiveData.HasData)
                                        {
                                            client.GetStream().Write(receiveData.Data, 0, receiveData.Data.Length);
                                        }
                                    }
                                }));
                            threadReader.IsBackground = true;
                            this.connectionThreads.Add(threadReader);
                            threadReader.Start(serverConnectionId);

                            System.Threading.Thread threadWriter = new System.Threading.Thread(
                            new System.Threading.ParameterizedThreadStart(delegate(object objConnectionId)
                            {
                                Guid connectionId = (Guid)objConnectionId;
                                TcpClient client = clients[connectionId];
                                while (!this.closing && client != null && client.Connected)
                                {
                                    DataPackage data = null;
                                    try
                                    {
                                        data = wcfTunnelChannel.ReceiveData(connectionId);
                                    }
                                    catch (CommunicationException comEx)
                                    {
                                        this.state = TunnelClientEndState.ServerUnavailable;
                                        Logger.Warning(TunnelErrorMessages.CouldNotReceiveData, comEx.ToString());
                                        return;
                                    }

                                    if (data.IsClosed)
                                    {
                                        lock (this.closeLock)
                                        {
                                            if (client != null)
                                            {
                                                client.Close();
                                                client = null;
                                                break;
                                            }
                                        }
                                    }

                                    if (data.HasData)
                                    {
                                        // We are taking things into our own hands for the control FTP connection.
                                        if (this.protocol == TunnelProtocolType.Ftp && remotePort == 0)
                                        {
                                            string stringData = ASCIIEncoding.ASCII.GetString(data.Data);

                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.Write(stringData);

                                            if (Regex.IsMatch(stringData, PassvReplyRegex))
                                            {
                                                string passvReply = Regex.Match(stringData, PassvReplyRegex).Value;
                                                string ipAndPort = Regex.Match(passvReply, PassvReplyIPAndPortRegex).Value;
                                                ipAndPort = ipAndPort.Substring(1, ipAndPort.Length - 2);

                                                string[] parts = ipAndPort.Split(',');

                                                byte ip1 = byte.Parse(parts[0], CultureInfo.InvariantCulture);
                                                byte ip2 = byte.Parse(parts[1], CultureInfo.InvariantCulture);
                                                byte ip3 = byte.Parse(parts[2], CultureInfo.InvariantCulture);
                                                byte ip4 = byte.Parse(parts[3], CultureInfo.InvariantCulture);
                                                byte port1 = byte.Parse(parts[4], CultureInfo.InvariantCulture);
                                                byte port2 = byte.Parse(parts[5], CultureInfo.InvariantCulture);

                                                int ephemeralPort = NetworkInterface.GrabEphemeralPort();

                                                this.StartTCPTunnel(remoteHttpUrl, ephemeralPort, "127.0.0.1", (port1 * 256) + port2);

                                                port1 = (byte)(ephemeralPort / 256);
                                                port2 = (byte)(ephemeralPort % 256);

                                                string tunnelPassvReply = string.Format("(127,0,0,1,{0},{1})", port1, port2);
                                                passvReply = Regex.Replace(passvReply, PassvReplyIPAndPortRegex, tunnelPassvReply);
                                                stringData = Regex.Replace(stringData, PassvReplyRegex, passvReply);
                                                data.Data = ASCIIEncoding.ASCII.GetBytes(stringData);

                                                Console.ForegroundColor = ConsoleColor.Blue;
                                                Console.Write(stringData);
                                            }
                                        }

                                        if (this.protocol == TunnelProtocolType.Ftp && remotePort != 0)
                                        {
                                            string stringData = ASCIIEncoding.ASCII.GetString(data.Data);

                                            Console.ForegroundColor = ConsoleColor.Cyan;
                                            Console.Write(stringData);
                                        }

                                        if (client != null)
                                        {
                                            client.GetStream().Write(data.Data, 0, data.Data.Length);
                                        }
                                    }
                                }
                            }));
                            threadWriter.IsBackground = true;
                            this.connectionThreads.Add(threadWriter);
                            threadWriter.Start(serverConnectionId);
                        }
                    }
                }));

            newThread.IsBackground = true;
            newThread.Start();
            this.tunnelRunners.Add(newThread);
        }

        /// <summary>
        /// Starts a UDP tunnel.
        /// </summary>
        /// <param name="remoteHttpUrl">The remote HTTP URL.</param>
        /// <param name="localPort">The local port.</param>
        /// <param name="localIp">The local ip.</param>
        /// <param name="listening">True if the UDP socket is listening mode.</param>
        private void StartUDPTunnel(Uri remoteHttpUrl, int localPort, string localIp, bool listening)
        {
            UdpClient listener;
            EndpointAddress endpointAddress = new EndpointAddress(remoteHttpUrl);
            listener = new UdpClient();
            if (listening)
            {
                listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                listener.Client.Bind(new IPEndPoint(IPAddress.Parse(localIp), localPort));
            }
            else
            {
                listener.Connect(new IPEndPoint(IPAddress.Parse(localIp), localPort));
            }

            Thread newThread = new Thread(
            new ThreadStart(
                delegate
                {
                    BasicHttpBinding basicBinding = new BasicHttpBinding();
                    basicBinding.ReaderQuotas.MaxArrayLength = 98304;
                    basicBinding.ReaderQuotas.MaxBytesPerRead = 10240000;
                    basicBinding.MaxReceivedMessageSize = 10240000;
                    basicBinding.MaxBufferPoolSize = 10240000;
                    basicBinding.MaxBufferSize = 10240000;

                    using (ChannelFactory<ITunnel> channelFactory = new ChannelFactory<ITunnel>(basicBinding, endpointAddress))
                    {
                        ITunnel wcfTunnelChannel = channelFactory.CreateChannel(endpointAddress);

                        Guid serverConnectionId = Guid.Empty;
                        try
                        {
                            serverConnectionId = wcfTunnelChannel.OpenConnection(0);
                        }
                        catch (CommunicationException comEx)
                        {
                            this.state = TunnelClientEndState.ServerUnavailable;
                            Logger.Warning(TunnelErrorMessages.CouldNotOpenConnection, comEx.ToString());
                            return;
                        }

                        System.Threading.Thread threadReader = new System.Threading.Thread(
                            new System.Threading.ParameterizedThreadStart(delegate(object objConnectionId)
                            {
                                Guid connectionId = (Guid)objConnectionId;
                                while (!this.closing)
                                {
                                    byte[] buffer;

                                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, localPort);
                                    buffer = listener.Receive(ref remoteEndPoint);

                                    DataPackage sendDataPackage = new DataPackage();
                                    sendDataPackage.Data = buffer;
                                    sendDataPackage.HasData = true;

                                    try
                                    {
                                        wcfTunnelChannel.SendData(connectionId, sendDataPackage, true);
                                    }
                                    catch (CommunicationException comEx)
                                    {
                                        this.state = TunnelClientEndState.ServerUnavailable;
                                        Logger.Warning(TunnelErrorMessages.CouldNotSendData, comEx.ToString());
                                        return;
                                    }
                                }
                            }));

                        threadReader.IsBackground = true;
                        this.connectionThreads.Add(threadReader);
                        if (listening)
                        {
                            threadReader.Start(serverConnectionId);
                        }

                        System.Threading.Thread threadWriter = new System.Threading.Thread(
                        new System.Threading.ParameterizedThreadStart(delegate(object objConnectionId)
                        {
                            Guid connectionId = (Guid)objConnectionId;
                            while (!this.closing)
                            {
                                DataPackage data = null;
                                try
                                {
                                    data = wcfTunnelChannel.ReceiveData(connectionId);
                                }
                                catch (CommunicationException comEx)
                                {
                                    this.state = TunnelClientEndState.ServerUnavailable;
                                    Logger.Warning(TunnelErrorMessages.CouldNotReceiveData, comEx.ToString());
                                    return;
                                }

                                if (data.HasData)
                                {
                                    if (listener != null)
                                    {
                                        listener.Send(data.Data, data.Data.Length);
                                    }
                                }
                            }
                        }));

                        threadWriter.IsBackground = true;
                        this.connectionThreads.Add(threadWriter);
                        if (!listening)
                        {
                            threadWriter.Start(serverConnectionId);
                        }

                        while (!this.closing)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }));

            newThread.IsBackground = true;
            newThread.Start();
            this.tunnelRunners.Add(newThread);
        }
    }
}