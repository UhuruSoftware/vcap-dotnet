using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;
using System.Globalization;

namespace Uhuru.CloudFoundry.ServiceBase
{
    abstract class MessageWithSuccessStatus : JsonConvertibleObject
    {
        abstract bool Success
        {
            get;
            set;
        }

        abstract Dictionary<string, object> Error
        {
            get;
            set;
        }
    }

    class ProvisionRequest : JsonConvertibleObject
    {

        [JsonName("plan")]
        public ProvisionedServicePlanType Plan
        {
            get;
            set;
        }

        [JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }

    // Node --> Provisioner
    class ProvisionResponse : JsonConvertibleObject, MessageWithSuccessStatus
    {
        [JsonName("success")]
        public bool Success
        {
            get;
            set;
        }

        [JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }

        [JsonName("error")]
        public Dictionary<string, object> Error
        {
            get;
            set;
        }
    }


    class UnprovisionRequest : JsonConvertibleObject
    {
        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        [JsonName("bindings")]
        public ServiceCredentials[] Bindings
        {
            get;
            set;
        }
    }

    class BindRequest : JsonConvertibleObject
    {
        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        [JsonName("bind_opts")]
        public Dictionary<string, object> BindOptions
        {
            get;
            set;
        }

        [JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }

    class BindResponse : JsonConvertibleObject, MessageWithSuccessStatus
    {
        [JsonName("success")]
        public bool Success
        {
            get;
            set;
        }

        [JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }

        [JsonName("error")]
        public Dictionary<string, object> Error
        {
            get;
            set;
        }
    }

    class UnbindRequest : JsonConvertibleObject
    {
        [JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }

    class SimpleResponse : JsonConvertibleObject, MessageWithSuccessStatus
    {
        [JsonName("success")]
        public bool Success
        {
            get;
            set;
        }

        [JsonName("error")]
        public Dictionary<string, object> Error
        {
            get;
            set;
        }
    }

    class RestoreRequest : JsonConvertibleObject
    {
        [JsonName("instance_id")]
        public string InstanceId
        {
            get;
            set;
        }

        [JsonName("backup_path")]
        public string BackupPath
        {
            get;
            set;
        }
    }

    class CheckOrphanRequest : JsonConvertibleObject
    {
        [JsonName("handles")]
        public Handle[] Handles
        {
            get;
            set;
        }
    }

    class CheckOrphanResponse : JsonConvertibleObject, MessageWithSuccessStatus
    {

        [JsonName("success")]
        public bool Success
        {
            get;
            set;
        }

        [JsonName("error")]
        public Dictionary<string, object> Error
        {
            get;
            set;
        }
        // A hash for orphan instances
        // Key: the id of the node with orphans
        // Value: orphan instances list
        [JsonName("orphan_instances")]
        public Dictionary<string, object> OrphanInstances
        {
            get;
            set;
        }
        // A hash for orphan bindings
        // Key: the id of the node with orphans
        // Value: orphan bindings list
        [JsonName("orphan_bindings")]
        public Dictionary<string, object> OrphanBindings
        {
            get;
            set;
        }
    }

    class PurgeOrphanRequest : JsonConvertibleObject
    {
        // A list of orphan instances names
        [JsonName("orphan_ins_list")]
        public string[] OrphanInsList
        {
            get;
            set;
        }

        // A list of orphan bindings credentials
        [JsonName("orphan_binding_list")]
        public ServiceCredentials[] OrphanBindingList
        {
            get;
            set;
        }
    }
}