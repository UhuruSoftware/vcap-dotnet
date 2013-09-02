// -----------------------------------------------------------------------
// <copyright file="Buildpack.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2013 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Uhuru.Utilities;
    using YamlDotNet.RepresentationModel;
    using YamlDotNet.RepresentationModel.Serialization;

    class Buildpack
    {
        public string Name
        {
            get 
            {
                return detectOutput.Trim();
            }
        }

        private string detectOutput;
        private string path;
        private string appDir;
        private string cacheDir;
        private string logFile;

        public Buildpack(string path, string appDir, string cacheDir, string logFile)
        {
            this.path = path;
            this.appDir = appDir;
            this.cacheDir = cacheDir;
            this.logFile = logFile;
        }

        public bool Detect()
        {
            string script = GetExecutable(Path.Combine(path, "bin"), "detect");

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = script;
            start.Arguments = this.appDir;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            using (Process process = Process.Start(start))
            {
                process.WaitForExit(3000);
                detectOutput = process.StandardOutput.ReadToEnd();
                if (process.ExitCode == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Compile() 
        {
            string script = GetExecutable(Path.Combine(path, "bin"), "compile");
            string args = string.Format("{0} {1} >> {2}", this.appDir, this.cacheDir, this.logFile);
            string output = Command.RunCommandAndGetOutput(script, args);
        }

        public ReleaseInfo GetReleaseInfo() 
        {
            string script = GetExecutable(Path.Combine(path, "bin"), "release");
            string args = string.Format("{0}", this.appDir);
            string output = Command.RunCommandAndGetOutput(script, args);
            using (var reader = new StringReader(output))
            {
                Deserializer deserializer = new Deserializer();
                return (ReleaseInfo)deserializer.Deserialize(reader, typeof(ReleaseInfo));
            }
        }

        private string GetExecutable(string path, string file)
        {
            string[] pathExt = Environment.GetEnvironmentVariable("PATHEXT").Split(';');
            foreach (string ext in pathExt)
            {
                if(File.Exists(Path.Combine(path, file+ext)))
                {
                    return Path.Combine(path, file+ext);
                }
            }
            throw new Exception("No executable found");
        }
    }

    class ReleaseInfo
    {
        [YamlAlias("default_process_type")]
        public DefaultProcessType defaultProcessType { get; set; }
    }

    class DefaultProcessType
    {
        [YamlAlias("web")]
        public string Web { get; set; }
    }
}
