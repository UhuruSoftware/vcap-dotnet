using System;
using Microsoft.ManagementConsole;
using Uhuru.CloudFoundry.UI;
using Uhuru.CloudFoundry.UI.Forms;

namespace CloudFoundry.Net.Management.MMCSnapIn.ScopeNodes
{
    class TargetsScopeNode:ScopeNode
    {
        CloudTargetManager manager = new CloudTargetManager();

        public TargetsScopeNode()
        {
            this.DisplayName = "Targets";
            this.ImageIndex = 18;
            this.SelectedImageIndex = 18;
            Microsoft.ManagementConsole.Action action = new Microsoft.ManagementConsole.Action("Add target...", "Add a new Cloud Foundry target.", 0, "AddTarget");
            this.ActionsPaneItems.Add(action);
            Refresh();            
        }

        void Refresh()
        {
            this.Children.Clear();

            CloudTarget[] targets = manager.GetTargets();

            foreach (CloudTarget target in targets)
            {
                TargetScopeNode node = new TargetScopeNode(target);
                node.DisplayName = target.TargetUrl + " (" + target.Username + ")";
                node.NodeAsksForRemoval += new Action<TargetScopeNode>(node_NodeAsksForRemoval);

                this.Children.Add(node);
            }
        }

        void node_NodeAsksForRemoval(TargetScopeNode node)
        {
            this.Children.Remove(node);
        }

        private void AddTarget(string target, string email, string password)
        {
            CloudTarget cTarget = new CloudTarget(email,
                                        CloudCredentialsEncryption.GetSecureString(password), target);

            manager.SaveTarget(cTarget);
        }

        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "AddTarget":
                    {
                        AddTargetForm addTargetForm = new AddTargetForm();
                        if (this.SnapIn.Console.ShowDialog(addTargetForm) == System.Windows.Forms.DialogResult.OK)
                        {
                            string target = addTargetForm.Target;
                            string email = addTargetForm.Email;
                            string password = addTargetForm.Password;
                            AddTarget(target, email, password);
                            Refresh();
                        }
                        break;
                    }
            }
        }



    }
}
