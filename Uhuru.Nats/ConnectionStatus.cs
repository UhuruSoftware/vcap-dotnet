using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.NatsClient
{
    /// <summary>
    /// Enum detailing possible states for the NATS connection.
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// Connections is open.
        /// </summary>
        Open,
        /// <summary>
        /// Connection is closed.
        /// </summary>
        Closed,
        /// <summary>
        /// The connection is in an error state.
        /// </summary>
        Error,
        /// <summary>
        /// Connection is being rebuilt.
        /// </summary>
        Reconnecting,
        /// <summary>
        /// Connection is being rebuilt.
        /// </summary>
        Closing
    }
}
