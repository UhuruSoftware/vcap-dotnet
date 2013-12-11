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
    using Uhuru.Isolation;
    using Uhuru.Utilities;
    using YamlDotNet.RepresentationModel;
    using YamlDotNet.RepresentationModel.Serialization;

    public class Buildpack
    {
        public string Name
        {
            get 
            {
                return detectOutput.Trim();
            }
        }

        public string PhysicalPath
        {
            get
            {
                return this.path;
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

        public bool Detect(ProcessPrison prison)
        {
            string exe = GetExecutable(Path.Combine(this.path, "bin"), "detect");

            string outputPath = Path.Combine(this.cacheDir, "detect.yml");
            string script = string.Format("\"{0}\" {1} > {2} 2>&1", exe, this.appDir, outputPath);

            Logger.Debug("Running detect script: {0}", script);
            var runInfo = new ProcessPrisonRunInfo();
            runInfo.WorkingDirectory = Path.Combine(this.appDir);
            runInfo.FileName = null;
            runInfo.Arguments = script;
            Process process = prison.RunProcess(runInfo);
            
            process.WaitForExit(5000);
            if (!process.HasExited)
            {
                process.Kill();                
            }

            if (File.Exists(outputPath))
            {
                this.detectOutput = File.ReadAllText(outputPath);
                Logger.Debug("Detect output: {0}", this.detectOutput);
                File.Delete(outputPath);
            }
            if (process.ExitCode == 0)
            {
                return true;
            }
            else
            {
                Logger.Warning("Detect process exited with {0}", process.ExitCode);
                return false;
            }
        }

        public Process StartCompile(ProcessPrison prison) 
        {
            string exe = GetExecutable(Path.Combine(path, "bin"), "compile");
            string args = string.Format("{0} {1} >> {2} 2>&1", this.appDir, this.cacheDir, this.logFile);
            Logger.Debug("Running compile script {0} {1}", exe, args);
           
            var runInfo = new ProcessPrisonRunInfo();
            runInfo.WorkingDirectory = Path.Combine(this.appDir);
            runInfo.FileName = null;
            runInfo.Arguments = string.Format("{0} {1}", exe, args);

            return prison.RunProcess(runInfo);
        }

        public ReleaseInfo GetReleaseInfo(ProcessPrison prison) 
        {
            string exe = GetExecutable(Path.Combine(this.path, "bin"), "release");

            string outputPath = Path.Combine(this.cacheDir, "release.yml");
            string script = string.Format("{0} {1} > {2} 2>&1", exe, this.appDir, outputPath);

            var runInfo = new ProcessPrisonRunInfo();
            runInfo.WorkingDirectory = Path.Combine(this.appDir);
            runInfo.FileName = null;
            runInfo.Arguments = script;
            Process process = prison.RunProcess(runInfo);
            process.WaitForExit(5000);

            string output = File.ReadAllText(outputPath);
            File.Delete(outputPath);
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

    public class ReleaseInfo
    {
        [YamlAlias("default_process_type")]
        public DefaultProcessType defaultProcessType { get; set; }
    }

    public class DefaultProcessType
    {
        [YamlAlias("web")]
        public string Web { get; set; }
    }
}
