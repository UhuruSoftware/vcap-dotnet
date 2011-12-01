using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net;
using System.IO;

namespace Uhuru.CloudFoundry.UI.Packaging
{

    public enum PushEventType
    {
        INFORMATION,
        WARNING,
        ERROR
    }

    public class PushEventArgs:EventArgs
    {
        public PushEventType EventType
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public int Progress
        {
            get;
            set;
        }

        public string ApplicationName
        {
            get;
            set;
        }
    }

    public class PackagePusher
    {
        int stepCount = 0;
        int step = 0;
        Client cloudClient = null;
        static int eventCounter = 0;

        private static void incHandlerCounter()
        {
            eventCounter++;
        }

        private static int getHandlerCounter()
        {
            return eventCounter;
        }

        public event EventHandler<PushEventArgs> OnPushEvent;

        public PackagePusher(Client cloudClient)
        {
            this.cloudClient = cloudClient;
            if(PackagePusher.getHandlerCounter() == 0)
            {
                cloudClient.UpdatePushStatus += new EventHandler<PushStatusEventArgs>(delegate(object sender, PushStatusEventArgs args)
                {
                    string message = String.Empty;
                    switch (args.Status)
                    {
                        case PushStatus.BINDING: message = "Binding services"; break;
                        case PushStatus.CHECKING: message = "Checking app status"; break;
                        case PushStatus.CREATING: message = "Creating app in the cloud"; break;
                        case PushStatus.PACKAGING: message = "Creating application package"; break;
                        case PushStatus.STAGING: message = "Staging the app"; break;
                        case PushStatus.STARTING: message = "App is starting"; break;
                        case PushStatus.UPLOADING: message = "Uploading app to cloud"; break;
                    }
                    UpdateStatus(PushEventType.INFORMATION, message, args.AppName);
                });
                PackagePusher.incHandlerCounter();
            }
        }

        private void UpdateStatus(PushEventType type, string message, string applicationName)
        {
            step++;
            if (OnPushEvent != null)
            {
                PushEventArgs args = new PushEventArgs();
                args.EventType = type;
                args.Message = message;
                args.Progress = Math.Min(Convert.ToInt32(((double)step / (double)stepCount) * 100), 100);
                args.ApplicationName = applicationName;
                OnPushEvent(this, args);
            }
        }

        private bool VerifySettingsCorrectness(CloudApplication package)
        {
            if (String.IsNullOrEmpty(package.Name))
            {
                UpdateStatus(PushEventType.ERROR, "Application must have a name!", "-");
                return false;
            }

            if (package.Urls.Length == 0)
            {
                UpdateStatus(PushEventType.ERROR, "Application must have at least one url!", package.Name);
                return false;
            }

            if (String.IsNullOrEmpty(package.Runtime))
            {
                UpdateStatus(PushEventType.ERROR, "Application must have a runtime!", package.Name);
                return false;
            }

            if (String.IsNullOrEmpty(package.Framework))
            {
                UpdateStatus(PushEventType.ERROR, "Application must have a framework!", package.Name);
                return false;
            }

            if (!cloudClient.Frameworks().Any(f => f.Name == package.Framework))
            {
                UpdateStatus(PushEventType.ERROR, "The framework of this application is currently not supported.", package.Name);
                return false;
            }

            if (!cloudClient.Runtimes().Any(r => r.Name == package.Runtime))
            {
                UpdateStatus(PushEventType.ERROR, "The runtime of this application is currently not supported.", package.Name);
                return false;
            }

            if (package.InstanceCount < 1)
            {
                UpdateStatus(PushEventType.ERROR, "Invalid instance count!", package.Name);
                return false;
            }
            
            if (package.Memory < 32)
            {
                UpdateStatus(PushEventType.ERROR, "Invalid memory setting! Set at least 32 MB.", package.Name);
                return false;
            }

            if (!package.Deployable)
            {
                UpdateStatus(PushEventType.ERROR, "Application is not marked as deployable. Ignoring.", package.Name);
                return false;
            }

            App existingApp = cloudClient.GetAppByName(package.Name);

            if (existingApp != null)
            {
                if (existingApp.StagingModel != package.Framework)
                {
                    UpdateStatus(PushEventType.ERROR, "Cannot update app that runs on a different framework.", package.Name);
                    return false;
                }
                if (existingApp.StagingStack != package.Runtime)
                {
                    UpdateStatus(PushEventType.ERROR, "Cannot update app that runs on a different runtime.", package.Name);
                    return false;
                }
            }
            return true;
        }

