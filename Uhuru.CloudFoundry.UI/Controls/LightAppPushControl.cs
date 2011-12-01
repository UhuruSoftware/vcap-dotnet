using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using CloudFoundry.Net;
using System.IO;

namespace Uhuru.CloudFoundry.UI
{
    public partial class LightAppPushControl : UserControl
    {
      private Client cfClient = null;
      private string directory = String.Empty;

      public LightAppPushControl(Client cfClient, string directory = "")
      {
          this.cfClient = cfClient;
          this.directory = directory;
          cfClient.UpdatePushStatus += new EventHandler<PushStatusEventArgs>(cfClient_UpdatePushStatus);
          InitializeComponent();
          progressBar.Maximum = 7;
      }

      public bool HasCompleted
      {
          get { return progressBar.Value == progressBar.Maximum; }
      }

      void cfClient_UpdatePushStatus(object sender, PushStatusEventArgs e)
      {
          if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate()
                    {
                        UpdateProgress(e);
                    }));
            }
            else
            {
                UpdateProgress(e);
            }
        }

        private void UpdateProgress(PushStatusEventArgs e)
        {
            progressBar.Value = (int)e.Status + 1;
            switch (e.Status)
            {
                case PushStatus.CHECKING: labelStatus.Text = "Checking app..."; break;
                case PushStatus.BINDING: labelStatus.Text = "Binding services to app..."; break;
                case PushStatus.CREATING: labelStatus.Text = "Creating app..."; break;
                case PushStatus.PACKAGING: labelStatus.Text = "Packaging app resources..."; break;
                case PushStatus.STAGING: labelStatus.Text = "Staging app..."; break;
                case PushStatus.STARTING: labelStatus.Text = "Done (app is starting)"; break;
                case PushStatus.UPLOADING: labelStatus.Text = "Uploading app..."; break;
            }

            Application.DoEvents();
        }


        private void RefreshServicesList()
        {
            listViewServices.Items.Clear();
            List<ProvisionedService> services = cfClient.ProvisionedServices();
            foreach (ProvisionedService service in services)
            {
                ListViewItem item = new ListViewItem(new string[] { service.Name }, Utils.GetServiceImageIndex(service.Vendor));
                listViewServices.Items.Add(item);
            }
        }

        private void textBoxAppName_TextChanged(object sender, EventArgs e)
        {
            if ((string)textBoxAppUrl.Tag == "auto")
            {
                textBoxAppUrl.Text = Utils.GetDefaultUrlForApp(textBoxAppName.Text, cfClient.TargetUrl);
            }
        }

        private void textBoxAppUrl_TextChanged(object sender, EventArgs e)
        {
            if (textBoxAppUrl.Text == Utils.GetDefaultUrlForApp(textBoxAppName.Text, cfClient.TargetUrl) || textBoxAppUrl.Text == String.Empty)
            {
                textBoxAppUrl.Tag = "auto";
            }
            else
            {
                textBoxAppUrl.Tag = "";
            }
        }

        public void Push(string path, string framework, string runtime)
        {
            cfClient.Push(textBoxAppName.Text,
                textBoxAppUrl.Text,
                path,
                Convert.ToInt32(numericUpDownInstances.Value),
                framework,
                runtime,
                Convert.ToInt32(numericUpDownMemory.Value),
                listViewServices.SelectedItems.Cast<ListViewItem>().Select(item => item.Text).ToList(),
                false, false, true);
        }


        private void AppPushControl_Load(object sender, EventArgs e)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            listViewServices.SmallImageList = new ImageList();
            listViewServices.SmallImageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("Uhuru.CloudFoundry.UI.Images.smallicons.bmp")));

            RefreshServicesList();

            if (directory != "")
            {
                if (textBoxAppName.Text == String.Empty)
                {
                    string[] pieces = directory.Split('\\');
                    textBoxAppName.Text = pieces[pieces.Length - 1].ToLower();
                }
            }
        }

        private void buttonAddService_Click(object sender, EventArgs e)
        {
            AddServiceForm serviceForm = new AddServiceForm(cfClient);
            if (serviceForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                RefreshServicesList();
            }
        }
    }
}
