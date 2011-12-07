using System;
using System.Collections.Generic;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class HeartbeatMessage : JsonConvertibleObject
    {

        public class InstanceHeartbeat : JsonConvertibleObject
        {
            [JsonName("droplet")]
            public int DropletId
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

            [JsonName("instance")]
            public string InstanceId
            {
                get;
                set;
            }

            [JsonName("index")]
            public int InstanceIndex
            {
                get;
                set;
            }

            [JsonName("state")]
            /*
            public string StateInterchangeableFormat
            {
                get { return State.ToString(); }
                set { State = (DropletInstanceState)Enum.Parse(typeof(DropletInstanceState), value); }
            }
            */
            public DropletInstanceState State
            {
                get;
                set;
            }

            [JsonName("state_timestamp")]
            public int StateTimestampInterchangeableFormat
            {
                get { return RubyCompatibility.DateTimeToEpochSeconds(StateTimestamp); }
                set { StateTimestamp = RubyCompatibility.DateTimeFromEpochSeconds(value); }
            }

            public DateTime StateTimestamp
            {
                get;
                set;
            }
        }

        //todo: stefi: change the type when json helper class can go deep into generic collections
        [JsonName("droplets")]
        public List<Dictionary<string, object>> Droplets = new List<Dictionary<string,object>>();

    }
}
