// -----------------------------------------------------------------------
// <copyright file="StorageQuota.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService
{
    using System;
    using System.Globalization;

    /// <summary>
    /// This class contains the implementation for a MS Sql Server Cloud Foundry system service node.
    /// </summary>
    public partial class Node
    {
        /// <summary>
        /// Gets the size of a DB.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns>The size of the DB in megabytes.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "db", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Method is not yet implemented")]
        private int GetDBSize(string db)
        {
            // todo: vladi: implement this.
            return 1;
        }

        /// <summary>
        /// Kills the user sessions.
        /// </summary>
        /// <param name="targetUser">The target_user.</param>
        /// <param name="targetDB">The target database.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Method is not yet implemented")]
        private void KillUserSessions(string targetUser, string targetDB)
        {
            // todo: vladi: implement this.
        }

        /// <summary>
        /// Disables access to a database.
        /// </summary>
        /// <param name="db">The database for which to disable access.</param>
        /// <returns>True if the operation was successful.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "db", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Method is not yet implemented")]
        private bool DisableAccess(string db)
        {
            // todo: vladi: implement this.
            return false;
        }

        /// <summary>
        /// Grants write access to a database.
        /// </summary>
        /// <param name="db">The database for which to grant access.</param>
        /// <param name="service">The service.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "db", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "service", Justification = "Method is not yet implemented")]
        private void GrantWriteAccess(string db, object service)
        {
            // todo: vladi: implement this.
        }

        /// <summary>
        /// Revokes write access for a database.
        /// </summary>
        /// <param name="db">The database for which to revoke access.</param>
        /// <param name="service">The service.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "service", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "db", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Method is not yet implemented")]
        private void RevokeWriteAccess(string db, object service)
        {
            // todo: vladi: implement this.
        }

        /// <summary>
        /// Formats a DB listing.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="db">The database.</param>
        /// <param name="size">The size.</param>
        /// <returns>A line of text containing database information.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Method is not yet implemented")]
        private string FormatDBListing(string user, string db, int size)
        {
            return string.Format(CultureInfo.InvariantCulture, Strings.StorageQuotaDbListingFormatString, user, db, size);
        }

        /// <summary>
        /// Enforces storage quota for databases.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented")]
        private void EnforceStorageQuota()
        {
            // todo: vladi: implement this.
        }

        /// <summary>
        /// Kills all database sessions.
        /// </summary>
        /// <param name="database">The database whose sessions to kill.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "database", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Method is not yet implemented")]
        private void KillDatabaseSession(string database)
        {
            // todo: vladi: implement this
        }
    }
}
