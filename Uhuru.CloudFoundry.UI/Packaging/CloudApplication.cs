using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;

namespace Uhuru.CloudFoundry.UI.Packaging
{
    public partial class CloudApplication
    {
        string packageFile = String.Empty;

        public string PackageFile
        {
            get { return packageFile; }
            set { packageFile = value; }
        }

        public CloudApplication()
        {
            this.Services = new CloudApplicationService[0];
            this.Variables = new EnvironmentVariable[0];
            this.Urls = new string[0];
        }

        public void Save(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CloudApplication));
            using (FileStream file = File.Open(filename, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(file, this);
            }
            packageFile = filename;
        }

        public bool Load(string filename)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CloudApplication));
                using (FileStream file = File.OpenRead(filename))
                {
                    CloudApplication loadedApp = (CloudApplication)serializer.Deserialize(file);
                    this.Framework = loadedApp.Framework;
                    this.InstanceCount = loadedApp.InstanceCount;
                    this.Memory = loadedApp.Memory;
                    this.Name = loadedApp.Name;
                    this.Runtime = loadedApp.Runtime;
                    this.Services = loadedApp.Services != null ? loadedApp.Services : new CloudApplicationService[0];
                    this.Urls = loadedApp.Urls != null ? loadedApp.Urls : new string[0];
                    this.Variables = loadedApp.Variables != null ? loadedApp.Variables : new EnvironmentVariable[0];
                    this.Deployable = loadedApp.Deployable;
                }
                packageFile = filename;
                return true;
            }
            catch (SerializationException)
            {
                return false;
            }

        }
    }
}