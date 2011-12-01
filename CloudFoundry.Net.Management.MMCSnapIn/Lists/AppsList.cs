using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ManagementConsole;
using System.Windows.Forms;
using System.Xml;
using CloudFoundry.Net;
using CloudFoundry.Net.Management.MMCSnapIn.ScopeNodes;
using System.Diagnostics;
using CloudFoundry.Net;

namespace CloudFoundry.Net.Management.MMCSnapIn.Lists
{
    public class AppsList : MmcListView
    {
        System.Timers.Timer refreshTimer;


        public AppsList()
        {
            refreshTimer = new System.Timers.Timer(5000);
            refreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(refreshTimer_Elapsed);
            refreshTimer.Enabled = true;
        }

        void refreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Client api = (Client)this.ScopeNode.Tag;
            List<App> apps = api.Apps();

            this.SnapIn.Invoke(new MethodInvoker(delegate()
                {
                    Refresh(apps);
                }));
        }

        protected override void OnInitialize(AsyncStatus status)
        {
            base.OnInitialize(status);
            this.Columns[0].Title = "App Name";
            this.Columns[0].SetWidth(200);
            this.Columns.Add(new MmcListViewColumn("Health", 100));
            this.Columns.Add(new MmcListViewColumn("URLs", 200));
            this.Columns.Add(new MmcListViewColumn("Services", 200));

            this.Mode = MmcListViewMode.Report;

            

            this.SelectionData.EnabledStandardVerbs = StandardVerbs.Delete;
            Microsoft.ManagementConsole.Action startAction = new Microsoft.ManagementConsole.Action("Start", "Start the App", 27, "StartApp");
            this.SelectionData.ActionsPaneItems.Add(startAction);
            Microsoft.ManagementConsole.Action stopAction = new Microsoft.ManagementConsole.Action("Stop", "Stop the App", 28, "StopApp");
            this.SelectionData.ActionsPaneItems.Add(stopAction);
            Microsoft.ManagementConsole.Action browseAction = new Microsoft.ManagementConsole.Action("Browse", "Browse the app in a browser", 31, "BrowseApp");
            this.SelectionData.ActionsPaneItems.Add(browseAction);


        }

        protected override void OnShow()
        {
            
            ((AppsScopeNode)this.ScopeNode).AppList = this;
            Refresh();
        }

        protected override void OnDelete(SyncStatus status)
        {
            Client api = (Client)this.ScopeNode.Tag;
            foreach (ResultNode cutNode in this.SelectedNodes)
            {
                string appName = cutNode.DisplayName;
                api.DeleteApp(appName);
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
                case "StartApp":
                    {
                        Client api = (Client)this.ScopeNode.Tag;
                        foreach (ResultNode cutNode in this.SelectedNodes)
                        {
                            string appName = cutNode.DisplayName;
                            api.StartApp(appName, false);
                        }
                        Refresh();
                    }
                    break;
                case "StopApp":
                    {
                        Client api = (Client)this.ScopeNode.Tag;
                        foreach (ResultNode cutNode in this.SelectedNodes)
                        {
                            string appName = cutNode.DisplayName;
                            api.StopApp(appName);
                        }
                        Refresh();
                    }
                    break;
                case "BrowseApp":
                    {
                        foreach (ResultNode cutNode in this.SelectedNodes)
                        {
                            if (cutNode.SubItemDisplayNames[1] != null)
                            {
                                string[] urls = cutNode.SubItemDisplayNames[1].Split(',');

                                foreach (string url in urls)
                                {
                                    //TODO: Not sure if this is the best way to prefix the URL. May be https sometime?
                                    Utils.OpenLink("http://" + url.Trim());
                                }
                            }
                        }
                        Refresh();
                    }
                    break;
            }
        }

        private void ShowSelected()
        {
        }

        public void Refresh(List<App> apps = null)
        {
            refreshTimer.Enabled = false;
            if (apps == null)
            {
                Client api = (Client)this.ScopeNode.Tag;
                apps = api.Apps();
            }

            foreach (App app in apps)
            {
                ResultNode rNode;

                if (!this.ResultNodes.Cast<ResultNode>().Any(node => node.DisplayName == app.Name))
                {
                    rNode = new ResultNode();
                    rNode.DisplayName = app.Name;
                    rNode.SubItemDisplayNames.Add(app.RunningInstances + "/" + app.Instances);
                    rNode.SubItemDisplayNames.Add(app.Uris);
                    rNode.SubItemDisplayNames.Add(app.Services);
                    this.ResultNodes.Add(rNode);
                }
                else
                {
                    rNode = this.ResultNodes.Cast<ResultNode>().First(node => node.DisplayName == app.Name);
                    
                    rNode.SubItemDisplayNames[0] = app.RunningInstances + "/" + app.Instances;
                    rNode.SubItemDisplayNames[1] = app.Uris;
                    rNode.SubItemDisplayNames[2] = app.Services;
                }

                if (app.RunningInstances == app.Instances)
                {
                    rNode.ImageIndex = 1;
                }
                else if (app.RunningInstances == "0")
                {
                    rNode.ImageIndex = 7;
                }
                else
                {
                    rNode.ImageIndex = 6;
                }
            }

            ResultNode[] nodesToDelete = this.ResultNodes.Cast<ResultNode>().Where(r => !apps.Exists(app => app.Name == r.DisplayName)).ToArray();
            
            for (int i=0; i < nodesToDelete.Length; i++)
            {
                this.ResultNodes.Remove(nodesToDelete[i]);
            }

            refreshTimer.Enabled = true;
        }
    }
}
