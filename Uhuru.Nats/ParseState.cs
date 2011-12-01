using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.NatsClient
{
    public enum ParseState
    {
        AwaitingControlLine,
        AwaitingMsgPayload,
    }
}
