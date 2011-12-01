using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CloudFoundry.Net;
using System.Reflection;

namespace Uhuru.CloudFoundry.UI
{
    public partial class AppsDataGrid : UserControl
    {
        private List<ProvisionedService> allServices = new List<ProvisionedService>(); // we need this to know the type of each service of an app
        private ImageList imageList = new ImageList();
        private Client api = new Client();
        private System.Timers.Timer refreshTimer;
        private string[] restrictedAppSet = null; //for the times when we only want to show a subset of apps
        private bool contextMenuEnabled = false;

        public delegate void SelectionChangedEventHandler(object sender, EventArgs e);
        public event SelectionChangedEventHandler SelectionChanged;

        public delegate void RightClickEventHandler(object sender, MouseEventArgs e, bool clickedOnSelection);
        public event RightClickEventHandler RightClick; 

        private enum GridColumns
        { 
            AppName = 0,
            Health,
            URLs,
            Services,
            Framework,
            Runtime
        }
        
        public bool ContextMenuEnabled
        {
            get { return contextMenuEnabled; }
            set { contextMenuEnabled = value; }
        }

        public Font Font
        {
            get { return this.dataGrid.Font; }
            set {
                    foreach (Control c in this.Controls)
                    {
                        c.Font = value;
                    }
                }
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

        public void SetClient(Client apiClient)
        {
            this.api = apiClient;
        }

        public AppsDataGrid()
        {
            // Initialize the control.
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            dataGrid.DragEnter += new DragEventHandler(dataGrid_DragEnter);
            dataGrid.DragDrop += new DragEventHandler(dataGrid_DragDrop);

            this.AllowDrop = true;
            dataGrid.AllowDrop = true;

            Assembly asm = Assembly.GetExecutingAssembly();
            imageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("Uhuru.CloudFoundry.UI.Images.smallicons.bmp")));
            imageList.TransparentColor = Color.White;

            startApps.Image = imageList.Images[(int)Utils.SmallImages.Start];
            stopApps.Image = imageList.Images[(int)Utils.SmallImages.Stop];
            browseApps.Image = imageList.Images[(int)Utils.SmallImages.BrowseSite];
            removeApps.Image = imageList.Images[(int)Utils.SmallImages.Delete];

            refreshTimer = new System.Timers.Timer(5000);
            refreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(refreshTimer_Elapsed);
            refreshTimer.Enabled = true;
        }

