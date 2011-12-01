using System.Xml;
using CloudFoundry.Net.Management.MMCSnapIn.FormViews;
using CloudFoundry.Net.Management.MMCSnapIn.Lists;
using Microsoft.ManagementConsole;
using CloudFoundry.Net.Management.MMCSnapIn.Controls;
using Uhuru.CloudFoundry.UI;
using System;

namespace CloudFoundry.Net.Management.MMCSnapIn.ScopeNodes
{
    class TargetScopeNode:ScopeNode
    {
        private CloudTarget cloudTarget = null;
        private Client client = null;

        public event Action<TargetScopeNode> NodeAsksForRemoval;

        public TargetScopeNode(CloudTarget targetNode)
        {
            
            this.ImageIndex = 19;
            this.SelectedImageIndex = 19;
            Microsoft.ManagementConsole.Action action = new Microsoft.ManagementConsole.Action("Remove", "Remove this target", 32, "RemoveTarget");
            this.ActionsPaneItems.Add(action);

            cloudTarget = targetNode;
            client = new Client();

            string target = targetNode.TargetUrl;
            string email = targetNode.Username;
            string password = CloudCredentialsEncryption.GetUnsecureString(targetNode.Password);

            client.Target(target);
            client.Login(email, password);

            AppsScopeNode apps = new AppsScopeNode();
            ProvisionedServicesScopeNode provisionedServices = new ProvisionedServicesScopeNode();
            UsersScopeNode users = new UsersScopeNode();
            CloudInfoScopeNode cloudInfo = new CloudInfoScopeNode();

            apps.Tag = client;
            provisionedServices.Tag = client;
            users.Tag = client;
            cloudInfo.Tag = client;

            FormViewDescription fwdApps = new FormViewDescription();
            fwdApps.DisplayName = "Deployed Applications";
            fwdApps.ViewType = typeof(AppsFormView);
            fwdApps.ControlType = typeof(AppViewControlContainer);
            apps.ViewDescriptions.Add(fwdApps);
            apps.ViewDescriptions.DefaultIndex = 0;

            MmcListViewDescription lvdUsers = new MmcListViewDescription();
            lvdUsers.DisplayName = "Users";
            lvdUsers.ViewType = typeof(UsersList);
            lvdUsers.Options = MmcListViewOptions.ExcludeScopeNodes | MmcListViewOptions.SingleSelect;
            users.ViewDescriptions.Add(lvdUsers);
            users.ViewDescriptions.DefaultIndex = 0;
            
            MmcListViewDescription lvdProvisionedServices = new MmcListViewDescription();
            lvdProvisionedServices.DisplayName = "Provisioned Services";
            lvdProvisionedServices.ViewType = typeof(ProvisionedServicesList);
            lvdProvisionedServices.Options = MmcListViewOptions.ExcludeScopeNodes;
            provisionedServices.ViewDescriptions.Add(lvdProvisionedServices);
            provisionedServices.ViewDescriptions.DefaultIndex = 0;

            FormViewDescription fwdCloudInfo = new FormViewDescription();
            fwdCloudInfo.DisplayName = "Cloud Info";
            fwdCloudInfo.ViewType = typeof(CloudInfoFormView);
            fwdCloudInfo.ControlType = typeof(CloudInfoContainer);
            cloudInfo.ViewDescriptions.Add(fwdCloudInfo);
            cloudInfo.ViewDescriptions.DefaultIndex = 0;

            this.Children.Add(apps);
            this.Children.Add(cloudInfo);
            this.Children.Add(provisionedServices);
            this.Children.Add(users);
 
        }


        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "RemoveTarget":
                    {
                        RemoveTargetFromRegistry();
                        //remove from tree
                        if (NodeAsksForRemoval != null)
                            NodeAsksForRemoval(this);

                        break;
                    }
            }
        }

        private void RemoveTargetFromRegistry()
        {
            CloudTargetManager manager = new CloudTargetManager();
            manager.RemoveTarget(cloudTarget);
        }



    }
}
