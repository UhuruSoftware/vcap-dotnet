using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ManagementConsole;
using System.Windows.Forms;
using System.Xml;
using CloudFoundry.Net;
using CloudFoundry.Net.Management.MMCSnapIn.ScopeNodes;
using CloudFoundry.Net;

namespace CloudFoundry.Net.Management.MMCSnapIn.Lists
{
    public class ProvisionedServicesList : MmcListView
    {
        System.Timers.Timer refreshTimer;

        public ProvisionedServicesList()
        {
            refreshTimer = new System.Timers.Timer(5000);
            refreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(refreshTimer_Elapsed);
            refreshTimer.Enabled = true;
        }

        void refreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.SnapIn.Invoke(new MethodInvoker(delegate()
            {
                Refresh();
            }));
        }

        protected override void OnInitialize(AsyncStatus status)
        {
            base.OnInitialize(status);

            this.Columns[0].Title = "Service Name";
            this.Columns[0].SetWidth(200);

            this.Columns.Add(new MmcListViewColumn("Type", 200));

            this.Mode = MmcListViewMode.Report;

            this.SelectionData.EnabledStandardVerbs = StandardVerbs.Delete;
        }

        protected override void OnShow()
        {
            ((ProvisionedServicesScopeNode)this.ScopeNode).ServicesList = this;
            Refresh();
        }

        protected override void OnDelete(SyncStatus status)
        {
            Client api = (Client)this.ScopeNode.Tag;
            foreach (ResultNode cutNode in this.SelectedNodes)
            {
                string serviceName = cutNode.DisplayName;
                api.DeleteService(serviceName);
            }
            Refresh();
        }

        protected override void OnSelectionChanged(SyncStatus status)
        {
            if (this.SelectedNodes.Count == 0)
            {
                this.SelectionData.Clear();
            }
            else
            {
                this.SelectionData.Update(null, this.SelectedNodes.Count > 1, null, null);
            }
        }

        protected override void OnRefresh(AsyncStatus status)
        {
            Refresh();
        }

        protected override void OnSelectionAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
            }
        }

        private void ShowSelected()
        {
        }


        public void Refresh()
        {
            refreshTimer.Enabled = false;
            Client api = (Client)this.ScopeNode.Tag;
            List<ProvisionedService> provisionedServices = api.ProvisionedServices();

            foreach (ProvisionedService provisionedService in provisionedServices)
            {
                ResultNode rNode;

                if (!this.ResultNodes.Cast<ResultNode>().Any(node => node.DisplayName == provisionedService.Name))
                {
                    rNode = new ResultNode();
                    rNode.DisplayName = provisionedService.Name;
                    rNode.SubItemDisplayNames.Add(provisionedService.Vendor);
                    this.ResultNodes.Add(rNode);
                }
                else
                {
                    rNode = this.ResultNodes.Cast<ResultNode>().First(node => node.DisplayName == provisionedService.Name);

                    rNode.SubItemDisplayNames[0] = provisionedService.Vendor;
                }


                rNode.ImageIndex = Utils.GetServiceImageIndex(provisionedService.Vendor);
            }

            ResultNode[] nodesToDelete = this.ResultNodes.Cast<ResultNode>().Where(r => !provisionedServices.Exists(service => service.Name == r.DisplayName)).ToArray();

            for (int i = 0; i < nodesToDelete.Length; i++)
            {
                this.ResultNodes.Remove(nodesToDelete[i]);
            }

            refreshTimer.Enabled = true;
        }
    }
}
