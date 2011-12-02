using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Uhuru.Utilities
{
    public delegate void TimerCallback();

    public static class TimerHelper
    {
        public static Timer DelayedCall(double delay, TimerCallback callback)
        {
            Timer newTimer = new Timer(delay);
            newTimer.AutoReset = false;
            newTimer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs args)
                {
                    callback();
                });
            newTimer.Enabled = true;

            return newTimer;
        }

        public static Timer RecurringCall(double delay, TimerCallback callback)
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

        public static Timer RecurringLongCall(double delay, TimerCallback callback)
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
