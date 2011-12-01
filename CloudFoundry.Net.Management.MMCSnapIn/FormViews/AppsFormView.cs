using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Management.MMCSnapIn.Controls;
using Microsoft.ManagementConsole;
using System.Windows.Forms;
using Uhuru.CloudFoundry.UI.Forms;

namespace CloudFoundry.Net.Management.MMCSnapIn.FormViews
{
    public class AppsFormView : FormView
    {
        private AppViewControlContainer contentContainer = new AppViewControlContainer();
       
        /// <summary>
        /// Constructor.
        /// </summary>
        public AppsFormView()
        {
       
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnInitialize(AsyncStatus status)
        {
            // Call the parent method.
            base.OnInitialize(status);

            this.SelectionData.EnabledStandardVerbs = StandardVerbs.Delete;
            Microsoft.ManagementConsole.Action startAction = new Microsoft.ManagementConsole.Action("Start", "Start the App", (int)Utils.SmallImages.Start, "StartApp");
            this.SelectionData.ActionsPaneItems.Add(startAction);
            Microsoft.ManagementConsole.Action stopAction = new Microsoft.ManagementConsole.Action("Stop", "Stop the App", (int)Utils.SmallImages.Stop, "StopApp");
            this.SelectionData.ActionsPaneItems.Add(stopAction);
            Microsoft.ManagementConsole.Action browseAction = new Microsoft.ManagementConsole.Action("Browse", "Browse the app in a browser", (int)Utils.SmallImages.BrowseSite, "BrowseApp");
            this.SelectionData.ActionsPaneItems.Add(browseAction);

            Microsoft.ManagementConsole.Action editServices = new Microsoft.ManagementConsole.Action("Services...", "Add/Remove Services", (int)Utils.SmallImages.Modules, "EditServices");
            this.SelectionData.ActionsPaneItems.Add(editServices);
            Microsoft.ManagementConsole.Action editURLs = new Microsoft.ManagementConsole.Action("URLs...", "Add/Remove URLs", (int)Utils.SmallImages.Modules, "EditURLs");
            this.SelectionData.ActionsPaneItems.Add(editURLs);
            
            // Get a typed reference to the hosted control
            // that is set up by the form view description.
            contentContainer = (AppViewControlContainer)this.Control;
            Refresh();
        }
        /// <summary>
        /// Load the data.
        /// </summary>
        protected void Refresh()
        {
           contentContainer.RefreshData();            
        }
        /// <summary>
        /// Handle the selected action.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="status"></param>
        protected override void OnSelectionAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            try
            {
                Client api = (Client)this.ScopeNode.Tag;
                switch ((string)action.Tag)
                {
                    case "StartApp":
                        {
                            contentContainer.SelectedApps.ForEach(app =>
                                        api.StartApp(app.Name, false));

                            Refresh();
                            break;
                        }
                    case "StopApp":
                        {
                            contentContainer.SelectedApps.ForEach(app =>
                                        api.StopApp(app.Name));

                            Refresh();
                            break;
                        }
                    case "BrowseApp":
                        {
                            contentContainer.SelectedApps.ForEach(app =>
                                {
                                    string[] urls = app.UriList;
                                    foreach (string url in urls) Utils.OpenLink(url);
                                });
                            
                            break;
                        }
                    case "EditServices":
                        {
                            EditServices editServicesForm = new EditServices();
                            editServicesForm.Api = api;
                            editServicesForm.App = contentContainer.SelectedApps[0];
                            editServicesForm.ShowDialog();

                            if (editServicesForm.DialogResult == DialogResult.OK)
                                    Refresh();
                            break;
                        }
                    case "EditURLs":
                        {
                            EditURLs editUrlsForm = new EditURLs();
                            editUrlsForm.Api = api;
                            editUrlsForm.App = contentContainer.SelectedApps[0];
                            editUrlsForm.ShowDialog();

                            if (editUrlsForm.DialogResult == DialogResult.OK)
                                Refresh();

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        protected override void OnDelete(SyncStatus status)
        {
            Client api = (Client)this.ScopeNode.Tag;

            contentContainer.SelectedApps.ForEach(app => api.DeleteApp(app.Name));
            
            Refresh();
        }

        protected override void OnRefresh(AsyncStatus status)
        {
            Refresh();
        }
    }
}
