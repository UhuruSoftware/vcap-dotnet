// -----------------------------------------------------------------------
// <copyright file="ClientEnd.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.HttpTunnel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.ServiceModel;
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
        private Thread tunnelRunner = null;

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
        /// <param name="protocol">The protocol to use (TCP or UDP).</param>
        public void Start(Uri remoteHttpUrl, int localPort, string localIP, TunnelProtocolType protocol)
        {
            switch (protocol)
            {
                case TunnelProtocolType.Tcp:
                    {
                        this.StartTCPTunnel(remoteHttpUrl, localPort, localIP);
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
            if (this.tunnelRunner != null)
            {
                if ((this.tunnelRunner.ThreadState & ThreadState.Unstarted) == 0)
                {
                    this.tunnelRunner.Join(5000);
                }

                if ((this.tunnelRunner.ThreadState & ThreadState.Stopped) == 0)
                {
                    this.tunnelRunner.Abort();
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
        private void StartTCPTunnel(Uri remoteHttpUrl, int localPort, string localIp)
        {
            TcpListener listener;
            EndpointAddress endpointAddress = new EndpointAddress(remoteHttpUrl);
            Dictionary<Guid, TcpClient> clients = new Dictionary<Guid, TcpClient>();

            listener = new TcpListener(IPAddress.Parse(localIp), localPort);
            listener.Start();
            this.tunnelRunner = new Thread(
            new ThreadStart(
                delegate
                {
                    using (ChannelFactory<ITunnel> channelFactory = new ChannelFactory<ITunnel>(new BasicHttpBinding(), endpointAddress))
                    {
                        ITunnel wcfTunnelChannel = channelFactory.CreateChannel(endpointAddress);

                        while (!this.closing)
                        {
                            TcpClient acceptedClient = listener.AcceptTcpClient();
                            Guid serverConnectionId = Guid.Empty;

                            try
                            {
                                serverConnectionId = wcfTunnelChannel.OpenConnection();
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
                                    while (!this.closing)
                                    {
                                        byte[] buffer = new byte[DataPackage.BufferSize];
                                        int readCount = 0;

                                        try
                                        {
                                            readCount = client.GetStream().Read(buffer, 0, buffer.Length);
                                        }
                                        catch (IOException ex)
                                        {
                                            SocketException socketException = ex.InnerException as SocketException;

                                            // the 10054 means connection reset by peer
                                            if (socketException != null && socketException.ErrorCode == 10054)
                                            {
                                                client.Close();

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
                                            client.Close();

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
                                            receiveData = wcfTunnelChannel.SendData(connectionId, sendDataPackage);
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
                                while (!this.closing && client.Connected)
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

            this.tunnelRunner.IsBackground = true;
            this.tunnelRunner.Start();
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

            this.tunnelRunner = new Thread(
            new ThreadStart(
                delegate
                {
                    using (ChannelFactory<ITunnel> channelFactory = new ChannelFactory<ITunnel>(new BasicHttpBinding(), endpointAddress))
                    {
                        ITunnel wcfTunnelChannel = channelFactory.CreateChannel(endpointAddress);

                        Guid serverConnectionId = Guid.Empty;
                        try
                        {
                            serverConnectionId = wcfTunnelChannel.OpenConnection();
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
                                        wcfTunnelChannel.SendData(connectionId, sendDataPackage);
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

            this.tunnelRunner.IsBackground = true;
            this.tunnelRunner.Start();
        }
    }
}