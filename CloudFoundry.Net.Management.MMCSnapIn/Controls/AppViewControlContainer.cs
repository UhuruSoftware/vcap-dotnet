using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CloudFoundry.Net.Management.MMCSnapIn.Forms;
using CloudFoundry.Net.Management.MMCSnapIn.FormViews;
using Microsoft.ManagementConsole;
using System.Reflection;
using System.Drawing;

namespace CloudFoundry.Net.Management.MMCSnapIn.Controls
{
    public partial class AppViewControlContainer : UserControl, IFormViewControl
    {
        AppsFormView parentFormView = null;
        List<ProvisionedService> allServices = new List<ProvisionedService>(); // we need this to know the type of each service of an app
        
        private enum GridColumns
        { 
            AppName = 0,
            Health,
            URLs,
            Services,
            Framework,
            Runtime
        }

        public List<App> SelectedApps
        {
            get {
                    List<App> selectedApps = new List<App>();
                    foreach (DataGridViewRow row in dataGrid.SelectedRows)
                    {
                        selectedApps.Add(row.Tag as App);
                    }

                    return selectedApps; 
                }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public AppViewControlContainer()
        {
            // Initialize the control.
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            Assembly asm = Assembly.GetExecutingAssembly();
            imageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("CloudFoundry.Net.Management.MMCSnapIn.Images.smallicons.bmp")));

            dataGrid.SelectionChanged += new Uhuru.CloudFoundry.UI.AppsDataGrid.SelectionChangedEventHandler(dataGrid_SelectionChanged);
            dataGrid.RightClick += new Uhuru.CloudFoundry.UI.AppsDataGrid.RightClickEventHandler(dataGrid_RightClick);
                
            this.AllowDrop = true;
            dataGrid.AllowDrop = true;
        }
                
        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="parentSelectionFormView"></param>
        void IFormViewControl.Initialize(FormView parentSelectionFormView)
        {
            parentFormView = (AppsFormView)parentSelectionFormView;
            dataGrid.SetClient(parentFormView.ScopeNode.Tag as Client);
        }

        public void RefreshData()
        {
            dataGrid.RefreshData();
        }

        private void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count > 0)
            {
                // Update MMC with the current selection information
                parentFormView.SelectionData.Update(dataGrid.GetSelectedAppNames(), dataGrid.SelectedRows.Count > 1, null, null);

                // Update the title of the selected data menu in the actions pane
                parentFormView.SelectionData.DisplayName = string.Join(", ", dataGrid.GetSelectedAppNames());
            }
            else //deselect
            {
                //let the formview know, in order to clear the right hand pane
                parentFormView.SelectionData.Clear();
            }
        }

        private void dataGrid_RightClick(object sender, MouseEventArgs e, bool clickedOnSelection)
        {
            parentFormView.ShowContextMenu(PointToScreen(e.Location), clickedOnSelection);
        } 
    }
}
