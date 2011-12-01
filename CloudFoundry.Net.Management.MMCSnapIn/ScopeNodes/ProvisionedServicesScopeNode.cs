using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ManagementConsole;
using System.Xml;
using System.IO;
using CloudFoundry.Net.Management.MMCSnapIn.Lists;
using CloudFoundry.Net.Management.MMCSnapIn.Forms;
using Uhuru.CloudFoundry.UI;

namespace CloudFoundry.Net.Management.MMCSnapIn.ScopeNodes
{
    class ProvisionedServicesScopeNode:ScopeNode
    {
        private ProvisionedServicesList servicesList = null;

        public ProvisionedServicesList ServicesList
        {
            get { return servicesList; }
            set { servicesList = value; }
        }

        public ProvisionedServicesScopeNode() : base(true)
        {
            this.DisplayName = "Provisioned Services";
            this.ImageIndex = 4;
            this.SelectedImageIndex = 4;

            Microsoft.ManagementConsole.Action actionCreate = new Microsoft.ManagementConsole.Action("Create...", "Provision a new service", 30, "CreateService");
            this.ActionsPaneItems.Add(actionCreate);
        
            this.EnabledStandardVerbs = StandardVerbs.Refresh;
        }

        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "CreateService":
                    {
                        AddServiceForm serviceForm = new AddServiceForm((Client)this.Tag);
                        if (this.SnapIn.Console.ShowDialog(serviceForm) == System.Windows.Forms.DialogResult.OK)
                        {
                            if (servicesList != null)
                            {
                                servicesList.Refresh();
                            }
                        }
                    }
                    break;
            }
        }
   }
}
