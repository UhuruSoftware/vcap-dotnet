using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class HearbeatMessage : JsonConvertibleObject
    {

        public class InstanceHeartbeat : JsonConvertibleObject
        {
            [JsonName("droplet")]
            public int DropletId;

            [JsonName("version")]
            public string Version;

            [JsonName("instance")]
            public string InstanceId;

            [JsonName("index")]
            public int InstanceIndex;



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


        }


        [JsonName("droplets")]
        public List<Dictionary<string, object>> Droplets = new List<Dictionary<string,object>>(); //todo: stefi: change the type when json helper class can go deep into generic collections



    }
}
