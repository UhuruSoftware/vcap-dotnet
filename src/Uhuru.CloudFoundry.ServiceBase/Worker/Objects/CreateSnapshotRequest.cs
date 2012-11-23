// -----------------------------------------------------------------------
// <copyright file="CreateSnapshotRequest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase.Worker.Objects
{
    using Newtonsoft.Json;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CreateSnapshotRequest
    {
        [JsonProperty("service_id")]
        public string ServiceId { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("service_version")]
        public string ServiceVersion { get; set; }
    }
}
