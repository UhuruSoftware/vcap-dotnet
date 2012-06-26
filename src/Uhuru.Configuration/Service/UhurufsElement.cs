// -----------------------------------------------------------------------
// <copyright file="UhurufsElement.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.Service
{
    using System.Configuration;

    /// <summary>
    /// This configuration class defines settings for the Uhurufs Node component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Uhurufs", Justification = "Uhurufs is the correct spelling")]
    public class UhurufsElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the maximum allowed storage per instance. Size in MB.
        /// </summary>
        [ConfigurationProperty("maxStorageSize", IsRequired = false, DefaultValue = 100L)]
        public long MaxStorageSize
        {
            get
            {
                return (long)base["maxStorageSize"];
            }

            set
            {
                base["maxStorageSize"] = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
