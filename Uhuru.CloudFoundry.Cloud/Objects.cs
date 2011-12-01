using System;
using System.Collections.Generic;
namespace CloudFoundry.Net
{
    public class App
    {
        private string metaCreated = String.Empty;
        private string metaVersion = String.Empty;
        private string metaDebug = String.Empty;
        private string version = String.Empty;
        private string state = String.Empty;
        private string resourcesFDS = String.Empty;
        private string resourcesDisk = String.Empty;
        private string resourcesMemory = String.Empty;
        private string stagingStack = String.Empty;
        private string stagingModel = String.Empty;
        private string uris = String.Empty;
        private string name = String.Empty;
        private string instances = String.Empty;
        private string runningInstances = String.Empty;
        private string services = String.Empty;

        public string Name
        {
            get { return name == null ? String.Empty : name; }
            set { name = value; }
        }

        public string Instances
        {
            get 
            {
                int val = 0;
                int.TryParse(instances, out val);
                return val.ToString();
            }
            set { instances = value; }
        }

        public string RunningInstances
        {
            get
            {
                int val = 0;
                int.TryParse(runningInstances, out val);
                return val.ToString();
            }
            set { runningInstances = value; }
        }

        public string Services
        {
            get { return services == null ? String.Empty : services; }
            set { services = value; }
        }

        public string[] ServiceNames
        {
            get 
            {
                return services.Split(new string[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public string Uris
        {
            get { return uris == null ? String.Empty : uris; }
            set { uris = value; }
        }

        public string[] UriList
        {
            get
            {
                return uris.Split(new string[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public string StagingModel
        {
            get { return stagingModel == null ? String.Empty : stagingModel; }
            set { stagingModel = value; }
        }

        public string StagingStack
        {
            get { return stagingStack == null ? String.Empty : stagingStack; }
            set { stagingStack = value; }
        }

        public string ResourcesMemory
        {
            get { return resourcesMemory == null ? String.Empty : resourcesMemory; }
            set { resourcesMemory = value; }
        }

        public string ResourcesDisk
        {
            get { return resourcesDisk == null ? String.Empty : resourcesDisk; }
            set { resourcesDisk = value; }
        }

        public string ResourcesFDS
        {
            get { return resourcesFDS == null ? String.Empty : resourcesFDS; }
            set { resourcesFDS = value; }
        }

        public string State
        {
            get { return state == null ? String.Empty : state; }
            set { state = value; }
        }

        public string Version
        {
            get { return version == null ? String.Empty : version; }
            set { version = value; }
        }

        public string MetaDebug
        {
            get { return metaDebug == null ? String.Empty : metaDebug; }
            set { metaDebug = value; }
        }

        public string MetaVersion
        {
            get { return metaVersion == null ? String.Empty : metaVersion; }
            set { metaVersion = value; }
        }

        public string MetaCreated
        {
            get { return metaCreated == null ? String.Empty : metaCreated; }
            set { metaCreated = value; }
        }

        public bool HasSameMainValuesAs(App that)
        {
            if (that == null) return false;
            
            return (this.Name == that.Name && this.RunningInstances == that.RunningInstances &&
                this.Services == that.Services && this.StagingModel == that.StagingModel &&
                this.StagingStack == that.StagingStack && this.Uris == that.Uris);
        }
    }

    public class User
    {
        public string Email = String.Empty;
        public string IsAdmin = String.Empty;
        public string Apps = String.Empty;
    }

    public class Service
    {
        public string Vendor = String.Empty;
        public string Version = String.Empty;
        public string Description = String.Empty;
        public string Type = String.Empty;
    }

    public class ProvisionedService
    {
        public string Name = String.Empty;
        public string Type = String.Empty;
        public string Vendor = String.Empty;
        public string Tier = String.Empty;
        public string Version = String.Empty;
        public string MetaCreated = String.Empty;
        public string MetaTags = String.Empty;
        public string MetaUpdated = String.Empty;
        public string MetaVersion = String.Empty;
    }

    public class Runtime
    {
        public string Name = String.Empty;
        public string Description = String.Empty;
        public string Version = String.Empty;
    }

    public class Framework
    {
        public string Name = String.Empty;
        public List<Runtime> Runtimes = new List<Runtime>();
        public List<string> AppServers = new List<string>();
    }
}