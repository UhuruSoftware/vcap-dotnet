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
    public partial class AppPushControl : UserControl
    {
      private Client cfClient = null;
      private string directory = String.Empty;

      public AppPushControl(Client cfClient, string directory = "")
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

        private void RefreshFrameworksList()
        {
            imageComboBoxFrameworks.Items.Clear();
            List<Framework> frameworks = cfClient.Frameworks();
            foreach (Framework framework in frameworks)
            {
                imageComboBoxFrameworks.Items.Add(new ImageComboBoxItem(framework.Name, Utils.GetFrameworkImageIndex(framework.Name)));
            }
        }

        private void RefreshRuntimesList()
        {
            imageComboBoxRuntimes.Items.Clear();
            List<Runtime> runtimes = cfClient.Runtimes();
            foreach (Runtime runtime in runtimes)
            {
                imageComboBoxRuntimes.Items.Add(new ImageComboBoxItem(runtime.Name, Utils.GetRuntimeImageIndex(runtime.Name)));
            }
        }


        private void buttonBrowsePath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Open a folder which contains your app."; 
                dialog.ShowNewFolderButton = false; 
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string folder = dialog.SelectedPath;
                    textBoxPath.Text = folder;
                    if (Directory.Exists(folder))
                    {
                        string framework = Utils.GetFramework(folder);
                        if (framework != String.Empty)
                        {
                            ImageComboBoxItem selItem = imageComboBoxFrameworks.Items.Cast<ImageComboBoxItem>().FirstOrDefault(item => item.Text == framework);
                            if (selItem != null)
                            {
                                imageComboBoxFrameworks.SelectedItem = selItem;
                            }
                        }

                        if (textBoxAppName.Text == String.Empty)
                        {
                            string[] pieces = folder.Split('\\');
                            textBoxAppName.Text = pieces[pieces.Length - 1].ToLower();
                        }
                    }
                }
            }
        }

        private void imageComboBoxFrameworks_SelectedIndexChanged(object sender, EventArgs e)
        {
            Framework framework = cfClient.Frameworks().FirstOrDefault(row => row.Name == ((ImageComboBoxItem)imageComboBoxFrameworks.SelectedItem).Text);
            if (framework != null)
            {
                if (framework.Runtimes.Count > 0)
                {
                    ImageComboBoxItem item = imageComboBoxRuntimes.Items.Cast<ImageComboBoxItem>().FirstOrDefault(row => row.Text == framework.Runtimes[0].Name);
                    if (item != null)
                    {
                        imageComboBoxRuntimes.SelectedItem = item;
                    }
                }
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

        private void buttonGetFramework_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxPath.Text))
            {
                string framework = Utils.GetFramework(textBoxPath.Text);
                if (framework != String.Empty)
                {
                    ImageComboBoxItem selItem = imageComboBoxFrameworks.Items.Cast<ImageComboBoxItem>().FirstOrDefault(item => item.Text == framework);
                    if (selItem != null)
                    {
                        imageComboBoxFrameworks.SelectedItem = selItem;
                    }
                }
            }
        }

        public void Push()
        {
            cfClient.Push(textBoxAppName.Text,
                textBoxAppUrl.Text,
                textBoxPath.Text,
                Convert.ToInt32(numericUpDownInstances.Value),
                ((ImageComboBoxItem)imageComboBoxFrameworks.SelectedItem).Text,
                ((ImageComboBoxItem)imageComboBoxRuntimes.SelectedItem).Text,
                Convert.ToInt32(numericUpDownMemory.Value),
                listViewServices.SelectedItems.Cast<ListViewItem>().Select(item => item.Text).ToList(),
                false, false, true);
        }


        private void AppPushControl_Load(object sender, EventArgs e)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            listViewServices.SmallImageList = new ImageList();
            listViewServices.SmallImageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("Uhuru.CloudFoundry.UI.Images.smallicons.bmp")));

            imageComboBoxFrameworks.ImageList = new ImageList();
            imageComboBoxFrameworks.ImageList.TransparentColor = Color.White;
            imageComboBoxFrameworks.ImageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("Uhuru.CloudFoundry.UI.Images.smallicons.bmp")));

            imageComboBoxRuntimes.ImageList = new ImageList();
            imageComboBoxRuntimes.ImageList.TransparentColor = Color.White;
            imageComboBoxRuntimes.ImageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("Uhuru.CloudFoundry.UI.Images.smallicons.bmp")));

            RefreshServicesList();
            RefreshFrameworksList();
            RefreshRuntimesList();

            if (directory != "")
            {
                textBoxPath.Text = directory;
                buttonGetFramework_Click(null, null);

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
