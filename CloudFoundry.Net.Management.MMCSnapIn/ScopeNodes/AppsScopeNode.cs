using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ManagementConsole;
using System.Xml;
using System.IO;
using CloudFoundry.Net.Management.MMCSnapIn.Forms;
using CloudFoundry.Net.Management.MMCSnapIn.Lists;
using Uhuru.CloudFoundry.UI;

namespace CloudFoundry.Net.Management.MMCSnapIn.ScopeNodes
{
    class AppsScopeNode:ScopeNode
    {

        private AppsList appList = null;

        public AppsList AppList
        {
            get { return appList; }
            set { appList = value; }
        }


        public AppsScopeNode() : base(true)
        {
            this.DisplayName = "Apps";
            this.ImageIndex = 20;
            this.SelectedImageIndex = 20;

            Microsoft.ManagementConsole.Action actionPush = new Microsoft.ManagementConsole.Action("Push...", "Push a new app to CloudFoundry.", 30, "PushApp");
            Microsoft.ManagementConsole.Action actionBulkPush = new Microsoft.ManagementConsole.Action("Bulk Push...", "Push multiple apps to CloudFoundry.", 29, "BulkPush");
            this.ActionsPaneItems.Add(actionPush);
            this.ActionsPaneItems.Add(actionBulkPush);

            this.EnabledStandardVerbs = StandardVerbs.Refresh;

        }

        private void RefreshChildNodes()
        {
            try
            {
                Client api = this.Tag as Client;
                if (api == null) return;

                List<App> apps = api.Apps();
                apps.ForEach(app =>
                {
                    //if an node corresponding to this app exists, update info
                    AppScopeNode nodeOfApp = this.Children.ToArray().Where(n => n.DisplayName == app.Name).FirstOrDefault() as AppScopeNode;
                    if (nodeOfApp != null)
                    {
                        nodeOfApp.ViewDescriptions = new ViewDescriptionCollection();
                        nodeOfApp.App = app;

                        HtmlViewDescription view = new HtmlViewDescription(GetAppUrl(app));
                        view.DisplayName = app.Name;
                        nodeOfApp.ViewDescriptions.Add(view);
                    }
                    else // add it
                    {
                        AppScopeNode node = new AppScopeNode();
                        node.App = app;
                        node.Api = api;

                        HtmlViewDescription view = new HtmlViewDescription(GetAppUrl(app));
                        view.DisplayName = app.Name;
                        
                        node.ViewDescriptions.Add(view);
                        this.Children.Add(node);
                    }
                });

                //delete nodes that no longer have apps
                for (int i = this.Children.Count - 1; i >= 0; i--)
                {
                    ScopeNode node = this.Children[i];
                    if (!apps.Any(app => app.Name == node.DisplayName))
                        this.Children.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                throw;
            }
        }

        public new object Tag
        {
            get { return base.Tag; }
            set {
                    base.Tag = value;
                    RefreshChildNodes();
                }
        }
        
        private Uri GetAppUrl(App app)
        {
            string[] urls = app.UriList;
            return new Uri(urls.Length == 0 ? string.Empty : "http://" + urls[0]);
        }

        protected override void OnRefresh(AsyncStatus status)
        {
            RefreshShownInfo();
        }

        private void RefreshShownInfo()
        {
            //if (appList != null)
            //{
            //    appList.Refresh();
            //}

            RefreshChildNodes();
        }

        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "PushApp":
                    {
                        PushAppForm pushForm = new PushAppForm((Client)this.Tag);
                        if (this.SnapIn.Console.ShowDialog(pushForm) == System.Windows.Forms.DialogResult.OK)
                        {
                            RefreshShownInfo();
                        }
                    }
                    break;
                case "BulkPush":
                    {
                        BulkPushForm pushForm = new BulkPushForm((Client)this.Tag);
                        this.SnapIn.Console.ShowDialog(pushForm);
                        RefreshShownInfo();
                    }
                    break;
            }
        }
   }
}
