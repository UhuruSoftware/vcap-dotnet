namespace Uhuru.CloudFoundry.UI.Forms
{
    partial class EditServices
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
            this.linkAddNewService = new System.Windows.Forms.LinkLabel();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.listViewAllServices = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewAppServices = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // linkAddNewService
            // 
            this.linkAddNewService.AutoSize = true;
            this.linkAddNewService.Location = new System.Drawing.Point(66, 234);
            this.linkAddNewService.Name = "linkAddNewService";
            this.linkAddNewService.Size = new System.Drawing.Size(95, 13);
            this.linkAddNewService.TabIndex = 2;
            this.linkAddNewService.TabStop = true;
            this.linkAddNewService.Text = "Add new service...";
            this.linkAddNewService.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAddNewService_LinkClicked);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(167, 88);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(41, 23);
            this.buttonAdd.TabIndex = 3;
            this.buttonAdd.Text = ">>";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(167, 124);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(41, 23);
            this.buttonRemove.TabIndex = 4;
            this.buttonRemove.Text = "<<";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(214, 251);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(298, 251);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // listViewAllServices
            // 
            this.listViewAllServices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName});
            this.listViewAllServices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewAllServices.HideSelection = false;
            this.listViewAllServices.Location = new System.Drawing.Point(6, 10);
            this.listViewAllServices.Name = "listViewAllServices";
            this.listViewAllServices.Size = new System.Drawing.Size(155, 221);
            this.listViewAllServices.TabIndex = 19;
            this.listViewAllServices.UseCompatibleStateImageBehavior = false;
            this.listViewAllServices.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Existing services";
            this.columnHeaderName.Width = 232;
            // 
            // listViewAppServices
            // 
            this.listViewAppServices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewAppServices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewAppServices.HideSelection = false;
            this.listViewAppServices.Location = new System.Drawing.Point(214, 10);
            this.listViewAppServices.Name = "listViewAppServices";
            this.listViewAppServices.Size = new System.Drawing.Size(155, 221);
            this.listViewAppServices.TabIndex = 20;
            this.listViewAppServices.UseCompatibleStateImageBehavior = false;
            this.listViewAppServices.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "App services";
            this.columnHeader1.Width = 232;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 280);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(379, 22);
            this.statusStrip1.TabIndex = 21;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(76, 17);
            this.statusLabel.Text = "Current app: ";
            // 
            // EditServices
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 302);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.listViewAppServices);
            this.Controls.Add(this.listViewAllServices);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.linkAddNewService);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MinimumSize = new System.Drawing.Size(341, 309);
            this.Name = "EditServices";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add/Remove App Services";
            this.Load += new System.EventHandler(this.EditServices_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkAddNewService;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ListView listViewAllServices;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ListView listViewAppServices;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}