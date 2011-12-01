using System;
using System.Windows.Forms;
using Uhuru.CloudFoundry.UI.Forms;

namespace Uhuru.CloudFoundry.UI.VS2010.Dialogs
{
    public partial class TargetManagerDialog : Form
    {

        public TargetManagerDialog()
        {
            InitializeComponent();

            bindTargetList();
        }

        private void bindTargetList()
        {
            CloudTarget[] targets = IntegrationCenter.GetCloudTargetManager().GetTargets();

            listBoxTargets.DataSource = targets;
            listBoxTargets.DisplayMember = "DisplayName";
            listBoxTargets.ValueMember = "TargetId";
        }
        
        private void btnAddTarget_Click(object sender, EventArgs e)
        {
            AddTargetForm addTarget = new AddTargetForm();

            DialogResult res = addTarget.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                CloudTargetManager targetManagerInstance = IntegrationCenter.GetCloudTargetManager();
                CloudTarget target = new CloudTarget(addTarget.Email, CloudCredentialsEncryption.GetSecureString(addTarget.Password), addTarget.Target);
                targetManagerInstance.SaveTarget(target);
            }
            refreshTargets();
        }

        private void refreshTargets()
        {
            bindTargetList();
        }
     

        private void btnRemoveTarget_Click(object sender, EventArgs e)
        {
            CloudTarget selectedTarget = (CloudTarget)listBoxTargets.SelectedItem;

            IntegrationCenter.GetCloudTargetManager().RemoveTarget(selectedTarget);

            refreshTargets();
            listBoxTargets.SetSelected(listBoxTargets.Items.Count - 1, true);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listViewTargets_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
