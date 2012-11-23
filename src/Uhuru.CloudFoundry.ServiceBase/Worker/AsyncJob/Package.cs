// -----------------------------------------------------------------------
// <copyright file="Package.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase.Worker.AsyncJob
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Uhuru.CloudFoundry.ServiceBase.Objects;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Package
    {
        private const string MANIFEST_FILE = "manifest";
        private const string CONTENT_FOLDER = "content";

        public Manifest Manifest { get; set; }
        private List<string> files;
        public string ZipFile { get; set; }

        public Package(string zipfile)
        {
            this.ZipFile = zipfile;
            files = new List<string>();
        }

        public void AddFile(string file)
        {
            if (File.Exists(file))
            {
                files.Add(file);
            }
        }

        public void Pack(string dumpPath)
        {
            string tempDir = Path.Combine(Config.TempFolder, Guid.NewGuid().ToString());
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            foreach (string file in files)
            {
                File.Copy(file, Path.Combine(tempDir, new FileInfo(file).Name));
            }

            string manifestFile = Path.Combine(tempDir, MANIFEST_FILE);
            File.WriteAllText(manifestFile, JsonConvert.SerializeObject(this.Manifest));
            ZipUtilities.ZipFile(tempDir, Path.Combine(dumpPath, this.ZipFile));

            Directory.Delete(tempDir, true);
        }
    }
}
