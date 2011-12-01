namespace Uhuru.CloudFoundry.UI.VS2010.Dialogs
{
    partial class TargetManagerDialog
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
            this.btnAddTarget = new System.Windows.Forms.Button();
            this.btnRemoveTarget = new System.Windows.Forms.Button();
            this.listBoxTargets = new System.Windows.Forms.ListBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnAddTarget
            // 
            this.btnAddTarget.BackColor = System.Drawing.Color.White;
            this.btnAddTarget.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddTarget.Location = new System.Drawing.Point(12, 230);
            this.btnAddTarget.Name = "btnAddTarget";
            this.btnAddTarget.Size = new System.Drawing.Size(87, 23);
            this.btnAddTarget.TabIndex = 12;
            this.btnAddTarget.Text = "Add";
            this.btnAddTarget.UseVisualStyleBackColor = true;
            this.btnAddTarget.Click += new System.EventHandler(this.btnAddTarget_Click);
            // 
            // btnRemoveTarget
            // 
            this.btnRemoveTarget.BackColor = System.Drawing.Color.White;
            this.btnRemoveTarget.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveTarget.Location = new System.Drawing.Point(105, 230);
            this.btnRemoveTarget.Name = "btnRemoveTarget";
            this.btnRemoveTarget.Size = new System.Drawing.Size(87, 23);
            this.btnRemoveTarget.TabIndex = 21;
            this.btnRemoveTarget.Text = "Remove Target";
            this.btnRemoveTarget.UseVisualStyleBackColor = true;
            this.btnRemoveTarget.Click += new System.EventHandler(this.btnRemoveTarget_Click);
            // 
            // listBoxTargets
            // 
            this.listBoxTargets.FormattingEnabled = true;
            this.listBoxTargets.Location = new System.Drawing.Point(12, 12);
            this.listBoxTargets.Name = "listBoxTargets";
            this.listBoxTargets.Size = new System.Drawing.Size(268, 212);
            this.listBoxTargets.TabIndex = 25;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.White;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(198, 230);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(82, 23);
            this.btnClose.TabIndex = 24;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // TargetManagerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 263);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.listBoxTargets);
            this.Controls.Add(this.btnRemoveTarget);
            this.Controls.Add(this.btnAddTarget);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "TargetManagerDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Target Manager";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAddTarget;
        private System.Windows.Forms.Button btnRemoveTarget;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ListBox listBoxTargets;
    }
}