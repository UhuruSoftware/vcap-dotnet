using System.Security.Permissions;
using System.Windows.Forms;
using CloudFoundry.Net;
using System;
using Uhuru.CloudFoundry.UI.VS2010.Dialogs;
using System.Windows.Media;
using Uhuru.CloudFoundry.UI.VS2010.Extensions;
using System.Linq;

namespace Uhuru.CloudFoundry.UI.VS2010
{
    /// <summary>
    /// Summary description for ToolWindowCloudManagerControl.
    /// </summary>
    public partial class ToolWindowCloudManagerControl : UserControl
    {
        private CloudTarget activeTarget = null;
        
        
        public ToolWindowCloudManagerControl()
        {
            InitializeComponent();
            RefreshTargets();
            appsDataGrid.Font = new System.Drawing.Font("Segoe UI", 9f); // new System.Drawing.Font("Microsoft Sans Serif", 8.25f); 
        }

        /// <summary> 
        /// Let this control process the mnemonics.
        /// </summary>
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogChar(char charCode)
        {
              // If we're the top-level form or control, we need to do the mnemonic handling
              if (charCode != ' ' && ProcessMnemonic(charCode))
              {
                    return true;
              }
              return base.ProcessDialogChar(charCode);
        }

        /// <summary>
        /// Enable the IME status handling for this control.
        /// </summary>
        protected override bool CanEnableIme
        {
            get
            {
                return true;
            }
        }

        private void displayStatusError(string message)
        {
            toolStripStatusLabel.ForeColor = System.Drawing.Color.DarkRed;
            toolStripStatusLabel.Text = message;
        }

        private void displayStatusInfo(string message)
        {
            toolStripStatusLabel.ForeColor = System.Drawing.Color.ForestGreen;
            toolStripStatusLabel.Text = message;
        }

        private void comboBoxTargets_SelectedIndexChanged(object sender, EventArgs e)
        {
            activeTarget = (CloudTarget)comboBoxTargets.SelectedItem;
            appsDataGrid.BindToNewSession(activeTarget);

            Client api = new Client();
            api.Target(activeTarget.TargetUrl);
            bool ret = api.Login(activeTarget.Username, CloudCredentialsEncryption.GetUnsecureString(activeTarget.Password));

            if (ret == true)
            {
                IntegrationCenter.CloudClient = api;
            }
        }

        public void RefreshTargets()
        {
            CloudTarget[] targets = IntegrationCenter.GetCloudTargetManager().GetTargets();

            comboBoxTargets.DataSource = targets;
            comboBoxTargets.DisplayMember = "DisplayName";
            comboBoxTargets.ValueMember = "TargetId";
        }

        private void buttonTargetManager_Click(object sender, EventArgs e)
        {
            TargetManagerDialog targetManagerDlg = new TargetManagerDialog();
            targetManagerDlg.ShowDialog();
            RefreshTargets();
        }

        private void checkBoxShowProjectsInSolution_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowProjectsInSolution.Checked)
            {
                string[] deployableAppNames = IntegrationCenter.DeployableApplications.Select(da => da.Name).ToArray();
                appsDataGrid.ShowOnlyTheseApps(deployableAppNames);
            }
            else //show all
                appsDataGrid.ShowAllApps();
        }
    }
}