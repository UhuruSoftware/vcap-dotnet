using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    class HelloMessage : JsonConvertibleObject
    {
        [JsonName("id")]
        public string Id;

        [JsonName("version")]
        public decimal Version;

        [JsonName("ip")]
        public string Host;

        [JsonName("port")]
        public int FileViewerPort;

    }
}
