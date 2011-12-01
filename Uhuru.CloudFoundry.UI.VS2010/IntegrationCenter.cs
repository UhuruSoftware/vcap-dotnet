using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net;
using Uhuru.CloudFoundry.UI.Packaging;
using Uhuru.CloudFoundry.UI.VS2010;
using EnvDTE;
using Uhuru.CloudFoundry.UI.VS2010.Extensions;
using Microsoft.VisualStudio.Shell;
using Uhuru.CloudFoundry.UI.Controls;

namespace Uhuru.CloudFoundry.UI.VS2010
{
    class IntegrationCenter
    {
        public const string CloudConfigurationFile = "uhurucloud.uhcf";
        private static Client cloudClient = null;
        private static CloudTargetManager targetManagerInstance;
        private static readonly object targetManagerLock = new object();
        private static PushInformationGrid pushInfoGrid;
        
        private static EnvDTE.Events envGlobalEvents = null;
        private static EnvDTE.BuildEvents envBuildEvents = null;
        private static EnvDTE.SolutionEvents envSolutionEvents = null;




        public static Client CloudClient
        {
            get
            {
                return cloudClient;
            }
            set
            {
                cloudClient = value;
            }
        }

        public static CloudProjectProperties[] DeployableApplications
        {
            get
            {
                List<CloudProjectProperties> result = new List<CloudProjectProperties>();

                foreach (Project project in GetActiveEnvironment().Solution.Projects)
                {
                    CloudProjectProperties projectCloudProperties = new CloudProjectProperties();
                    projectCloudProperties.Initialize(project);
                    if (projectCloudProperties.Deployable)
                    {
                        result.Add(projectCloudProperties);
                    }
                }

                return result.ToArray();
            }
        }

        public static CloudProjectProperties SelectedDeployableApp
        {
            get
            {
                object[] projects;
                Project project;

                projects = (object[])IntegrationCenter.GetActiveEnvironment().ActiveSolutionProjects;

                if (projects.Length > 0)
                {
                    project = projects.GetValue(0) as EnvDTE.Project;
                }
                else
                {
                    return null;
                }

                CloudProjectProperties result = new CloudProjectProperties();
                result.Initialize(project);

                if (result.Deployable == true)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public static Uhuru.CloudFoundry.UI.CloudTargetManager GetCloudTargetManager()
        {
            if (targetManagerInstance == null)
            {
                lock (targetManagerLock)
                {
                    if (targetManagerInstance == null)
                        targetManagerInstance = new CloudTargetManager();
                }
            }
            return targetManagerInstance;
        }

        public static DTE GetActiveEnvironment()
        {
            DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));
            return dte;
        }

        private static void setEvents()
        {
            envGlobalEvents = GetActiveEnvironment().Events;

            envBuildEvents = envGlobalEvents.BuildEvents;

            envSolutionEvents = envGlobalEvents.SolutionEvents;
        }

        public static EnvDTE.BuildEvents EnvBuildEvents
        {
            get
            {
                if (envBuildEvents == null)
                    setEvents();

                return envBuildEvents;
            }
        }

        public static PushInformationGrid PushInfoGrid
        {
            get
            {
                return pushInfoGrid;
            }
            set
            {
                pushInfoGrid = value;
            }
        }

    }
}
