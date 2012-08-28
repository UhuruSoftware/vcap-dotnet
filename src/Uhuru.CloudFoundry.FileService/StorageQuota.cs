// -----------------------------------------------------------------------
// <copyright file="StorageQuota.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService
{
    using System;
    using System.Globalization;

    /// <summary>
    /// This class contains the implementation for a Uhurufs Cloud Foundry system service node.
    /// </summary>
    public partial class FileServiceNode
    {
        /// <summary>
        /// Enforces storage quota for databases.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented")]
        private void EnforceStorageQuota()
        {
            // storage quota is enforced by VHD disk or FSRM
        }
    }
}
