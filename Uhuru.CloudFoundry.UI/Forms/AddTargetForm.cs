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
    public partial class AddTargetForm : Form
    {
        public string Target = String.Empty;
        public string Email = String.Empty;
        public string Password = String.Empty;

        public AddTargetForm()
        {
            InitializeComponent();
        }

        private void AddTargetForm_Load(object sender, EventArgs e)
        {

        }

        private bool VerifySettings(bool showSuccessDialog)
        {
            Client api = new Client();
            if (!api.Target(textBoxTargetURL.Text))
            {
                MessageBox.Show("Cannot target: " + textBoxTargetURL.Text, "CF Snap-in", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                if (!api.Login(textBoxEmail.Text, textBoxPassword.Text))
                {
                    MessageBox.Show("Cannot login to target using the specified credentials!");
                    return false;
                }
                else
                {
                    if (showSuccessDialog)
                    {
                        MessageBox.Show("Login successful to " + textBoxTargetURL.Text, "CF Snap-in", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            return true;
        }

        private void buttonVerify_Click(object sender, EventArgs e)
        {
            VerifySettings(true);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (VerifySettings(false))
            {
                this.Target = textBoxTargetURL.Text;
                this.Email = textBoxEmail.Text;
                this.Password = textBoxPassword.Text;
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
