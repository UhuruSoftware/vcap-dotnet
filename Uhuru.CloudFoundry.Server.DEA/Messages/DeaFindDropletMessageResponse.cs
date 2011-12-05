using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DeaFindDropletMessageResponse : JsonConvertibleObject
    {
        [JsonName("dea")]
        public string DeaId;

        [JsonName("version")]
        public string Version;

        [JsonName("droplet")]
        public int DropletId;

        [JsonName("instance")]
        public string InstanceId;

        [JsonName("index")]
        public int Index;


        [JsonName("state")]
        public string StateInterchangableFormat
        {
            get { return State.ToString(); }
            set { State = (DropletInstanceState)Enum.Parse(typeof(DropletInstanceState), value); }
        }
        public DropletInstanceState State;


        [JsonName("state_timestamp")]
        public int StateTimestampInterchangelbeFormat
        {
            get { return Utils.DateTimeToEpochSeconds(StateTimestamp); }
            set { StateTimestamp = Utils.DateTimeFromEpochSeconds(value); }
        }
        public DateTime StateTimestamp;


        [JsonName("file_uri")]
        public string FileUri;

        [JsonName("credentials")]
        public string[] FileAuth;

        [JsonName("staged")]
        public string Staged;

        [JsonName("debug_ip")]
        public string DebugIp;

        [JsonName("debug_port")]
        public int? DebugPort;


        [JsonName("stats")]
        public DropletStatusMessageResponse Stats;


    }
}
