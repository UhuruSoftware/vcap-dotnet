// -----------------------------------------------------------------------
// <copyright file="RouterMessage.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class encapsulates a message that is sent to the router to register an instance.
    /// </summary>
    public class RouterMessage : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the DEA id.
        /// </summary>
        [JsonName("dea")]
        public string DeaId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the app/droplet id.
        /// </summary>
        [JsonName("app")]
        public string DropletId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the host of the DEA service.
        /// </summary>
        [JsonName("host")]
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port on which the droplet instance listens.
        /// </summary>
        [JsonName("port")]
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the URLs of the running droplet.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It is used for JSON (de)serialization."), 
        JsonName("uris")]
        public string[] Uris
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the framework and runtime tags.
        /// </summary>
        [JsonName("tags")]
        public TagsObject Tags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the private instance id, used as a sticky session of the instance.
        /// </summary>
        [JsonName("private_instance_id")]
        public string PrivateInstanceId
        {
            get;
            set;
        }

        /// <summary>
        /// This class contains tags for the runtime and framework.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Code is cleaner this way.")]
        public class TagsObject : JsonConvertibleObject
        {
            /// <summary>
            /// Gets or sets the framework.
            /// </summary>
            [JsonName("framework")]
            public string Framework
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the runtime.
            /// </summary>
            [JsonName("runtime")]
            public string Runtime
            {
                get;
                set;
            }
        }
    }
}