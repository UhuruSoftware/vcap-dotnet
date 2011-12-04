using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.NatsClient
{
    /// <summary>
    /// Enum detailing the parse states for the NATS client.
    /// </summary>
    public enum ParseState
    {
        /// <summary>
        /// The client is waiting for a control line.
        /// </summary>
        AwaitingControlLine,
        /// <summary>
        /// The client received a message command, and now it's waiting for it.
        /// </summary>
        AwaitingMsgPayload,
    }
}
