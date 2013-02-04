// -----------------------------------------------------------------------
// <copyright file="RuntimeInfo.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using System.Collections.Generic;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// Runtime information message.
    /// </summary>
    public class RuntimeInfo : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the runtime description.
        /// </summary>
        [JsonName("description")]
        public string CloudControllerPartition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the runtime version.
        /// </summary>
        [JsonName("version")]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the runtime executable.
        /// </summary>
        [JsonName("executable")]
        public string Executable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the runtime staging.
        /// </summary>
        [JsonName("staging")]
        public string Staging
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the optional executable for version check.
        /// If this is null, use the runtime executable.
        /// </summary>
        [JsonName("version_executable")]
        public string VersionExecutable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the expected version output.
        /// </summary>
        [JsonName("version_output")]
        public string VersionOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the flag added to the executable to get the version.
        /// </summary>
        [JsonName("version_flag")]
        public string VersionParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets additional checks.
        /// </summary>
        [JsonName("additional_checks")]
        public string AdditionalChecks
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the runtime environment for the apps.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "For JSON serialization."), 
        JsonName("environment")]
        public Dictionary<string, string> Environment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the runtime status.
        /// </summary>
        [JsonName("status")]
        public Status Status
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the runtime series.
        /// </summary>
        [JsonName("series")]
        public string Series
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        [JsonName("category")]
        public string Category
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the runtime name.
        /// </summary>
        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Runtime info status information.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Cleaner.")]
    public class Status : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the status name.
        /// </summary>
        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }
    }
}
