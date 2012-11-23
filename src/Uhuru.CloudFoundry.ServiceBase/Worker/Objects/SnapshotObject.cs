// -----------------------------------------------------------------------
// <copyright file="Snapshot.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase.Objects
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SnapshotObject
    {
        [JsonProperty("snapshot_id")]
        public long SnapshotId { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("manifest")]
        public Manifest Manifest { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonIgnore]
        public string[] Files { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Manifest
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("service_version")]
        public string ServiceVersion { get; set; }
    }
}
