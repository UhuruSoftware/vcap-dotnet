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
        UdpOutgoing
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
        /// <returns>A DataPackage containing any data that was available on the server after the send was done.</returns>
        [OperationContract]
        DataPackage SendData(Guid connectionId, DataPackage data);

        /// <summary>
        /// Creates a TCP connection on the server.
        /// </summary>
        /// <returns>A GUID used to identify the connection for future send/receive or for close.</returns>
        [OperationContract]
        Guid OpenConnection();

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
        /// </summary>
        public const int BufferSize = 1024 * 16 * 6;

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
    }
}
