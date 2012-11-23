// -----------------------------------------------------------------------
// <copyright file="Lock.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase.Worker.AsyncJob
{
    using System;
    using BookSleeve;
    using System.Threading;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Lock : IDisposable
    {
        public delegate void LockExpired();
        public event LockExpired RaiseLockExpired;

        public Lock(string name)
        {
            new Thread(delegate()
            {
                Thread.Sleep(4000);
                RaiseLockExpired();
            }).Start();
        }


        public void Dispose()
        {
            
        }
    }
}