        void refreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(delegate()
                            {
                                RefreshData();
                            }));
                }
                catch (Exception ex)
                {
                    //TODO: get actual exception type
                    //this should only fail when the current thread is destroyed as the form is closing
                    //so we swallow it and move on
                }
            }
            else
                RefreshData();
        }

        public DataGridViewSelectedRowCollection SelectedRows
        {    
            get { return dataGrid.SelectedRows; }
        }

        public void Reset()
        {
            api.Logout();
            dataGrid.Rows.Clear();
        }


        private void dataGrid_DragDrop(object sender, DragEventArgs e)
        {
            string[] directories = (string[])e.Data.GetData(DataFormats.FileDrop);

            BulkPushForm bpf = new BulkPushForm(api);
            bpf.LoadFromDirectories(directories);
            bpf.TopMost = true;
            bpf.Show(this); //the form is TopMost instead of modal because the thread needs to be free to notify the control where the drag originated that a drop has been made
        }

        private void dataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        public void RefreshData()
        {
            List<App> apps = api.Apps();
            
            //if we need to restrict the app set shown, we do so here
            if (restrictedAppSet != null)
                apps = apps.Where(a => restrictedAppSet.Contains(a.Name)).ToList();

            string[] selectedAppNames = GetSelectedAppNames();
            dataGrid.Rows.Clear();
            
            allServices = api.ProvisionedServices(); // read latest info
                        
            foreach (App app in apps)
            {
                DataGridViewRow existingRow = FindRowWithApp(app.Name);

                //if newly added, create
                if (existingRow == null)
                {
                    AddAppDataRow(app);
                }
                else
                {
                    UpdateAppDataRow(existingRow, app);
                }
            }

            //remove controls whose apps exist no more
            for (int i = dataGrid.Rows.Count - 1; i >= 0; i--)
            {
                App appOfRow = dataGrid.Rows[i].Tag as App;
                if (appOfRow == null || !apps.Any(a => a.Name == appOfRow.Name))
                {
                    dataGrid.Rows.RemoveAt(i);
                    continue;
                }

                dataGrid.Rows[i].Selected = selectedAppNames.Contains(appOfRow.Name);
            }

        }

        private void UpdateAppDataRow(DataGridViewRow row, App app)
        {
            App currentApp = row.Tag as App;
            if (app.HasSameMainValuesAs(currentApp)) return;

            FillRowWithValues(row, app);
        }

        private void FillRowWithValues(DataGridViewRow row, App app)
        {
            row.Tag = app;

            DataGridViewImageAndTextCell appName = row.Cells[(int)GridColumns.AppName] as DataGridViewImageAndTextCell;
            KeyValuePair<Image, string> kvp = new KeyValuePair<Image, string>(imageList.Images[Utils.GetAppStateImageIndex(app)], app.Name);
            appName.Value = new List<KeyValuePair<Image, string>>() { kvp };

            DataGridViewProgressCell health = row.Cells[(int)GridColumns.Health] as DataGridViewProgressCell;
            health.Value = app.RunningInstances;
            health.MaxValue = float.Parse(app.Instances);

            DataGridViewUriListCell urls = row.Cells[(int)GridColumns.URLs] as DataGridViewUriListCell;
            urls.Value = app.UriList;
            int urlsHeight = urls.GetContentHeight(app.UriList, dataGrid.Font);
            
            DataGridViewImageAndTextCell services = row.Cells[(int)GridColumns.Services] as DataGridViewImageAndTextCell;
            string[] appServices = app.ServiceNames;
            List<KeyValuePair<Image, string>> list = new List<KeyValuePair<Image, string>>();
            int neededHeight = 10; // a small offset
            foreach (string appService in appServices)
            {
                kvp = new KeyValuePair<Image, string>(GetServiceImage(appService), appService);
                neededHeight += Math.Max(kvp.Key.Height, dataGrid.Font.Height);
                list.Add(kvp);
            }

            services.Value = list;
            int servicesHeight = neededHeight;

            DataGridViewImageAndTextCell framework = row.Cells[(int)GridColumns.Framework] as DataGridViewImageAndTextCell;
            kvp = new KeyValuePair<Image, string>(imageList.Images[Utils.GetFrameworkImageIndex(app.StagingModel)], app.StagingModel);
            framework.Value = new List<KeyValuePair<Image, string>>() { kvp };

            DataGridViewImageAndTextCell runtime = row.Cells[(int)GridColumns.Runtime] as DataGridViewImageAndTextCell;
            kvp = new KeyValuePair<Image, string>(imageList.Images[Utils.GetRuntimeImageIndex(app.StagingStack)], app.StagingStack);
            runtime.Value = new List<KeyValuePair<Image, string>>() { kvp };

            row.Height = (new List<int>() { dataGrid.RowTemplate.Height, urlsHeight, servicesHeight }).Max();
        }

       
        private DataGridViewRow FindRowWithApp(string appName)
        {
            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                if ((row.Tag as App).Name == appName)
                    return row;
            }

            return null;
        }

        private void AddAppDataRow(App app)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.Cells.Add(new DataGridViewImageAndTextCell()); 
            row.Cells.Add(new DataGridViewProgressCell());
            row.Cells.Add(new DataGridViewUriListCell());
            row.Cells.Add(new DataGridViewImageAndTextCell());
            row.Cells.Add(new DataGridViewImageAndTextCell());
            row.Cells.Add(new DataGridViewImageAndTextCell());
            

            FillRowWithValues(row, app);
            dataGrid.Rows.Add(row);

        }

        private Image GetServiceImage(string serviceName)
        {
            ProvisionedService service = allServices.Find(s => s.Name == serviceName);
            return imageList.Images[service == null ? (int)Utils.SmallImages.DefaultImage : Utils.GetServiceImageIndex(service.Vendor)];
        }

        public string[] GetSelectedAppNames()
        {
            if (dataGrid.SelectedRows.Count == 0) return new string[0];

            List<string> appNames = new List<string>();
            for (int i = 0; i < dataGrid.SelectedRows.Count; i++)
            {
                App app = dataGrid.SelectedRows[i].Tag as App;
                if (app!= null) appNames.Add(app.Name);
            }

            return appNames.ToArray();
        }

        private void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(sender, e);
        }

        private void dataGrid_MouseClick(object sender, MouseEventArgs e)
        {
            //if the right mouse button was clicked, show context menu
            if (e.Button == MouseButtons.Right)
            {
                if (contextMenuEnabled)
                {
                    //show the context menu of the control
                    contextMenu.Show(this.PointToScreen(e.Location));
                }
                else
                {
                    DataGridView.HitTestInfo info = dataGrid.HitTest(e.X, e.Y);

                    bool rightClickedOnSelection = (info != DataGridView.HitTestInfo.Nowhere && dataGrid.Rows[info.RowIndex].Selected); //have we clicked on a selected row or not?
                    
                    //let the host know about the right click
                    if (RightClick != null)
                        RightClick(sender, e, rightClickedOnSelection);
                }
            }

        }

        #region "VS extension only"


        public void BindToNewSession(CloudTarget target)
        {
            api = new Client();
            api.Target(target.TargetUrl);
            bool ret = api.Login(target.Username, CloudCredentialsEncryption.GetUnsecureString(target.Password));

            if (ret == true)
            {
                this.RefreshData();
            }
        }

        public void ShowOnlyTheseApps(string[] deployableAppNames)
        {
            restrictedAppSet = deployableAppNames;
            RefreshData();
        }

        public void ShowAllApps()
        {
            restrictedAppSet = null; //clear restrictions
            RefreshData();
        }

        #endregion


        private void startApps_Click(object sender, EventArgs e)
        {
            StartSelectedApps();
        }

        private void StartSelectedApps()
        {
            try
            {
                this.SelectedApps.ForEach(app => api.StartApp(app.Name, false));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); //this should do until a better way to show errors is implemented
            }
        }

        private void stopApps_Click(object sender, EventArgs e)
        {
            StopSelectedApps();
        }

        private void StopSelectedApps()
        {
            try
            {
                this.SelectedApps.ForEach(app => api.StopApp(app.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); //this should do until a better way to show errors is implemented
            }
        }

        private void browseApps_Click(object sender, EventArgs e)
        {
            BrowseSelectedApps();
        }

        private void BrowseSelectedApps()
        {
            this.SelectedApps.ForEach(app =>
            {
                string[] urls = app.UriList;
                foreach (string url in urls) Utils.OpenLink(url);
            });
        }

        private void removeApps_Click(object sender, EventArgs e)
        {
            RemoveSelectedApps();
        }

        private void RemoveSelectedApps()
        {
            try
            {
                this.SelectedApps.ForEach(app => api.DeleteApp(app.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); //this should do until a better way to show errors is implemented
            }
        }
    }
}
