using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using CloudFoundry.Net;

namespace Uhuru.CloudFoundry.UI
{
    public partial class BulkPushForm : Form
    {
        private Client cfClient;

        public BulkPushForm(Client cfClient)
        {
            this.cfClient = cfClient;
            InitializeComponent();
        }

        private void BulkPush_Load(object sender, EventArgs e)
        {
         
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonPush_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
            
            foreach (Control control in flowLayoutPanel.Controls)
            {
                if (control is AppPushControl)
                {
                    ThreadPool.QueueUserWorkItem(delegate(object data)
                    {
                        try
                        {
                            ((AppPushControl)data).Push();
                        }
                        catch (Exception ex)
                        {
                            ShowError(ex.Message); //this should do until a better error logging system is implemented
                        }
                    }, control);
                }
            }
           
        }

        private void ShowError(string error)
        {
            if (labelInfo.InvokeRequired)
            {
                labelInfo.Invoke(new MethodInvoker(delegate
                {
                    labelInfo.ForeColor = Color.Red;
                    labelInfo.Text = error;
                }));
            }
            else
            {
                labelInfo.ForeColor = Color.Red;
                labelInfo.Text = error;
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            AppPushControl newPushControl = new AppPushControl(cfClient.Clone(), "");
            newPushControl.Parent = flowLayoutPanel;
            newPushControl.Show();
        }

        private void flowLayoutPanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] directories = (string[])e.Data.GetData(DataFormats.FileDrop);
            LoadFromDirectories(directories);
        }

        public void LoadFromDirectories(string[] directories)
        {
            foreach (string directory in directories)
            {
                if (Directory.Exists(directory))
                {
                    AppPushControl newPushControl = new AppPushControl(cfClient.Clone(), directory);
                    newPushControl.Parent = flowLayoutPanel;
                    newPushControl.Show();
                }
            }
        }

        private void flowLayoutPanel_DragEnter(object sender, DragEventArgs e)
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

        private void timer_Tick(object sender, EventArgs e)
        {
            AppPushControl[] controls = flowLayoutPanel.Controls.OfType<AppPushControl>().ToArray();
            foreach (AppPushControl ctrl in controls)
            {
                if (!ctrl.HasCompleted) return; //wait until the next tick
            }

            this.Close(); //reaching this far means everything has completed
        }
    }
}