        private void PushServices(CloudApplication package)
        {
            foreach (CloudApplicationService service in package.Services)
            {
                if (!cloudClient.Services().Any(s => s.Vendor == service.ServiceType))
                {
                    UpdateStatus(PushEventType.WARNING,
                        String.Format("The service type {0} is not supported", service.ServiceType), package.Name);
                    continue;
                }
                
                if (!cloudClient.ProvisionedServices().Any(existingService => existingService.Name == service.ServiceName))
                {
                    if (cloudClient.CreateService(service.ServiceName, service.ServiceType))
                    {
                        UpdateStatus(PushEventType.INFORMATION,
                            String.Format("Created {0} service named {1}", service.ServiceName, service.ServiceType), package.Name);
                    }
                    else
                    {
                        UpdateStatus(PushEventType.WARNING,
                            String.Format("Could not create the {0} service named {1}", service.ServiceName, service.ServiceType), package.Name);
                    }
                }
                else
                {
                    if (service.OverwriteExisting)
                    {
                        if (cloudClient.DeleteService(service.ServiceName))
                        {
                            if (cloudClient.CreateService(service.ServiceName, service.ServiceType))
                            {
                                UpdateStatus(PushEventType.INFORMATION,
                                    String.Format("Recreated the {0} service named {1}", service.ServiceName, service.ServiceType), package.Name);
                            }
                            else
                            {
                                UpdateStatus(PushEventType.WARNING,
                                    String.Format("Could not recreate the {0} service named {1}", service.ServiceName, service.ServiceType), package.Name);
                            }
                        }
                        else
                        {
                            UpdateStatus(PushEventType.WARNING,
                                String.Format("Could not overwrite the {0} service named {1}", service.ServiceName, service.ServiceType), package.Name);
                        }
                    }
                    else
                    {
                        UpdateStatus(PushEventType.WARNING,
                            String.Format("{0} service named {1} already exists", service.ServiceName, service.ServiceType), package.Name);
                    }
                }
            }
        }

        private void BindServices(CloudApplication package)
        {
            string[] existingServices = cloudClient.GetAppServices(package.Name);

            foreach (CloudApplicationService appService in package.Services.Where(service => !existingServices.Any(existingService => existingService == service.ServiceName)))
            {
                if (cloudClient.BindService(package.Name, appService.ServiceName))
                {
                    UpdateStatus(PushEventType.INFORMATION,
                        String.Format("Successfuly bound service {0} to application named {1}", appService.ServiceName, package.Name), package.Name);
                }
                else
                {
                    UpdateStatus(PushEventType.WARNING,
                        String.Format("Could not bind service {0} to application named {1}", appService.ServiceName, package.Name), package.Name);
                }
            }
        }

        private void MapUrls(CloudApplication package)
        {
            foreach (string url in package.Urls.Skip(1))
            {
                if (cloudClient.MapUri(package.Name, url))
                {
                    UpdateStatus(PushEventType.INFORMATION,
                        String.Format("Mapped url {0} to {1}", url, package.Name), package.Name);
                }
                else
                {
                    UpdateStatus(PushEventType.WARNING,
                        String.Format("Could not map {0} to {1}", url, package.Name), package.Name);
                }
            }
        }

        private bool PushApp(CloudApplication package, bool debug)
        {

            if (!cloudClient.AppExists(package.Name))
            {
                if (!cloudClient.Push(package.Name, package.Urls[0], Path.GetDirectoryName(package.PackageFile), package.InstanceCount, package.Framework,
                    package.Runtime, package.Memory, package.Services.Select(service => service.ServiceName).ToList(), debug, false, false))
                {
                    UpdateStatus(PushEventType.ERROR,
                        String.Format("Could not push the {0} application named {1}", package.Framework, package.Name), package.Name);
                    return false;
                }
                else
                {
                    UpdateStatus(PushEventType.INFORMATION,
                        String.Format("Successfuly pushed the {0} application named {1}", package.Framework, package.Name), package.Name);
                }
            }
            else
            {
                if (!cloudClient.UpdateBits(package.Name, Path.GetDirectoryName(package.PackageFile), debug))
                {
                    UpdateStatus(PushEventType.ERROR,
                                       String.Format("Could not update the {0} application named {1}", package.Framework, package.Name), package.Name);
                    return false;
                }
                else
                {
                    UpdateStatus(PushEventType.INFORMATION,
                        String.Format("Successfuly updated the {0} application named {1}", package.Framework, package.Name), package.Name);
                }
            }
            return true;
        }

        private void StartApp(CloudApplication package)
        {
            if (cloudClient.GetAppByName(package.Name).State != "STARTED")
            {
                if (!cloudClient.StartApp(package.Name))
                {
                    UpdateStatus(PushEventType.WARNING,
                        String.Format("Could not start application {0}", package.Name), package.Name);
                }
                else
                {
                    UpdateStatus(PushEventType.INFORMATION,
                        String.Format("Started application {0}", package.Name), package.Name);
                }
            }
        }

        public bool Push(CloudApplication[] packages, bool debug = false)
        {

            stepCount = Convert.ToInt32(PushStatus.STARTING) * packages.Length +
                1 * packages.Length + // because enum is zero based
                1 * packages.Length + // start app steps
                packages.Sum(pack => pack.Urls.Length) +
                packages.Sum(pack => pack.Services.Length * 2);
            step = 0;

            foreach (CloudApplication package in packages)
            {
                if (!VerifySettingsCorrectness(package))
                {
                    return false;
                }

                PushServices(package);
                if (!PushApp(package, debug))
                {
                    return false;
                }
                BindServices(package);
                MapUrls(package);

                

            }
            stepCount = 1;
            step = 1;
            UpdateStatus(PushEventType.INFORMATION, String.Format("Done with app(s): [{0}]", String.Join(", ", packages.Select(package => package.Name).ToArray())), "-");
            return true;
        }
    }
}
