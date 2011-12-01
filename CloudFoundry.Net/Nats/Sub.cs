using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace CloudFoundry.Net.Nats
{
    class NatsSub
    {
        public string Subject;
        public SubscribeCallback Callback;
        public int Received;
        public int Queue;
        public int Max;
        public Timer Timeout = null;
        public int Expected;
    }
}
