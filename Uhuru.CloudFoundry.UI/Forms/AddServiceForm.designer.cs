namespace Uhuru.CloudFoundry.UI
{
    partial class AddServiceForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddServiceForm));
            this.buttonAddService = new System.Windows.Forms.Button();
            this.imageComboBoxFrameworks = new ImageComboBox();
            this.labelAppName = new System.Windows.Forms.Label();
            this.textBoxAppName = new System.Windows.Forms.TextBox();
            this.labelFramework = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonAddService
            // 
            this.buttonAddService.Location = new System.Drawing.Point(143, 68);
            this.buttonAddService.Name = "buttonAddService";
            this.buttonAddService.Size = new System.Drawing.Size(75, 23);
            this.buttonAddService.TabIndex = 17;
            this.buttonAddService.Text = "Create";
            this.buttonAddService.UseVisualStyleBackColor = true;
            this.buttonAddService.Click += new System.EventHandler(this.buttonAddService_Click);
            // 
            // imageComboBoxFrameworks
            // 
            this.imageComboBoxFrameworks.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.imageComboBoxFrameworks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imageComboBoxFrameworks.FormattingEnabled = true;
            this.imageComboBoxFrameworks.ImageList = null;
            this.imageComboBoxFrameworks.Location = new System.Drawing.Point(89, 38);
            this.imageComboBoxFrameworks.Name = "imageComboBoxFrameworks";
            this.imageComboBoxFrameworks.Size = new System.Drawing.Size(129, 21);
            this.imageComboBoxFrameworks.TabIndex = 30;
            // 
            // labelAppName
            // 
            this.labelAppName.AutoSize = true;
            this.labelAppName.Location = new System.Drawing.Point(9, 15);
            this.labelAppName.Name = "labelAppName";
            this.labelAppName.Size = new System.Drawing.Size(74, 13);
            this.labelAppName.TabIndex = 29;
            this.labelAppName.Text = "Service Name";
            // 
            // textBoxAppName
            // 
            this.textBoxAppName.Location = new System.Drawing.Point(89, 12);
            this.textBoxAppName.Name = "textBoxAppName";
            this.textBoxAppName.Size = new System.Drawing.Size(129, 20);
            this.textBoxAppName.TabIndex = 28;
            // 
            // labelFramework
            // 
            this.labelFramework.AutoSize = true;
            this.labelFramework.Location = new System.Drawing.Point(9, 41);
            this.labelFramework.Name = "labelFramework";
            this.labelFramework.Size = new System.Drawing.Size(70, 13);
            this.labelFramework.TabIndex = 27;
            this.labelFramework.Text = "Service Type";
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(62, 68);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 31;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // AddServiceForm
            // 
            this.AcceptButton = this.buttonAddService;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(232, 103);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.imageComboBoxFrameworks);
            this.Controls.Add(this.buttonAddService);
            this.Controls.Add(this.labelAppName);
            this.Controls.Add(this.textBoxAppName);
            this.Controls.Add(this.labelFramework);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AddServiceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Provision Service";
            this.Load += new System.EventHandler(this.PushAppForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonAddService;
        private ImageComboBox imageComboBoxFrameworks;
        private System.Windows.Forms.Label labelAppName;
        private System.Windows.Forms.TextBox textBoxAppName;
        private System.Windows.Forms.Label labelFramework;
        private System.Windows.Forms.Button buttonCancel;
    }
}