// -----------------------------------------------------------------------
// <copyright file="Lock.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase.Worker.AsyncJob
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BookSleeve;
    using Uhuru.Utilities;
    using System.Globalization;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Lock : IDisposable
    {
            public string Name
            {
                get;
                private set;
            }

            public int Timeout
            {
                get;
                private set;
            }

            /// <summary>
            /// Lock expires in given seconds if not refreshed
            /// </summary>
            public int Expiration
            {
                get;
                private set;
            }

            public int TTL
            {
                get;
                private set;
            }

            public bool Acquired
            {
                get;
                private set;
            }

            public event EventHandler OnTtlExpired;
            public event EventHandler OnRefreshError;

            private RedisConnection redisConnection;
            private decimal lockExpiration;

            // Keep the lock until the abort is complete
            private bool releaseLockAfterAbort;

            private Timer refreshTimer;
            private Timer ttlExpirationTimer;

            private object membersLock;


            private static decimal GetUnixTime()
            {
                return (decimal)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            }

            private static string ToRedisTime(decimal seconds)
            {
                return seconds.ToString(CultureInfo.InvariantCulture);
            }

            private static decimal FromRedisTime(string redisTime)
            {
                return decimal.Parse(redisTime, CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Redis locking primitive using SETNX.
            /// http://redis.io/commands/setnx
            /// Maintain compatibility with: 
            /// https://github.com/cloudfoundry/vcap-services-base/blob/master/lib/base/job/lock.rb
            /// </summary>
            /// <param name="redis">The Redis connection to use. The connection has to be open and ready to use.</param>
            /// <param name="name">The UUID of the lock.</param>
            /// <param name="timeout">The time, in seconds, to wait to acquire the lock.</param>
            /// <param name="expiration">Lock expires in given seconds if not refreshed.</param>
            /// <param name="ttl">The max time, in seconds, a thread can acquire a lock. Lock raise +JOB_TIMEOUT+ once the ttl is exceeded.</param>
            public Lock(RedisConnection redis, string name, int timeout = 20, int expiration = 10, int ttl = 600)
            {
                this.redisConnection = redis;
                this.Name = name;
                this.Timeout = timeout;
                this.Expiration = expiration;
                this.TTL = ttl;

                this.Acquired = false;
                this.lockExpiration = 0;

                this.membersLock = new object();
                this.releaseLockAfterAbort = true;
            }

            /// <summary>
            /// Acquire the lock.
            /// </summary>
            public void Lock()
            {
                lock (membersLock)
                {
                    Logger.Debug("Acquiring Lock: {0}", this.Name);

                    decimal startTime = GetUnixTime();
                    decimal expirationTime = startTime + this.Expiration + 1;

                    // If the lock key is set, watch it for <timeout> seconds and try to acquire if it becomes expired.
                    while (!redisConnection.Strings.SetIfNotExists(0, this.Name, ToRedisTime(expirationTime)).Result)
                    {
                        string expirationLock = redisConnection.Strings.GetString(0, this.Name).Result;
                        decimal existingLockTime;
                        if (expirationLock == null)
                        {
                            // The expiration key may be null if it has been deleted by another lock
                            // Set it to 0 so that the lock can be acquired.
                            existingLockTime = 0;
                        }
                        else
                        {
                            existingLockTime = FromRedisTime(expirationLock);
                        }

                        using (var transaction = redisConnection.CreateTransaction())
                        {
                            // This condition will make another GET on the Name key.
                            // It should not be a that big of a performance impact.
                            // Improve this check if newer versions of Booksleeve will make is possible.
                            transaction.AddCondition(Condition.KeyEquals(0, this.Name, ToRedisTime(existingLockTime)));

                            if (existingLockTime < GetUnixTime())
                            {
                                // fault injection code
                                // redisConnection.Strings.Set(0, this.Name, GetUnixTime().ToString(CultureInfo.InvariantCulture)).Wait();

                                transaction.Strings.Set(0, this.Name, ToRedisTime(expirationTime));

                                Logger.Debug("Redis lock {0} is expired, trying to acquire it.", this.Name);

                                bool setResult = transaction.Execute().Result;

                                if (setResult)
                                {
                                    Logger.Debug("Redis lock {0} is renewed and acquired.", this.Name);
                                    break;
                                }
                                else
                                {
                                    Logger.Debug("Redis lock {0} was updated by others.", this.Name);
                                }
                            }
                        }

                        if (GetUnixTime() - startTime > this.Timeout)
                        {
                            throw new Exception(string.Format(CultureInfo.InvariantCulture, "Redis lock timeout after waiting for {0} seconds", this.Timeout));
                        }

                        Thread.Sleep(1000);

                        // Update the expiration time for the lock
                        expirationTime = GetUnixTime() + this.Expiration + 1;
                    }

                    this.Acquired = true;
                    this.lockExpiration = expirationTime;

                    this.ActivateRefreshTimer();
                    this.ActivateTtlExpirationTimer();

                    Logger.Debug(string.Format(CultureInfo.InvariantCulture, "Redis lock {0} is acquired, will expire at {1}", this.Name, this.lockExpiration.ToString()));



                    long ttlExpirationMs = this.TTL * 1000L;
                    ttlExpirationTimer.Change(ttlExpirationMs, 0);
                }
            }

            private void ActivateRefreshTimer()
            {
                if (this.refreshTimer == null)
                {
                    refreshTimer = new Timer(RefreshHandler);
                }

                refreshTimer.Change(Math.Max(1, this.Expiration / 2) * 1000L, 0);
            }

            private void RefreshHandler(Object state)
            {
                lock (this.membersLock)
                {
                    if (!this.Acquired) return;

                    try
                    {
                        Logger.Debug(String.Format(CultureInfo.InvariantCulture, "Renewing Redis lock {0}", this.Name));

                        string expirationLock = redisConnection.Strings.GetString(0, this.Name).Result;
                        decimal existingLockTime;
                        if (expirationLock == null)
                        {
                            // The expiration key may be null if it has been deleted by another lock
                            // Set it to max value so that the lock can be acquired.
                            existingLockTime = decimal.MaxValue;
                        }
                        else
                        {
                            existingLockTime = FromRedisTime(expirationLock);
                        }

                        using (var transaction = redisConnection.CreateTransaction())
                        {
                            transaction.AddCondition(Condition.KeyEquals(0, this.Name, ToRedisTime(existingLockTime)));

                            if (existingLockTime > this.lockExpiration)
                            {
                                Logger.Debug("Redis lock {0} was updated by others.", this.Name);
                                this.Acquired = false;
                                this.Release();
                                if (this.OnRefreshError != null)
                                {
                                    this.OnRefreshError.Invoke(this, null);
                                }
                                return;
                            }

                            decimal expirationTime = GetUnixTime() + this.Expiration + 1;
                            string expiration = expirationTime.ToString(CultureInfo.InvariantCulture);

                            transaction.Strings.Set(0, this.Name, expiration);

                            bool setResult = transaction.Execute().Result;

                            if (setResult)
                            {
                                Logger.Debug("Redis lock {0} is renewed and acquired.", this.Name);
                            }
                            else
                            {
                                Logger.Debug("Redis lock {0} was updated by others.", this.Name);
                                this.Acquired = false;
                                this.Release();
                                if (this.OnRefreshError != null)
                                {
                                    this.OnRefreshError.Invoke(this, null);
                                }
                                return;
                            }

                            this.lockExpiration = expirationTime;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(String.Format(CultureInfo.InvariantCulture, "Can't renew Redis lock {0}, Exception: {1}", this.Name, e.ToString()));
                        this.Acquired = false;
                        this.Release();
                        if (this.OnRefreshError != null)
                        {
                            this.OnRefreshError.Invoke(this, null);
                        }
                    }

                    // Renew the timer
                    if (this.Acquired)
                    {
                        this.ActivateRefreshTimer();
                    }
                }
            }

            private void ActivateTtlExpirationTimer()
            {
                this.ttlExpirationTimer = new Timer(TtlExpirationHandler);
            }

            private void TtlExpirationHandler(Object state)
            {
                lock (this.membersLock)
                {
                    if (!this.Acquired) return;

                    if (this.releaseLockAfterAbort)
                    {
                        if (this.OnTtlExpired != null)
                        {
                            this.OnTtlExpired.Invoke(this, null);
                        }

                        this.Release();
                    }
                    else
                    {
                        this.Release();

                        if (this.OnTtlExpired != null)
                        {
                            this.OnTtlExpired.Invoke(this, null);
                        }
                    }
                }
            }

            public bool IsReleased()
            {
                return !this.Acquired;
            }

            public void Release()
            {
                lock (membersLock)
                {
                    if (this.Acquired)
                    {
                        Logger.Debug("Deleting Redis lock: {1}", this.Name);
                        string expirationLock = redisConnection.Strings.GetString(0, this.Name).Result;
                        decimal existingLockTime;
                        if (expirationLock == null)
                        {
                            // The expiration key may be null if it has been deleted by another lock
                            // Set it to max value so that the lock can be acquired.
                            existingLockTime = decimal.MaxValue;
                        }
                        else
                        {
                            existingLockTime = FromRedisTime(expirationLock);
                        }

                        using (var transaction = redisConnection.CreateTransaction())
                        {
                            transaction.AddCondition(Condition.KeyEquals(0, this.Name, ToRedisTime(existingLockTime)));

                            if (existingLockTime > this.lockExpiration)
                            {
                                Logger.Debug("Redis lock {0} is acquired by others.", this.Name);
                            }
                            else
                            {
                                transaction.Keys.Remove(0, this.Name);

                                bool removeResult = transaction.Execute().Result;

                                if (removeResult)
                                {
                                    Logger.Debug("Redis lock {0} is deleted.", this.Name);
                                }
                                else
                                {
                                    Logger.Debug("Redis lock {0} is acquired by others.", this.Name);
                                }
                            }
                        }
                    }

                    this.Acquired = false;

                    if (this.refreshTimer != null)
                    {
                        this.refreshTimer.Change(0, 0);
                    };

                    if (this.ttlExpirationTimer != null)
                    {
                        this.ttlExpirationTimer.Change(0, 0);
                    }

                }
            }

            public delegate void Block();

            public void ExecuteWithinLock(Block block)
            {
                lock (membersLock)
                {
                    if (this.Acquired)
                    {
                        throw new Exception("Redis lock is acquired.");
                    }

                    this.Lock();

                    try
                    {
                        var thread = new Thread(new ThreadStart(block.Invoke));

                        thread.Start();

                        // TOTD: abort on lock refresh error.
                        thread.Join(this.TTL * 1000);

                        if (thread.IsAlive)
                        {

                            try
                            {
                                thread.Abort();
                                thread.Join();
                            }
                            catch (ThreadStateException)
                            {
                            }
                        }
                    }
                    finally
                    {
                        this.Release();
                    }

                    // ThreadPool.QueueUserWorkItem((WaitCallback)block);
                    //new Worker
                    //block.Invoke();
                }
            }

            public void Dispose()
            {
                lock (membersLock)
                {
                    this.Release();

                    if (this.refreshTimer != null)
                    {
                        this.refreshTimer.Dispose();
                        this.refreshTimer = null;
                    };

                    if (this.ttlExpirationTimer != null)
                    {
                        this.ttlExpirationTimer.Dispose();
                        this.refreshTimer = null;
                    }
                }
            }
        }
}
