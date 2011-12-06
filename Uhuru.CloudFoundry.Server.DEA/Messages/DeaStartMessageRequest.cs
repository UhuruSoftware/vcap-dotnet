using System.Collections.Generic;
using Uhuru.Utilities;


namespace Uhuru.CloudFoundry.DEA
{

    //Sample:
    /*
     * [#10005] Received on [dea.2134fafa645b42278f96cb619067a1e0.start] : '
     * {"droplet" :198,"name" :"helloworld","uris" :["helloworld.uhurucloud.net"],"runtime" :"iis","framework" :"net","sha1" :"98b1159c7d3539dd450fd86f92647d3902a0067b",
     * "executableFile":"/var/vcap/shared/droplets/98b1159c7d3539dd450fd86f92647d3902a0067b","executableUri" :"http://192.168.1.160:9022/staged_droplets/198/98b1159c7d3539dd450fd86f92647d3902a0067b",
     * "version" :"98b1159c7d3539dd450fd86f92647d3902a0067b-1","services" :[],
     * "limits" :{"mem":128,"disk":2048,"fds":256},"env" :[],"users" :["dev@cloudfoundry.org"],"index" :0}'
     */
    public class DeaStartMessageRequest : JsonConvertibleObject
    {

        [JsonName("droplet")]
        public int DropletId
        {
            get;
            set;
        }

        [JsonName("index")]
        public int Index
        {
            get;
            set;
        }

        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), 
        JsonName("uris")]
        public string[] Uris
        {
            get;
            set;
        }

        [JsonName("runtime")]
        public string Runtime
        {
            get;
            set;
        }

        [JsonName("framework")]
        public string Framework
        {
            get;
            set;
        }

        [JsonName("sha1")]
        public string Sha1
        {
            get;
            set;
        }

        [JsonName("executableFile")]
        public string ExecutableFile
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), 
        JsonName("executableUri")]
        public string ExecutableUri
        {
            get;
            set;
        }

        [JsonName("version")]
        public string Version
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), 
        JsonName("services")]
        public Dictionary<string, object>[] Services
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), 
        JsonName("env")]
        public string[] Environment
        {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), 
        JsonName("users")]
        public string[] Users
        {
            get;
            set;
        }

        [JsonName("limits")]
        public StartRequestDropletLimits Limits
        {
            get;
            set;
        }

        public DeaStartMessageRequest()
        {
            Limits = new StartRequestDropletLimits();
        }
    }

    public class StartRequestDropletLimits : JsonConvertibleObject
    {

        [JsonName("mem")]
        public long? MemoryMbytes
        {
            get;
            set;
        }

        [JsonName("disk")]
        public long? DiskMbytes
        {
            get;
            set;
        }

        [JsonName("fds")]
        public long? Fds
        {
            get;
            set;
        }
    }
}
