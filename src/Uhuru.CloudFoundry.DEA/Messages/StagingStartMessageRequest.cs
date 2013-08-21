// -----------------------------------------------------------------------
// <copyright file="DeaStartMessageRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2013 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using System.Collections.Generic;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;
   
    class StagingStartMessageRequest : JsonConvertibleObject
    {
        public StagingStartMessageRequest()
        {
            this.StartMessage = new DeaStartMessageRequest();
            this.Properties = new StagingStartRequestProperties();
        }

        [JsonName("app_id")]
        public string AppID { get; set; }

        [JsonName("task_id")]
        public string TaskID { get; set; }

        [JsonName("download_uri")]
        public string DownloadURI { get; set; }

        [JsonName("upload_uri")]
        public string UploadURI { get; set; }

        [JsonName("buildpack_cache_download_uri")]
        public string BuildpackCacheDownloadURI { get; set; }

        [JsonName("buildpack_cache_upload_uri")]
        public string BuildpackCacheUploadURI { get; set; }

        [JsonName("properties")]
        public StagingStartRequestProperties Properties { get; set; }

        [JsonName("start_message")]
        public DeaStartMessageRequest StartMessage { get; set; }
    }
}
