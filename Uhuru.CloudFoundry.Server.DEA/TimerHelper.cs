using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Uhuru.CloudFoundry.DEA
{
    public class TimerHelper
    {
        public delegate void TimerCallbackDelegate();

        public static void DelayedCall(double delay, TimerCallbackDelegate callback)
        {
            Timer newTimer = new Timer(Math.Max(delay, 1));
            newTimer.AutoReset = false;
            newTimer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs args)
            {
                callback();
            });
            newTimer.Enabled = true;
        }

        public static Timer RecurringCall(double delay, TimerCallbackDelegate callback)
        {
            Timer newTimer = new Timer(delay);
            newTimer.AutoReset = true;
            newTimer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs args)
            {
                callback();
            });
            newTimer.Enabled = true;
            return newTimer;
        }

        public static Timer RecurringLongCall(double delay, TimerCallbackDelegate callback)
        {
            Timer newTimer = new Timer(delay);
            newTimer.AutoReset = false;
            newTimer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs args)
            {
                callback();
                newTimer.Enabled = true;
            });
            newTimer.Enabled = true;
            return newTimer;
        }
    }
}
