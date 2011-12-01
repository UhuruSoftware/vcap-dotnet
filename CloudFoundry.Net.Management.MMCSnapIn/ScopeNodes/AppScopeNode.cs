using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ManagementConsole;
using System.Xml;

namespace CloudFoundry.Net.Management.MMCSnapIn.ScopeNodes
{
    class AppScopeNode : ScopeNode
    {
        private Client api = null;
        private App app = null;

        public App App
        {
            get { return app; }
            set { 
                    app = value;
                    RefreshShownInfo(app);
                }
        }


        public Client Api
        {
            get { return api; }
            set { 
                    api = value;
                }
        }
                
        public AppScopeNode()
        {
            this.EnabledStandardVerbs = StandardVerbs.Delete;
            Microsoft.ManagementConsole.Action startAction = new Microsoft.ManagementConsole.Action("Start", "Start the App", 27, "StartApp");
            this.ActionsPaneItems.Add(startAction);
            Microsoft.ManagementConsole.Action stopAction = new Microsoft.ManagementConsole.Action("Stop", "Stop the App", 28, "StopApp");
            this.ActionsPaneItems.Add(stopAction);
        }


        private void RefreshShownInfo(App app)
        {
            this.ImageIndex = Utils.GetAppStateImageIndex(app);
            this.SelectedImageIndex = this.ImageIndex;
            this.DisplayName = app.Name;
        }

        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            try
            {
                switch ((string)action.Tag)
                {
                    case "StartApp":
                        {
                            if (app != null && api != null) api.StartApp(app.Name, false);
                            break;
                        }
                        break;
                    case "StopApp":
                        {
                            if (app != null && api != null) api.StopApp(app.Name);
                            break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
    }
}
