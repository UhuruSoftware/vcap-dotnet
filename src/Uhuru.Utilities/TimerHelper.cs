// -----------------------------------------------------------------------
// <copyright file="TimerHelper.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Timers;
    
    /// <summary>
    /// Delegate definition that refers to a method to be called when a timer tick occurs.
    /// </summary>
    public delegate void TimerCallback();

    /// <summary>
    /// This is a helper class that provides the developer with easy methods of creating timers.
    /// </summary>
    public static class TimerHelper
    {
        /// <summary>
        /// This method creates a timer that ticks only once and runs a callback method.
        /// This method is non-blocking.
        /// </summary>
        /// <param name="delay">A double specifying the amount of time to sleep before calling the callback method.</param>
        /// <param name="callback">A method that gets called when the timer ticks.</param>
        /// <returns>The timer that is used to delay the call to the callback method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "To fail fast on every exception.")]
        public static Timer DelayedCall(double delay, TimerCallback callback)
        {
            Timer newTimer = null;
            Timer returnTimer = null;
            try
            {
                newTimer = new Timer(Math.Max(delay, 1));
                newTimer.AutoReset = false;
                newTimer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs args)
                    {
                        try
                        {
                            callback();
                        }
                        catch (Exception ex)
                        {
                            Logger.Fatal(Strings.UnhandledExceptionCaught, ex.ToString());
                            Environment.FailFast(Strings.UnhandledExceptionCaught2 + "\r\n" + ex.ToString());
                        }
                    });
                newTimer.Enabled = true;
                returnTimer = newTimer;
                newTimer = null;
            }
            finally
            {
                if (newTimer != null)
                {
                    newTimer.Close();
                }
            }

            return returnTimer;
        }

        /// <summary>
        /// This method creates a timer that ticks forever, and on each tick it calls a callback method.
        /// This method is non-blocking.
        /// </summary>
        /// <param name="delay">A double specifying the interval between each tick.</param>
        /// <param name="callback">A method that gets called when the timer ticks.</param>
        /// <returns>The timer that is created.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "To fail fast on every exception.")]
        public static Timer RecurringCall(double delay, TimerCallback callback)
        {
            Timer newTimer = null;
            Timer returnTimer = null;

            try
            {
                newTimer = new Timer(delay);
                newTimer.AutoReset = true;
                newTimer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs args)
                {
                    try
                    {
                        callback();
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(Strings.UnhandledExceptionCaught, ex.ToString());
                        Environment.FailFast(Strings.UnhandledExceptionCaught2 + "\r\n" + ex.ToString());
                    }
                });
                newTimer.Enabled = true;
                returnTimer = newTimer;
                newTimer = null;
            }
            finally
            {
                if (newTimer != null)
                {
                    newTimer.Close();
                }
            }

            return returnTimer;
        }

        /// <summary>
        /// This method creates a timer that ticks once, and on each tick it calls a callback method.
        /// After each call to the callback method, the timer is reset.
        /// This method is non-blocking.
        /// </summary>
        /// <param name="delay">A double specifying the interval between each call of the callback method.</param>
        /// <param name="callback">A method that gets called when the timer ticks.</param>
        /// <returns>The timer that is created.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "To fail fast on every exception.")]
        public static Timer RecurringLongCall(double delay, TimerCallback callback)
        {
            object disposedLock = new object();
            bool isDisposed = false;
            Timer newTimer = new Timer(delay);

            newTimer.Disposed += new EventHandler(
                (e, args) =>
                {
                    lock (disposedLock)
                    {
                        isDisposed = true;
                    }
                });

            newTimer.AutoReset = false;

            newTimer.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs args)
            {
                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    Logger.Fatal(Strings.UnhandledExceptionCaught, ex.ToString());
                    Environment.FailFast(Strings.UnhandledExceptionCaught2 + "\r\n" + ex.ToString());
                }

                lock (disposedLock)
                {
                    if (newTimer != null && !isDisposed)
                    {
                        newTimer.Enabled = true;
                    }
                }
            });
            newTimer.Enabled = true;
            return newTimer;
        }
    }
}
