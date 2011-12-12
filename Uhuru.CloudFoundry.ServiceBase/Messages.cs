// -----------------------------------------------------------------------
// <copyright file="Messages.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;
    
    abstract class MessageWithSuccessStatus : JsonConvertibleObject
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

    class ProvisionRequest : JsonConvertibleObject
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("plan")]
        public ProvisionedServicePlanType Plan
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }

    // Node --> Provisioner
    class ProvisionResponse : MessageWithSuccessStatus
    {
        [JsonName("success")]
        public override bool Success
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("credentials")]
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

    class UnprovisionRequest : JsonConvertibleObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("bindings")]
        public ServiceCredentials[] Bindings
        {
            get;
            set;
        }
    }

    class BindRequest : JsonConvertibleObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("bind_opts")]
        public Dictionary<string, object> BindOptions
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }

    class BindResponse : MessageWithSuccessStatus
    {
        [JsonName("success")]
        public override bool Success
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("credentials")]
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

    class UnbindRequest : JsonConvertibleObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }

    class SimpleResponse : MessageWithSuccessStatus
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

    class RestoreRequest : JsonConvertibleObject
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

    class CheckOrphanRequest : JsonConvertibleObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("handles")]
        public Handle[] Handles
        {
            get;
            set;
        }
    }

    class CheckOrphanResponse : MessageWithSuccessStatus
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
        // A hash for orphan instances
        // Key: the id of the node with orphans
        // Value: orphan instances list
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("orphan_instances")]
        public Dictionary<string, object> OrphanInstances
        {
            get;
            set;
        }
        // A hash for orphan bindings
        // Key: the id of the node with orphans
        // Value: orphan bindings list
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("orphan_bindings")]
        public Dictionary<string, object> OrphanBindings
        {
            get;
            set;
        }
    }

    class PurgeOrphanRequest : JsonConvertibleObject
    {
        // A list of orphan instances names
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("orphan_ins_list")]
        public string[] OrphanInsList
        {
            get;
            set;
        }

        // A list of orphan bindings credentials
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), JsonName("orphan_binding_list")]
        public ServiceCredentials[] OrphanBindingList
        {
            get;
            set;
        }
    }
}