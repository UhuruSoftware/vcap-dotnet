using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CloudFoundry.Net;

namespace Uhuru.CloudFoundry.UI.Forms
{
    public partial class EditURLs : Form
    {
        public EditURLs()
        {
            InitializeComponent();
        }

        private Client api = null;
        private App app = null;
        private string[] appUris = new string[] { };

        public App App
        {
            get { return app; }
            set
            {
                app = value;
                statusLabel.Text += app.Name;
                appUris = app.UriList;
                listBox.Items.AddRange(appUris);
            }
        }

        public Client Api
        {
            get { return api; }
            set
            {
                api = value;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonAddNew_Click(object sender, EventArgs e)
        {
            listBox.Items.Add(textBoxNewUri.Text);
            textBoxNewUri.Text = string.Empty;
        }

        private void listBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            { 
                //remove selected values
                for (int i = listBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    object item = listBox.SelectedItems[i];
                    listBox.Items.Remove(item);
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
           
            foreach (object item in listBox.Items)
            {
                try
                {
                    if (!appUris.Contains(item.ToString()))
                    {
                        api.MapUri(app.Name, item.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error when mapping {0}: {1}", item.ToString(), ex.Message));
                }
            }
            
                        
            foreach (string appUri in appUris)
            {
                int itemIndex = listBox.FindStringExact(appUri);
                if (itemIndex < 0)
                    {
                        api.UnmapUri(app.Name, appUri);
                    }
            }
            
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        
    }
}
