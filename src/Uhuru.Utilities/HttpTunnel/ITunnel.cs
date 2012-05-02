// -----------------------------------------------------------------------
// <copyright file="ITunnel.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.HttpTunnel
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    /// <summary>
    /// Defines possible protocol types to tunnel
    /// </summary>
    [DataContract]
    public enum TunnelProtocolType
    {
        /// <summary>
        /// Tunnel a TCP port
        /// </summary>
        [EnumMember]
        Tcp,

        /// <summary>
        /// Tunnel a UDP port (incoming data)
        /// </summary>
        [EnumMember]
        UdpIncoming,

        /// <summary>
        /// Tunnel a UDP port (outgoing data)
        /// </summary>
        [EnumMember]
        UdpOutgoing,

        /// <summary>
        /// Tunnel FTP traffic (only passive mode)
        /// </summary>
        [EnumMember]
        Ftp
    }

    /// <summary>
    /// This interface is the communication contract between the tunnel server and client.
    /// </summary>
    [ServiceContract]
    public interface ITunnel
    {
        /// <summary>
        /// Gets data from the server.
        /// </summary>
        /// <param name="connectionId">The connection id for which to get data.</param>
        /// <returns>A <see cref="DataPackage"/> containing the available data (if any).</returns>
        [OperationContract]
        DataPackage ReceiveData(Guid connectionId);

        /// <summary>
        /// Sends data to a server, for a specific connection.
        /// </summary>
        /// <param name="connectionId">The connection id for which to send data.</param>
        /// <param name="data">The data to be sent to the actual server.</param>
        /// <param name="alsoRead">True if data should be read from the server after writing (may improve performance in some cases).</param>
        /// <returns>A DataPackage containing any data that was available on the server after the send was done.</returns>
        [OperationContract]
        DataPackage SendData(Guid connectionId, DataPackage data, bool alsoRead);

        /// <summary>
        /// Creates a TCP connection on the server.
        /// </summary>
        /// <param name="remotePort">Remote port used for data transfer. FTP protocol only.</param>
        /// <returns>A GUID used to identify the connection for future send/receive or for close.</returns>
        [OperationContract]
        Guid OpenConnection(int remotePort);

        /// <summary>
        /// Closes a TCP connection on the server.
        /// </summary>
        /// <param name="connectionId">The connection id to be closed.</param>
        [OperationContract]
        void CloseConnection(Guid connectionId);
    }

    /// <summary>
    /// This class contains data about data that needs to be transferred between the tunnel server and client. 
    /// </summary>
    [DataContract]
    public class DataPackage
    {
        /// <summary>
        /// The buffer size to use for transferring data in the tunnel.
        /// If you change this value, make sure to also change server-side quotas as well.
        /// </summary>
        public const int BufferSize = 1024 * 1024;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPackage"/> class.
        /// </summary>
        public DataPackage()
        {
            this.IsClosed = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this package has data.
        /// </summary>
        [DataMember]
        public bool HasData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data for this package.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is a chunk of data"), 
        DataMember]
        public byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the connection is open. Only used for FTP data connections.
        /// </summary>
        [DataMember]
        public bool IsClosed
        {
            get;
            set;
        }
    }
}
