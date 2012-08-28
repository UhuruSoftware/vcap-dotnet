// -----------------------------------------------------------------------
// <copyright file="DirectoryAccounting.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using Microsoft.Storage;

    /// <summary>
    /// Windows Disk Quota Utilites.
    /// Warning! This class uses potentail deprecated Windows Interfaces to work with Windows 2008 R2. After migrating to Windows 2012 use the WMI to manage FSRM.
    /// Quote from msdn.com http://msdn.microsoft.com/en-us/library/windows/desktop/bb613247(v=vs.85).aspx
    ///  "[This interface is supported for compatibility but it's recommended to use the FSRM WMI Classes to manage FSRM. Please see the MSFT_FSRMQuota class.]"
    /// </summary>
    public class DirectoryAccounting
    {
        /// <summary>
        /// Fsrm quota manager.
        /// </summary>
        private IFsrmQuotaManager quotaManager = new FsrmQuotaManagerClass();

        /// <summary>
        /// Lock for quotasCache.
        /// </summary>
        private object quotasCacheLock = new object();

        /// <summary>
        /// Cache for directory quotas.
        /// </summary>
        private Dictionary<string, IFsrmQuota> quotasCache = new Dictionary<string, IFsrmQuota>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryAccounting"/> class.
        /// </summary>
        public DirectoryAccounting()
        {
        }

        /// <summary>
        /// Enforces the quota.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="sizeBytes">The size bytes.</param>
        public void SetDirectoryQuota(string path, long sizeBytes)
        {
            path = CanonicalPath(path);
            IFsrmQuota dirQuota = this.GetDirectoryFsrmQuota(path);

            dirQuota.QuotaLimit = sizeBytes;
            dirQuota.QuotaFlags = 0x00000100; // Hard quota enforcement
            dirQuota.Commit();
        }

        /// <summary>
        /// Gets the directory quota limit.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Quota limit in bytes.</returns>
        public long GetDirectoryQuota(string path)
        {
            IFsrmQuota dirQutoa = this.GetDirectoryFsrmQuota(path);
            dirQutoa.RefreshUsageProperties();
            return (long)(decimal)dirQutoa.QuotaLimit;
        }

        /// <summary>
        /// Gets the disk usage.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The current size of the directory.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StatusRebuilding", Justification = "Spelled corectly.")]
        public long GetDirectorySize(string path)
        {
            path = CanonicalPath(path);
            IFsrmQuota dirQutoa = this.GetDirectoryFsrmQuota(path);

            dirQutoa.RefreshUsageProperties();

            int retires = 400;

            // FsrmQuotaFlags_StatusRebuilding   = 0x00020000
            while ((dirQutoa.QuotaFlags & 0x00020000) != 0)
            {
                if (--retires < 0)
                {
                    throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, "Quota for dir '{0}' is still in StatusRebuilding state.", path));
                }

                dirQutoa.RefreshUsageProperties();
                Thread.Sleep(10);
            }

            // FsrmQuotaFlags_StatusIncomplete  = 0x00010000
            if ((dirQutoa.QuotaFlags & 0x00010000) != 0)
            {
                this.quotaManager.Scan(path);
                dirQutoa.RefreshUsageProperties();
            }

            dirQutoa.RefreshUsageProperties();
            return (long)(decimal)dirQutoa.QuotaUsed;
        }

        /// <summary>
        /// Removes the directory quota.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>True if quota was found and deleted. False if quota was not found.</returns>
        public bool RemoveDirectoryQuota(string path)
        {
            path = CanonicalPath(path);
            bool ret = false;

            IFsrmQuota dirQuota = null;
            lock (this.quotasCacheLock)
            {
                this.quotasCache.TryGetValue(path, out dirQuota);
            }

            if (dirQuota == null)
            {
                try
                {
                    dirQuota = this.quotaManager.GetQuota(path);
                }
                catch (COMException)
                {
                }
            }

            if (dirQuota != null)
            {
                dirQuota.Delete();
                dirQuota.Commit();
                ret = true;
            }

            lock (this.quotasCacheLock)
            {
                this.quotasCache.Remove(path);
            }

            return ret;
        }

        /// <summary>
        /// Canonicalizes a path string.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Canonicalized path.</returns>
        private static string CanonicalPath(string path)
        {
            return Path.GetFullPath(path).ToUpper(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the directory quota.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The fsrm quota instance.</returns>
        private IFsrmQuota GetDirectoryFsrmQuota(string path)
        {
            path = CanonicalPath(path);
            IFsrmQuota dirQuota = null;
            lock (this.quotasCacheLock)
            {
                this.quotasCache.TryGetValue(path, out dirQuota);
            }

            if (dirQuota == null)
            {
                try
                {
                    dirQuota = this.quotaManager.GetQuota(path);
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == -2147200255)
                    {
                        // Error code: 0x80045301
                        // The specified quota could not be found.
                        dirQuota = null;
                    }
                    else if (ex.ErrorCode == -2147200252)
                    {
                        // Error code: 0x80045301
                        // The quota for the specified path could not be found.
                        throw new DirectoryNotFoundException(path);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (dirQuota == null)
                {
                    dirQuota = this.quotaManager.CreateQuota(path);
                    dirQuota.QuotaLimit = (long)1024 * 1024 * 1024 * 10; // 10 GiB

                    // FsrmQuotaFlags_Enforce            = 0x00000100,
                    // FsrmQuotaFlags_Disable            = 0x00000200,
                    // FsrmQuotaFlags_StatusIncomplete   = 0x00010000,
                    // FsrmQuotaFlags_StatusRebuilding   = 0x00020000 
                    dirQuota.QuotaFlags = 0x00000000; // Soft quota enforcement
                    dirQuota.Commit();
                }

                lock (this.quotasCacheLock)
                {
                    this.quotasCache[path] = dirQuota;
                }
            }

            return dirQuota;
        }
    }
}
