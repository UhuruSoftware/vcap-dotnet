// -----------------------------------------------------------------------
// <copyright file="Messages.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;
    
    internal abstract class MessageWithSuccessStatus : JsonConvertibleObject
    {
        public abstract bool Success
        {
            get;
            set;
        }

        public abstract Dictionary<string, object> Error
        {
            get;
            set;
        }
    }

    internal class ProvisionRequest : JsonConvertibleObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"), 
        JsonName("plan")]
        public ProvisionedServicePlanType Plan
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }

    // Node --> Provisioner
    internal class ProvisionResponse : MessageWithSuccessStatus
    {
        [JsonName("success")]
        public override bool Success
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"), 
        JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }

        [JsonName("error")]
        public override Dictionary<string, object> Error
        {
            get;
            set;
        }
    }

    internal class UnprovisionRequest : JsonConvertibleObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("bindings")]
        public ServiceCredentials[] Bindings
        {
            get;
            set;
        }
    }

    internal class BindRequest : JsonConvertibleObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("bind_opts")]
        public Dictionary<string, object> BindOptions
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"), 
        JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }

    internal class BindResponse : MessageWithSuccessStatus
    {
        [JsonName("success")]
        public override bool Success
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }

        [JsonName("error")]
        public override Dictionary<string, object> Error
        {
            get;
            set;
        }
    }

    internal class UnbindRequest : JsonConvertibleObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }

    internal class SimpleResponse : MessageWithSuccessStatus
    {
        [JsonName("success")]
        public override bool Success
        {
            get;
            set;
        }

        [JsonName("error")]
        public override Dictionary<string, object> Error
        {
            get;
            set;
        }
    }

    internal class RestoreRequest : JsonConvertibleObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("instance_id")]
        public string InstanceId
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("backup_path")]
        public string BackupPath
        {
            get;
            set;
        }
    }

    internal class CheckOrphanRequest : JsonConvertibleObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("handles")]
        public Handle[] Handles
        {
            get;
            set;
        }
    }

    internal class CheckOrphanResponse : MessageWithSuccessStatus
    {
        [JsonName("success")]
        public override bool Success
        {
            get;
            set;
        }

        [JsonName("error")]
        public override Dictionary<string, object> Error
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a hash for orphan instances;
        /// Key: the id of the node with orphans
        /// Value: orphan instances list
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("orphan_instances")]
        public Dictionary<string, object> OrphanInstances
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a hash for orphan bindings;
        /// Key: the id of the node with orphans
        /// Value: orphan bindings list
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("orphan_bindings")]
        public Dictionary<string, object> OrphanBindings
        {
            get;
            set;
        }
    }

    internal class PurgeOrphanRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets a list of orphan instances names
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("orphan_ins_list")]
        public string[] OrphanInsList
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of orphan bindings credentials
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("orphan_binding_list")]
        public ServiceCredentials[] OrphanBindingList
        {
            get;
            set;
        }
    }
}