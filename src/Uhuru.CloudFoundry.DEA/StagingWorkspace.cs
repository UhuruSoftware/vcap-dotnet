// -----------------------------------------------------------------------
// <copyright file="StagingWorkspace.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2013 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    class StagingWorkspace
    {
        private const string DropletFile = "droplet.tgz";
        private const string BuildpackCacheFile = "buildpack_cache.tgz";
        private const string StagingLog = "staging_task.log";
        private const string StagingInfo = "staging_info";

        public string WorkspaceDir
        {
            get
            {
                string stagingDir = Path.Combine(this.baseDir, "staging");
                if (!Directory.Exists(stagingDir))
                    Directory.CreateDirectory(stagingDir);
                return stagingDir;
            }
        }

        public string TempDir
        {
            get
            {
                string tempDir = Path.Combine(this.WorkspaceDir, "tmp");
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);
                return tempDir;
            }
        }

        public string DownloadDropletPath { get { return Path.Combine(this.WorkspaceDir, "app.zip"); } }
        public string DownloadBuildpackCachePath { get { return Path.Combine(this.WorkspaceDir, BuildpackCacheFile); } }
        public string UnstagedDir { get { return Path.Combine(this.TempDir, "unstaged"); } }
        public string StagedDir { get { return Path.Combine(this.TempDir, "staged"); } }
        public string Cache { get { return Path.Combine(this.TempDir, "cache"); } }
        public string StagingLogPath { get { return Path.Combine(this.TempDir, "logs", StagingLog); } }
        public string StagedDroplet { get { return Path.Combine(this.TempDir, DropletFile); } }
        public string StagedDropletDir { get { return Path.Combine(this.WorkspaceDir, "staged"); } }

        private string baseDir;

        public StagingWorkspace(string baseDir)
        {
            this.baseDir = baseDir;
        }


    }
}
