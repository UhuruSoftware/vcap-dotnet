using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;
using Uhuru.Utilities.Json;

namespace Uhuru.CloudFoundry.DEA
{
    public class StagingInstanceProperties : JsonConvertibleObject
    {

        [JsonName("app_id")]
        public string AppId { get; set; }

        [JsonName("task_id")]
        public string TaskId { get; set; }

        /// <summary>
        /// Gets or sets the File Descriptors Quota.
        /// </summary>
        [JsonName("fds_quota")]
        public long FDSQuota { get; set; }

        /// <summary>
        /// Gets or sets the disk memory quota.
        /// </summary>
        [JsonName("disk_quota")]
        public long DiskQuotaBytes { get; set; }

        /// <summary>
        /// Gets or sets the RAM quota.
        /// </summary>
        [JsonName("mem_quota")]
        public long MemoryQuotaBytes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [resources tracked]. Flag if the instance resources have been accounted to avoid tracking or untracking them several times.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [resources tracked]; otherwise, <c>false</c>.
        /// </value>
        [JsonName("resources_tracked")]
        public bool ResourcesTracked { get; set; }

        /// <summary>
        /// Gets or sets the windows username used for the instance.
        /// </summary>
        [JsonName("windows_username")]
        public string WindowsUserName { get; set; }

        /// <summary>
        /// Gets or sets the windows password associated with the windows user.
        /// </summary>
        [JsonName("windows_password")]
        public string WindowsPassword { get; set; }

        /// <summary>
        /// Gets or sets the directory the instance is stored.
        /// </summary>
        [JsonName("dir")]
        public string Directory { get; set; }

        [JsonName("instance_id")]
        public string InstanceId { get; set; }



        /// <summary>
        /// Gets or sets a value indicating whether [stop processed]. Indicated if the StopStaging routine was completely invoked on this instance.
        /// </summary>
        public bool StopProcessed
        {
            get
            {
                return this.stopProcessed;
            }

            set
            {
                this.stopProcessed = value;
            }
        }

        public string Reply { get; set; }
        public string StreamingLogUrl { get; set; }
        public string TaskLog { get; set; }
        public string DetectedBuildpack { get; set; }
        public string BuildpackCacheDownloadURI { get; set; }
        public string BuildpackCacheUploadURI { get; set; }
        public string DownloadURI { get; set; }
        public string UploadURI { get; set; }
        public string MetaCommand { get; set; }
        public bool UseDiskQuota { get; set; }
        public long UploadThrottleBitsps { get; set; }        

        /// <summary>
        /// Gets or sets the instance start timestamp.
        /// </summary>
        public DateTime Start
        {
            get;
            set;
        }

        /// <summary>
        /// Indicated if the StopDroplet routine was completely invoked on this instance.
        /// </summary>
        private bool stopProcessed;
    }
}
