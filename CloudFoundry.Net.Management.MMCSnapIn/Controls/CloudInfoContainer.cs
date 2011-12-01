using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.ManagementConsole;
using CloudFoundry.Net.Management.MMCSnapIn.FormViews;

namespace CloudFoundry.Net.Management.MMCSnapIn.Controls
{
    public partial class CloudInfoContainer : UserControl, IFormViewControl
    {
        CloudInfoFormView parentFormView = null;
        
        public CloudInfoContainer()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            Assembly asm = Assembly.GetExecutingAssembly();
            listViewFrameworks.SmallImageList = new ImageList();
            listViewFrameworks.SmallImageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("CloudFoundry.Net.Management.MMCSnapIn.Images.smallicons.bmp")));

            listViewRuntimes.SmallImageList = new ImageList();
            listViewRuntimes.SmallImageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("CloudFoundry.Net.Management.MMCSnapIn.Images.smallicons.bmp")));

            listViewServices.SmallImageList = new ImageList();
            listViewServices.SmallImageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("CloudFoundry.Net.Management.MMCSnapIn.Images.smallicons.bmp")));
        }


        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="parentSelectionFormView"></param>
        void IFormViewControl.Initialize(FormView parentSelectionFormView)
        {
            parentFormView = (CloudInfoFormView)parentSelectionFormView;
        }

        internal void Refresh(Client api)
        {
            RefreshFrameworks(api.Frameworks());
            RefreshRuntimes(api.Runtimes());
            RefreshServices(api.Services());
        }

        private void RefreshServices(List<Service> services)
        {
            listViewServices.Items.Clear();

            foreach (Service service in services)
            {
                ListViewItem rNode = new ListViewItem();
                rNode.Text = service.Vendor;
                rNode.SubItems.Add(service.Description);
                rNode.SubItems.Add(service.Version);
                rNode.ImageIndex = Utils.GetServiceImageIndex(service.Vendor.Trim().ToLower());
              
                listViewServices.Items.Add(rNode);
            }
        }

        private void RefreshRuntimes(List<Runtime> runtimes)
        {
            listViewRuntimes.Items.Clear();

            foreach (Runtime runtime in runtimes)
            {
                ListViewItem item = new ListViewItem();
                item.Text = runtime.Name;
                item.SubItems.Add(runtime.Description);
                item.SubItems.Add(runtime.Version);
                item.ImageIndex = Utils.GetRuntimeImageIndex(runtime.Name.Trim().ToLower());

                listViewRuntimes.Items.Add(item);
            }
        }

        private void RefreshFrameworks(List<Framework> frameworks)
        {
            listViewFrameworks.Items.Clear();
            
            foreach (Framework framework in frameworks)
            {
                ListViewItem item = new ListViewItem();
                item.Text = framework.Name;
                item.ImageIndex = Utils.GetFrameworkImageIndex(framework.Name.Trim().ToLower());
                listViewFrameworks.Items.Add(item);
            }
        }
    }
}
