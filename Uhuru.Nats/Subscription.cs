using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Uhuru.NatsClient
{
    class Subscription
    {
        public string Subject
        { get; set; }
        public SubscribeCallback Callback
        { get; set; }
        public int Received
        { get; set; }
        public int Queue
        { get; set; }
        public int Max
        { get; set; }
        public Timer Timeout
        { get; set; }
        //public int Expected TODO:Mitza
        //{ get; set; }
    }
}
