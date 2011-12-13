// -----------------------------------------------------------------------
// <copyright file="DeaStartMessageRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System.Collections.Generic;
    using Uhuru.Utilities;

    // Sample:
    /*
     * [#10005] Received on [dea.2134fafa645b42278f96cb619067a1e0.start] : '
     * {"droplet" :198,"name" :"helloworld","uris" :["helloworld.uhurucloud.net"],"runtime" :"iis","framework" :"net","sha1" :"98b1159c7d3539dd450fd86f92647d3902a0067b",
     * "executableFile":"/var/vcap/shared/droplets/98b1159c7d3539dd450fd86f92647d3902a0067b","executableUri" :"http://192.168.1.160:9022/staged_droplets/198/98b1159c7d3539dd450fd86f92647d3902a0067b",
     * "version" :"98b1159c7d3539dd450fd86f92647d3902a0067b-1","services" :[],
     * "limits" :{"mem":128,"disk":2048,"fds":256},"env" :[],"users" :["dev@cloudfoundry.org"],"index" :0}'
     */
    /// <summary>
    /// This class encapsulates a request message to start a droplet instance.
    /// </summary>
    public class DeaStartMessageRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the droplet id of the instance that has to be started.
        /// </summary>
        [JsonName("droplet")]
        public int DropletId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the instance that has to be started.
        /// </summary>
        [JsonName("index")]
        public int Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the droplet.
        /// </summary>
        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the URLs that are mapped to the droplet.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), 
        JsonName("uris")]
        public string[] Uris
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the runtime that the droplet needs to run.
        /// </summary>
        [JsonName("runtime")]
        public string Runtime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the framework used by the droplet.
        /// </summary>
        [JsonName("framework")]
        public string Framework
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unique hash identifying the droplet.
        /// </summary>
        [JsonName("sha1")]
        public string Sha1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the executable file.
        /// </summary>
        [JsonName("executableFile")]
        public string ExecutableFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the executable URI.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), 
        JsonName("executableUri")]
        public string ExecutableUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the droplet.
        /// </summary>
        [JsonName("version")]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the services that are bound to this droplet.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), 
        JsonName("services")]
        public Dictionary<string, object>[] Services
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the environment variables for the droplet.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), 
        JsonName("env")]
        public string[] Environment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the users that own the droplet.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), 
        JsonName("users")]
        public string[] Users
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the usage limits of the app.
        /// </summary>
        [JsonName("limits")]
        public StartRequestDropletLimits Limits
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeaStartMessageRequest"/> class.
        /// </summary>
        public DeaStartMessageRequest()
        {
            this.Limits = new StartRequestDropletLimits();
        }
    }

    /// <summary>
    /// This class contains information about the usage limits of a droplet.
    /// </summary>
    public class StartRequestDropletLimits : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the maximum memory limit in megabytes.
        /// </summary>
        [JsonName("mem")]
        public long? MemoryMbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum disk usage in megabytes.
        /// </summary>
        [JsonName("disk")]
        public long? DiskMbytes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the mximum number of open files and sockets.
        /// </summary>
        [JsonName("fds")]
        public long? FileDescriptors
        {
            get;
            set;
        }
    }
}
