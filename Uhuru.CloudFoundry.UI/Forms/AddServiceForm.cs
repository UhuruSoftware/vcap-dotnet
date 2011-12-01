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
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using CloudFoundry.Net;

namespace Uhuru.CloudFoundry.UI
{
    public partial class AddServiceForm : Form
    {
        private Client cfClient = null;

        public AddServiceForm(Client cfClient)
        {
            this.cfClient = cfClient;
            InitializeComponent();
        }

        private void PushAppForm_Load(object sender, EventArgs e)
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            imageComboBoxFrameworks.ImageList = new ImageList();
            imageComboBoxFrameworks.ImageList.TransparentColor = Color.White;
            imageComboBoxFrameworks.ImageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("Uhuru.CloudFoundry.UI.Images.smallicons.bmp")));

            RefreshServicesList();
        }


        private void RefreshServicesList()
        {
            imageComboBoxFrameworks.Items.Clear();
            List<Service> services = cfClient.Services();
            foreach (Service service in services)
            {
                ImageComboBoxItem item = new ImageComboBoxItem(service.Vendor, Utils.GetServiceImageIndex(service.Vendor));
                imageComboBoxFrameworks.Items.Add(item);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void buttonAddService_Click(object sender, EventArgs e)
        {
            cfClient.CreateService(textBoxAppName.Text, ((ImageComboBoxItem)imageComboBoxFrameworks.SelectedItem).Text);
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}