using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    public class DeaFindDropletMessageRequest : JsonConvertibleObject
    {
        [JsonName("droplet")]
        public int DropletId;

        [JsonName("version")]
        public string Version;

        [JsonName("instances")]
        public HashSet<string> InstanceIds;

        [JsonName("indices")]
        public HashSet<int> Indices;

        [JsonName("states")]
        public HashSet<string> StatesInterchangableFormat
        {
            get 
            { 
                HashSet<string> res = new HashSet<string>();
                foreach(DropletInstanceState state in States)
                {
                    res.Add(state.ToString());
                }
                return res;
            }
            set 
            {
                States = new HashSet<DropletInstanceState>();
                foreach (string state in value)
                {
                    States.Add((DropletInstanceState)Enum.Parse(typeof(DropletInstanceState), state));
                }
            }
        }
        public HashSet<DropletInstanceState> States;

        [JsonName("include_stats")]
        public bool IncludeStates;



    }
}
