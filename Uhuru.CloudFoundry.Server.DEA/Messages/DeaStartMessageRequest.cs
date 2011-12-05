using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public int DropletId;

        [JsonName("index")]
        public int Index;

        [JsonName("name")]
        public string Name;

        [JsonName("uris")]
        public List<string> Uris;

        [JsonName("runtime")]
        public string Runtime;

        [JsonName("framework")]
        public string Framework;

        [JsonName("sha1")]
        public string Sha1;

        [JsonName("executableFile")]
        public string ExecutableFile;

        [JsonName("executableUri")]
        public string ExecutableUri;

        [JsonName("version")]
        public string Version;

        [JsonName("services")]
        public List<Dictionary<string, object>> Services;

        [JsonName("env")]
        public List<string> Environment;

        [JsonName("users")]
        public List<string> Users;

        [JsonName("limits")]
        public DropletLimits Limits = new DropletLimits();

        public class DropletLimits : JsonConvertibleObject
        {

            [JsonName("mem")]
            public long? MemoryMbytes; 

            [JsonName("disk")]
            public long? DiskMbytes;

            [JsonName("fds")]
            public long? Fds;

        }


    }
}
