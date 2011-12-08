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


    /*
     * Example:
     * {"droplet":30,"name":"helloworld","uris":["helloworld.uhurucloud.net"],"runtime":"iis","framework":"net","sha1":"3bce1e6408234c4e2deb7eee8268f4b6123d2b5f","executableFile":"/var/vcap/shared/droplets/3bce1e6408234c4e2deb7eee8268f4b61
23d2b5f","executableUri":"http://192.168.1.117:9022/staged_droplets/30/3bce1e6408234c4e2deb7eee8268f4b6123d2b5f","version":"3bce1e6408234c4e2deb7eee8268f4b6123d2b5f-2","services":[{"name":"helloservice","type":"database","label":"mssql-2008","vendor":"mssql","version":"2008","tags":["mssql","2008","
relational"],"plan":"free","plan_option":null,"credentials":{"name":"D4TA4f587f703ee24294808c7aa6df78e4f2","hostname":"192.168.1.116","host":"192.168.1.116","port":1433,"user":"US3RkkyIUnYreXC8","username":"US3RkkyIUnYreXC8","password":"P4SSJ0jwJTg0ojGx"}}],"limits":{"mem":128,"disk":2048,"fds":256}
,"env":[],"users":["dev@cloudfoundry.org"],"debug":null,"index":0}'
     */

    /*
     * 
     *  "name":"helloservice",
        "type":"database",
        "label":"mssql-2008",
        "vendor":"mssql",
        "version":"2008",
        "tags":[
            "mssql",
            "2008",
            " relational"
        ],
        "plan":"free",
        "plan_option":null,
        "credentials":{
            "name":"D4TA4f587f703ee24294808c7aa6df78e4f2",
            "hostname":"192.168.1.116",
            "host":"192.168.1.116",
            "port":1433,
            "user":"US3RkkyIUnYreXC8",
            "username":"US3RkkyIUnYreXC8",
            "password":"P4SSJ0jwJTg0ojGx"
           }
     */
    public class StartRequestService : JsonConvertibleObject
    {
        public StartRequestService()
        {
            Credentials = new StartRequestServiceCredentials();
        }

        [JsonName("name")]
        public string ServiceName { get; set; }

        [JsonName("type")]
        public string ServiceType { get; set; }

        [JsonName("label")]
        public string Label { get; set; }

        [JsonName("vendor")]
        public string Vendor { get; set; }

        [JsonName("version")]
        public string Version { get; set; }

        [JsonName("tags")]
        public string[] Tags { get; set; }

        [JsonName("plan")]
        public string Plan { get; set; }

        [JsonName("plan_option")]
        public Dictionary<string, object> PlanOptions { get; set; }

        [JsonName("credentials")]
        public StartRequestServiceCredentials Credentials {get; set; }

    }


    public class StartRequestServiceCredentials : JsonConvertibleObject
    {
        [JsonName("name")]
        public string InstanceName { get; set; }

        [JsonName("hostname")]
        public string Hostname { get; set; }

        [JsonName("host")]
        public string Host { get; set; }

        [JsonName("port")]
        public int Port { get; set; }

        [JsonName("user")]
        public string User { get; set; }

        [JsonName("username")]
        public string Username { get; set; }

        [JsonName("password")]
        public string Password { get; set; }
    }

}
