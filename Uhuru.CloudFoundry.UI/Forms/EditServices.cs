using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CloudFoundry.Net;
using System.Reflection;

namespace Uhuru.CloudFoundry.UI.Forms
{
    public partial class EditServices : Form
    {
        private Client api = null;
        private App app = null;
        private List<ProvisionedService> allServices = null;
        private string[] appServiceNames = new string[] {};

        public App App
        {
            get { return app; }
            set { 
                    app = value;
                    statusLabel.Text += app.Name;
                    appServiceNames = app.ServiceNames;
                }
        }

        public Client Api
        {
            get { return api; }
            set { 
                    api = value;
                    allServices = api.ProvisionedServices();
                }
        }

        private void EditServices_Load(object sender, EventArgs e)
        {
            FillServicesLists();
        }

        private void FillServicesLists()
        {
            if (api == null || app == null) return;
                        
            //first we add the app services
            foreach (string appServiceName in appServiceNames)
            {
                ProvisionedService service = allServices.Where(s => s.Name == appServiceName).FirstOrDefault();
                if (service == null) continue;

                listViewAppServices.Items.Add(new ListViewItem(new string[] { service.Name }, Utils.GetServiceImageIndex(service.Vendor)));
            }

            RefreshAllServicesList();

        }

        private void RefreshAllServicesList()
        {
            listViewAllServices.Items.Clear();
            allServices = api.ProvisionedServices();

            allServices.ForEach(service =>
            {
                if (!appServiceNames.Contains(service.Name)) //we only want to show the services that are not already allotted to the current app
                    listViewAllServices.Items.Add(new ListViewItem(new string[] { service.Name }, Utils.GetServiceImageIndex(service.Vendor)));
            });
        }
                
        public EditServices()
        {
            InitializeComponent();

            Assembly asm = Assembly.GetExecutingAssembly();
            listViewAllServices.SmallImageList = new ImageList();
            listViewAllServices.SmallImageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("Uhuru.CloudFoundry.UI.Images.smallicons.bmp")));

            listViewAppServices.SmallImageList = new ImageList();
            listViewAppServices.SmallImageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("Uhuru.CloudFoundry.UI.Images.smallicons.bmp")));

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            MoveSelectedItems(listViewAllServices, listViewAppServices);
        }

        private void MoveSelectedItems(ListView source, ListView destination)
        {
            for (int i = source.SelectedItems.Count - 1; i >= 0; i--)
            {
                ListViewItem item =  source.SelectedItems[i];
                
                destination.Items.Add(new ListViewItem(new string[] { item.Text }, item.ImageIndex));
                source.Items.Remove(item);
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            MoveSelectedItems(listViewAppServices, listViewAllServices);
        }

        private void linkAddNewService_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AddServiceForm serviceForm = new AddServiceForm(api);
            if (serviceForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                RefreshAllServicesList();
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            //first stop: check the appServices ListView
            foreach (ListViewItem item in listViewAppServices.Items)
            {
                //if not in the original appServicesList, bind it
                if (!appServiceNames.Contains(item.Text))
                    api.BindService(app.Name, item.Text);
            }

            //and now the reverse operation, find the services that were in the initial list but are no longer in the appServices ListView
            foreach (string appServiceName in appServiceNames)
            {
                ListViewItem item = listViewAppServices.FindItemWithText(appServiceName);
                if (item == null)
                    api.UnbindService(app.Name, appServiceName);
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

       
    }
}
