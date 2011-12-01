using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Uhuru.CloudFoundry.UI.Packaging;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using CloudFoundry.Net;
using System.Globalization;
using EnvDTE;
using System.IO;
using Uhuru.CloudFoundry.UI.Packaging;
using System.Runtime.Serialization;

namespace Uhuru.CloudFoundry.UI.VS2010.Extensions
{

    [ComVisible(true)]
    public class CloudProjectProperties
    {
        private CloudApplication projectPackage = new CloudApplication();
        private string configFile = String.Empty;
        private string projectDirectory;
        private Project _vsProject;

        public CloudApplication PushablePackage
        {
            get
            {
                return projectPackage;
            }
        }

        public string ProjectDirectory
        {
            get
            {
                return projectDirectory;
            }
        }

        public void Initialize(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            _vsProject = project;

            //get project directory
            projectDirectory = Path.GetDirectoryName(project.FileName);

            if (Directory.Exists(projectDirectory))
            {
                configFile = Path.Combine(projectDirectory, IntegrationCenter.CloudConfigurationFile);
                if (!File.Exists(configFile) || !projectPackage.Load(configFile))
                {
                    string detectedFramework = Utils.GetFramework(projectDirectory);
                    projectPackage.Framework = detectedFramework.Length > 0 ? detectedFramework : "net";
                    projectPackage.Services = new CloudApplicationService[0];
                    projectPackage.Urls = new string[0];
                    projectPackage.Variables = new EnvironmentVariable[0];
                    projectPackage.Memory = 128;
                    projectPackage.InstanceCount = 1;
                    projectPackage.Runtime = "iis";
                    projectPackage.Deployable = false;
                    projectPackage.Name = project.Name;
                }
            }
        }

        private void SaveSettings()
        {
            if (!String.IsNullOrEmpty(configFile))
            {
                if (projectPackage.Deployable || File.Exists(configFile))
                {
                    projectPackage.Save(configFile);
                }
            }
        }

        [DisplayName("Deployable")]
        [Description("Set this to true if you want the project to be deployable.")]
        [Category("Cloud Foundry")]
        [DefaultValue(false)]
        public Boolean Deployable
        {
            get
            {
                return projectPackage.Deployable;
            }
            set
            {
                projectPackage.Deployable = value;
                SaveSettings();
            }
        }


        [Editor(typeof(FrameworkEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(FrameworkConverter))]
        [DisplayName("Framework")]
        [Description("The framework you application is implemented in.")]
        [Category("Cloud Foundry")]
        [DefaultValue("net")]
        public string Framework
        {
            get
            {
                return projectPackage.Framework;
            }
            set
            {
                projectPackage.Framework = value;
                SaveSettings();

                Client client = IntegrationCenter.CloudClient;
                if (client != null)
                {
                    Framework cloudFramework = client.Frameworks().FirstOrDefault(row => row.Name == projectPackage.Framework);
                    if (cloudFramework != null)
                    {
                        if (cloudFramework.Runtimes.Count > 0)
                        {
                            Runtime = cloudFramework.Runtimes[0].Name;
                        }
                    }
                }
            }
        }

        [DisplayName("Instance Count")]
        [Description("Number of instances to start.")]
        [Category("Cloud Foundry")]
        [DefaultValue(1)]
        public int InstanceCount
        {
            get
            {
                return projectPackage.InstanceCount;
            }
            set
            {
                projectPackage.InstanceCount = value;
                SaveSettings();
            }
        }

        [DisplayName("Memory")]
        [Description("Maximum memory available for your application to use per instance.")]
        [Category("Cloud Foundry")]
        [DefaultValue(128)]
        public int Memory
        {
            get
            {
                return projectPackage.Memory;
            }
            set
            {
                projectPackage.Memory = value;
                SaveSettings();
            }
        }

        [DisplayName("Application Name")]
        [Description("Name of the application.")]
        [Category("Cloud Foundry")]
        public string Name
        {
            get
            {
                return projectPackage.Name;
            }
            set
            {
                projectPackage.Name = value;
                SaveSettings();

                Client client = IntegrationCenter.CloudClient;
                if (client != null)
                {
                    if (projectPackage.Urls == null || projectPackage.Urls.Length == 0)
                    {
                        string[] defaultUrls = new string[1];
                        defaultUrls[0] = Utils.GetDefaultUrlForApp(projectPackage.Name.ToLower(), client.TargetUrl);
                        Urls = defaultUrls;
                    }
                }
            }
        }

        [DisplayName("Services")]
        [Description("Services to be bound to the application.")]
        [Category("Cloud Foundry")]
        [TypeConverter(typeof(ServicesConverter))]
        public CloudApplicationServiceProperties[] Services
        {
            get
            {
                if (projectPackage.Services != null)
                {
                    return projectPackage.Services.Select(service => new CloudApplicationServiceProperties(service)).ToArray();
                }
                else
                {
                    return new CloudApplicationServiceProperties[0];
                }
            }
            set
            {
                CloudApplicationService[] services = value.Select(cp => new CloudApplicationService() { ServiceType = cp.ServiceType, OverwriteExisting = cp.OverwriteExisting, ServiceName = cp.ServiceName }).ToArray();
                
                projectPackage.Services = services;
                SaveSettings();
            }
        }

        [DisplayName("URLs")]
        [Description("List of URLs that will bound to the application.")]
        [Category("Cloud Foundry")]
        [TypeConverter(typeof(UrlsConverter))]
        public string[] Urls
        {
            get
            {
                return projectPackage.Urls;
            }
            set
            {
                projectPackage.Urls = value;
                SaveSettings();
            }
        }

        [DisplayName("Environment Variables")]
        [Description("Variables that will be passed to the application at runtime.")]
        [Category("Cloud Foundry")]
        [TypeConverter(typeof(EnvironmentVariablesConverter))]
        public EnvironmentVariable[] Variables
        {
            get
            {
                if (projectPackage.Variables != null)
                {
                    return projectPackage.Variables;
                }
                else
                {
                    return new EnvironmentVariable[0];
                }
            }
            set
            {
                projectPackage.Variables = value;
                SaveSettings();
            }
        }

        [Editor(typeof(RuntimeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(RuntimeConverter))]
        [DisplayName("Runtime")]
        [Description("Runtime used to run the application in the cloud.")]
        [Category("Cloud Foundry")]
        [DefaultValue("iis")]
        public string Runtime
        {
            get
            {
                return projectPackage.Runtime;
            }
            set
            {
                projectPackage.Runtime = value;
                SaveSettings();
            }
        }

        public object VsProjectItem
        {
            get { return (object)_vsProject; }
        }
    }

    public class CloudApplicationServiceProperties : CloudApplicationService
    {
        [Editor(typeof(CloudApplicationServiceEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(CloudApplicationServiceConverter))]
        [DisplayName("Service Type")]
        [Description("Type of service that needs provisioning")]
        [DefaultValue("mssql")]
        public new string ServiceType
        {
            get
            {
                return base.ServiceType;
            }
            set
            {
                base.ServiceType = value;
            }
        }

        public CloudApplicationServiceProperties()
            : base()
        {
        }

        public CloudApplicationServiceProperties(CloudApplicationService service)
        {
            this.OverwriteExisting = service.OverwriteExisting;
            this.ServiceName = service.ServiceName;
            this.ServiceType = service.ServiceType;
        }

        public override string ToString()
        {
            return this.ServiceType + "#" + this.ServiceName;
        }
    }

}