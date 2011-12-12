using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    class HelloMessage : JsonConvertibleObject
    {
        [JsonName("id")]
        public string Id
        {
            get;
            set;
        }

        [JsonName("version")]
        public decimal Version
        {
            get;
            set;
        }

        [JsonName("ip")]
        public string Host
        {
            get;
            set;
        }

        [JsonName("port")]
        public int FileViewerPort
        {
            get;
            set;
        }
    }
}
