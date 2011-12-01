using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Uhuru.CloudFoundry.UI.VS2010.Extensions;
using System.ComponentModel.Design;
using CloudFoundry.Net;
using Uhuru.CloudFoundry.UI.Packaging;
using Uhuru.CloudFoundry.UI.Controls;
using System.Windows;
using System.Threading;
using System.Globalization;
using System.Collections;


namespace Uhuru.CloudFoundry.UI.VS2010
{
    [Guid(GuidList.guidUhuruCloudFoundryUIVS2010PkgString)]
    public class CloudPackage : UhuruCloudFoundryUIVS2010PackageBase
    {
        List<int> dynamicExtenderProviderCookies = new List<int>();
        ObjectExtenders extensionManager;
        private static int _handlerCounter = 0;
        AutoResetEvent buildDoneResetEvent = new AutoResetEvent(false);

        private static int handlerCounter()
        {
            return _handlerCounter++;
        }

        protected override void Initialize()
        {
            base.Initialize();
            RegisterExtensions();
        }

        protected override void Dispose(bool disposing)
        {
            UnregisterExtensions();
            base.Dispose(disposing);
        }

        private void RegisterExtensions()
        {
            extensionManager = (ObjectExtenders)GetService(typeof(ObjectExtenders));
            if (extensionManager == null)
            {
                throw new NullReferenceException("GetService failed to get the extender object");
            }

            CloudProjectExtenderProvider dynamicExtenderProvider = new CloudProjectExtenderProvider();

            foreach (string objectToExtend in CloudProjectExtenderProvider.ProjectTypesToExtend)
            {
                dynamicExtenderProviderCookies.Add(extensionManager.RegisterExtenderProvider(
                    objectToExtend, CloudProjectExtenderProvider.DynamicExtenderName, dynamicExtenderProvider));
            }
        }

        private void UnregisterExtensions()
        {
            if (extensionManager != null)
            {
                foreach (int dynamicExtenderProviderCookie in dynamicExtenderProviderCookies)
                {
                    if (dynamicExtenderProviderCookie != 0)
                    {
                        extensionManager.UnregisterExtenderProvider(dynamicExtenderProviderCookie);
                    }
                }
            }
        }

        protected override void ButtonBuildAllAndPushExecuteHandler(object sender, EventArgs e)
        {
            if (IntegrationCenter.GetActiveEnvironment().Solution.IsOpen)
            {
                Client targetAPI = IntegrationCenter.CloudClient;
                if (targetAPI == null)
                {
                    return;
                }

                buildIfNecessary(true);

                CloudProjectProperties[] deployableApps = IntegrationCenter.DeployableApplications;

                PackagePusher packagePusher = new PackagePusher(targetAPI);

                PushInformationGrid infoGrid = IntegrationCenter.PushInfoGrid;
                infoGrid.PackagePusher = packagePusher;

                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object data)
                     {
                         buildDoneResetEvent.WaitOne(-1);
                         packagePusher.Push(deployableApps.Select(app => app.PushablePackage).ToArray());
                     }));
            }
        }

        protected override void ButtonBuildAndPushProjectExecuteHandler(object sender, EventArgs e)
        {
            if (IntegrationCenter.GetActiveEnvironment().Solution.IsOpen)
            {
                CloudProjectProperties selectedApp = IntegrationCenter.SelectedDeployableApp;
                if (selectedApp != null)
                {
                    Client targetAPI = IntegrationCenter.CloudClient;
                    if (targetAPI == null)
                    {
                        return;
                    }

                    buildIfNecessary(false);

                    PackagePusher packagePusher = new PackagePusher(targetAPI);

                    PushInformationGrid infoGrid = IntegrationCenter.PushInfoGrid;
                    infoGrid.PackagePusher = packagePusher;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object data)
                    {
                        buildDoneResetEvent.WaitOne(-1);
                        packagePusher.Push(new CloudApplication[] { selectedApp.PushablePackage });
                    }));
                }
                else
                {
                    MessageBox.Show("The selected project is not marked as deployable.\r\nMake sure to set 'Deployable' to true, so the cloud pusher can pick it up.", "Cloud Foundry Extension", MessageBoxButton.OK, MessageBoxImage.Information);
                } 
            }
        }

        private void buildIfNecessary(bool multipleSelection)
        {
            CloudProjectProperties[] deployableProjects = null;
            DTE env = IntegrationCenter.GetActiveEnvironment();

            if(CloudPackage.handlerCounter() == 0)
                IntegrationCenter.EnvBuildEvents.OnBuildDone += new _dispBuildEvents_OnBuildDoneEventHandler(EnvBuildEvents_OnBuildDone);
           
            if (env.Solution.IsOpen)
            {
                if (multipleSelection)
                    deployableProjects = IntegrationCenter.DeployableApplications;
                else
                {
                    deployableProjects = new CloudProjectProperties[1];
                    deployableProjects[0] = IntegrationCenter.SelectedDeployableApp;
                }

                SolutionBuild slnBuild = env.Solution.SolutionBuild;

                SolutionConfiguration activeConfig = slnBuild.ActiveConfiguration;

                foreach (CloudProjectProperties projectProps in deployableProjects)
                {
                    Project proj = (Project)projectProps.VsProjectItem;
                    
                    slnBuild.BuildProject(activeConfig.Name, proj.UniqueName, false);
                }
            }
        }

        void EnvBuildEvents_OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {
            buildDoneResetEvent.Set();
        }
    }
}