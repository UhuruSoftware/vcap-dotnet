using Uhuru.CloudFoundry.UI;
namespace CloudFoundry.Net.Management.MMCSnapIn.Forms
{
    partial class PushAppForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PushAppForm));
            this.buttonPush = new System.Windows.Forms.Button();
            this.groupBoxAppInfo = new System.Windows.Forms.GroupBox();
            this.labelInstances = new System.Windows.Forms.Label();
            this.numericUpDownInstances = new System.Windows.Forms.NumericUpDown();
            this.buttonGetFramework = new System.Windows.Forms.Button();
            this.imageComboBoxRuntimes = new Uhuru.CloudFoundry.UI.ImageComboBox();
            this.imageComboBoxFrameworks = new Uhuru.CloudFoundry.UI.ImageComboBox();
            this.labelMemory = new System.Windows.Forms.Label();
            this.numericUpDownMemory = new System.Windows.Forms.NumericUpDown();
            this.buttonBrowsePath = new System.Windows.Forms.Button();
            this.labelAppName = new System.Windows.Forms.Label();
            this.textBoxAppName = new System.Windows.Forms.TextBox();
            this.labelRuntime = new System.Windows.Forms.Label();
            this.labelFramework = new System.Windows.Forms.Label();
            this.labelPath = new System.Windows.Forms.Label();
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.labelAppUrl = new System.Windows.Forms.Label();
            this.textBoxAppUrl = new System.Windows.Forms.TextBox();
            this.groupBoxServices = new System.Windows.Forms.GroupBox();
            this.listViewServices = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonAddService = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBoxAppInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInstances)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMemory)).BeginInit();
            this.groupBoxServices.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonPush
            // 
            this.buttonPush.Location = new System.Drawing.Point(488, 251);
            this.buttonPush.Name = "buttonPush";
            this.buttonPush.Size = new System.Drawing.Size(75, 23);
            this.buttonPush.TabIndex = 4;
            this.buttonPush.Text = "Push";
            this.buttonPush.UseVisualStyleBackColor = true;
            this.buttonPush.Click += new System.EventHandler(this.buttonPush_Click);
            // 
            // groupBoxAppInfo
            // 
            this.groupBoxAppInfo.Controls.Add(this.labelInstances);
            this.groupBoxAppInfo.Controls.Add(this.numericUpDownInstances);
            this.groupBoxAppInfo.Controls.Add(this.buttonGetFramework);
            this.groupBoxAppInfo.Controls.Add(this.imageComboBoxRuntimes);
            this.groupBoxAppInfo.Controls.Add(this.imageComboBoxFrameworks);
            this.groupBoxAppInfo.Controls.Add(this.labelMemory);
            this.groupBoxAppInfo.Controls.Add(this.numericUpDownMemory);
            this.groupBoxAppInfo.Controls.Add(this.buttonBrowsePath);
            this.groupBoxAppInfo.Controls.Add(this.labelAppName);
            this.groupBoxAppInfo.Controls.Add(this.textBoxAppName);
            this.groupBoxAppInfo.Controls.Add(this.labelRuntime);
            this.groupBoxAppInfo.Controls.Add(this.labelFramework);
            this.groupBoxAppInfo.Controls.Add(this.labelPath);
            this.groupBoxAppInfo.Controls.Add(this.textBoxPath);
            this.groupBoxAppInfo.Controls.Add(this.labelAppUrl);
            this.groupBoxAppInfo.Controls.Add(this.textBoxAppUrl);
            this.groupBoxAppInfo.Location = new System.Drawing.Point(12, 13);
            this.groupBoxAppInfo.Name = "groupBoxAppInfo";
            this.groupBoxAppInfo.Size = new System.Drawing.Size(295, 232);
            this.groupBoxAppInfo.TabIndex = 15;
            this.groupBoxAppInfo.TabStop = false;
            this.groupBoxAppInfo.Text = "App Information";
            // 
            // labelInstances
            // 
            this.labelInstances.AutoSize = true;
            this.labelInstances.Location = new System.Drawing.Point(10, 162);
            this.labelInstances.Name = "labelInstances";
            this.labelInstances.Size = new System.Drawing.Size(53, 13);
            this.labelInstances.TabIndex = 30;
            this.labelInstances.Text = "Instances";
            // 
            // numericUpDownInstances
            // 
            this.numericUpDownInstances.Location = new System.Drawing.Point(81, 160);
            this.numericUpDownInstances.Name = "numericUpDownInstances";
            this.numericUpDownInstances.Size = new System.Drawing.Size(198, 20);
            this.numericUpDownInstances.TabIndex = 29;
            this.numericUpDownInstances.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonGetFramework
            // 
            this.buttonGetFramework.Location = new System.Drawing.Point(247, 76);
            this.buttonGetFramework.Name = "buttonGetFramework";
            this.buttonGetFramework.Size = new System.Drawing.Size(32, 23);
            this.buttonGetFramework.TabIndex = 28;
            this.buttonGetFramework.Text = "?";
            this.buttonGetFramework.UseVisualStyleBackColor = true;
            this.buttonGetFramework.Click += new System.EventHandler(this.buttonGetFramework_Click);
            // 
            // imageComboBoxRuntimes
            // 
            this.imageComboBoxRuntimes.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.imageComboBoxRuntimes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imageComboBoxRuntimes.FormattingEnabled = true;
            this.imageComboBoxRuntimes.ImageList = null;
            this.imageComboBoxRuntimes.Location = new System.Drawing.Point(81, 133);
            this.imageComboBoxRuntimes.Name = "imageComboBoxRuntimes";
            this.imageComboBoxRuntimes.Size = new System.Drawing.Size(198, 21);
            this.imageComboBoxRuntimes.TabIndex = 27;
            // 
            // imageComboBoxFrameworks
            // 
            this.imageComboBoxFrameworks.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.imageComboBoxFrameworks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imageComboBoxFrameworks.FormattingEnabled = true;
            this.imageComboBoxFrameworks.ImageList = null;
            this.imageComboBoxFrameworks.Location = new System.Drawing.Point(81, 105);
            this.imageComboBoxFrameworks.Name = "imageComboBoxFrameworks";
            this.imageComboBoxFrameworks.Size = new System.Drawing.Size(198, 21);
            this.imageComboBoxFrameworks.TabIndex = 26;
            this.imageComboBoxFrameworks.SelectedIndexChanged += new System.EventHandler(this.imageComboBoxFrameworks_SelectedIndexChanged);
            // 
            // labelMemory
            // 
            this.labelMemory.AutoSize = true;
            this.labelMemory.Location = new System.Drawing.Point(10, 188);
            this.labelMemory.Name = "labelMemory";
            this.labelMemory.Size = new System.Drawing.Size(69, 13);
            this.labelMemory.TabIndex = 25;
            this.labelMemory.Text = "Memory (MB)";
            // 
            // numericUpDownMemory
            // 
            this.numericUpDownMemory.Location = new System.Drawing.Point(81, 186);
            this.numericUpDownMemory.Name = "numericUpDownMemory";
            this.numericUpDownMemory.Size = new System.Drawing.Size(198, 20);
            this.numericUpDownMemory.TabIndex = 24;
            this.numericUpDownMemory.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // buttonBrowsePath
            // 
            this.buttonBrowsePath.Location = new System.Drawing.Point(214, 76);
            this.buttonBrowsePath.Name = "buttonBrowsePath";
            this.buttonBrowsePath.Size = new System.Drawing.Size(32, 23);
            this.buttonBrowsePath.TabIndex = 18;
            this.buttonBrowsePath.Text = "...";
            this.buttonBrowsePath.UseVisualStyleBackColor = true;
            this.buttonBrowsePath.Click += new System.EventHandler(this.buttonBrowsePath_Click);
            // 
            // labelAppName
            // 
            this.labelAppName.AutoSize = true;
            this.labelAppName.Location = new System.Drawing.Point(10, 29);
            this.labelAppName.Name = "labelAppName";
            this.labelAppName.Size = new System.Drawing.Size(57, 13);
            this.labelAppName.TabIndex = 23;
            this.labelAppName.Text = "App Name";
            // 
            // textBoxAppName
            // 
            this.textBoxAppName.Location = new System.Drawing.Point(81, 26);
            this.textBoxAppName.Name = "textBoxAppName";
            this.textBoxAppName.Size = new System.Drawing.Size(198, 20);
            this.textBoxAppName.TabIndex = 22;
            this.textBoxAppName.TextChanged += new System.EventHandler(this.textBoxAppName_TextChanged);
            // 
            // labelRuntime
            // 
            this.labelRuntime.AutoSize = true;
            this.labelRuntime.Location = new System.Drawing.Point(10, 134);
            this.labelRuntime.Name = "labelRuntime";
            this.labelRuntime.Size = new System.Drawing.Size(46, 13);
            this.labelRuntime.TabIndex = 20;
            this.labelRuntime.Text = "Runtime";
            // 
            // labelFramework
            // 
            this.labelFramework.AutoSize = true;
            this.labelFramework.Location = new System.Drawing.Point(10, 107);
            this.labelFramework.Name = "labelFramework";
            this.labelFramework.Size = new System.Drawing.Size(59, 13);
            this.labelFramework.TabIndex = 18;
            this.labelFramework.Text = "Framework";
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(10, 81);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(29, 13);
            this.labelPath.TabIndex = 17;
            this.labelPath.Text = "Path";
            // 
            // textBoxPath
            // 
            this.textBoxPath.Location = new System.Drawing.Point(81, 78);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new System.Drawing.Size(127, 20);
            this.textBoxPath.TabIndex = 16;
            // 
            // labelAppUrl
            // 
            this.labelAppUrl.AutoSize = true;
            this.labelAppUrl.Location = new System.Drawing.Point(10, 55);
            this.labelAppUrl.Name = "labelAppUrl";
            this.labelAppUrl.Size = new System.Drawing.Size(51, 13);
            this.labelAppUrl.TabIndex = 15;
            this.labelAppUrl.Text = "App URL";
            // 
            // textBoxAppUrl
            // 
            this.textBoxAppUrl.Location = new System.Drawing.Point(81, 52);
            this.textBoxAppUrl.Name = "textBoxAppUrl";
            this.textBoxAppUrl.Size = new System.Drawing.Size(198, 20);
            this.textBoxAppUrl.TabIndex = 14;
            this.textBoxAppUrl.Tag = "auto";
            this.textBoxAppUrl.TextChanged += new System.EventHandler(this.textBoxAppUrl_TextChanged);
            // 
            // groupBoxServices
            // 
            this.groupBoxServices.Controls.Add(this.listViewServices);
            this.groupBoxServices.Controls.Add(this.buttonAddService);
            this.groupBoxServices.Location = new System.Drawing.Point(313, 13);
            this.groupBoxServices.Name = "groupBoxServices";
            this.groupBoxServices.Size = new System.Drawing.Size(251, 232);
            this.groupBoxServices.TabIndex = 16;
            this.groupBoxServices.TabStop = false;
            this.groupBoxServices.Text = "Services";
            // 
            // listViewServices
            // 
            this.listViewServices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName});
            this.listViewServices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewServices.HideSelection = false;
            this.listViewServices.Location = new System.Drawing.Point(7, 20);
            this.listViewServices.Name = "listViewServices";
            this.listViewServices.Size = new System.Drawing.Size(238, 177);
            this.listViewServices.TabIndex = 18;
            this.listViewServices.UseCompatibleStateImageBehavior = false;
            this.listViewServices.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Service Name";
            this.columnHeaderName.Width = 232;
            // 
            // buttonAddService
            // 
            this.buttonAddService.Location = new System.Drawing.Point(170, 203);
            this.buttonAddService.Name = "buttonAddService";
            this.buttonAddService.Size = new System.Drawing.Size(75, 23);
            this.buttonAddService.TabIndex = 17;
            this.buttonAddService.Text = "Add...";
            this.buttonAddService.UseVisualStyleBackColor = true;
            this.buttonAddService.Click += new System.EventHandler(this.buttonAddService_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(407, 251);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 17;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 252);
            this.progressBar.Maximum = 7;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(389, 21);
            this.progressBar.TabIndex = 33;
            this.progressBar.Visible = false;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 283);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(579, 22);
            this.statusStrip.TabIndex = 34;
            this.statusStrip.Text = "statusStrip1";
            // 
            // labelStatus
            // 
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // PushAppForm
            // 
            this.AcceptButton = this.buttonPush;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(579, 305);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxServices);
            this.Controls.Add(this.groupBoxAppInfo);
            this.Controls.Add(this.buttonPush);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PushAppForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Push Application to CloudFoundry";
            this.Load += new System.EventHandler(this.PushAppForm_Load);
            this.groupBoxAppInfo.ResumeLayout(false);
            this.groupBoxAppInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInstances)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMemory)).EndInit();
            this.groupBoxServices.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonPush;
        private System.Windows.Forms.GroupBox groupBoxAppInfo;
        private System.Windows.Forms.Label labelRuntime;
        private System.Windows.Forms.Label labelFramework;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.TextBox textBoxPath;
        private System.Windows.Forms.Label labelAppUrl;
        private System.Windows.Forms.TextBox textBoxAppUrl;
        private System.Windows.Forms.GroupBox groupBoxServices;
        private System.Windows.Forms.Button buttonAddService;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonBrowsePath;
        private System.Windows.Forms.Label labelAppName;
        private System.Windows.Forms.TextBox textBoxAppName;
        private System.Windows.Forms.Label labelMemory;
        private System.Windows.Forms.NumericUpDown numericUpDownMemory;
        private System.Windows.Forms.ListView listViewServices;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private ImageComboBox imageComboBoxRuntimes;
        private Uhuru.CloudFoundry.UI. ImageComboBox imageComboBoxFrameworks;
        private System.Windows.Forms.Button buttonGetFramework;
        private System.Windows.Forms.Label labelInstances;
        private System.Windows.Forms.NumericUpDown numericUpDownInstances;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel labelStatus;
    }
}