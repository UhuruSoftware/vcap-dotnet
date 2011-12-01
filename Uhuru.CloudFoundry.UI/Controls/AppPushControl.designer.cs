namespace Uhuru.CloudFoundry.UI
{
    partial class AppPushControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelInstances = new System.Windows.Forms.Label();
            this.numericUpDownInstances = new System.Windows.Forms.NumericUpDown();
            this.buttonGetFramework = new System.Windows.Forms.Button();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonAddService = new System.Windows.Forms.Button();
            this.numericUpDownMemory = new System.Windows.Forms.NumericUpDown();
            this.labelMemory = new System.Windows.Forms.Label();
            this.listViewServices = new System.Windows.Forms.ListView();
            this.buttonBrowsePath = new System.Windows.Forms.Button();
            this.labelAppName = new System.Windows.Forms.Label();
            this.textBoxAppName = new System.Windows.Forms.TextBox();
            this.labelRuntime = new System.Windows.Forms.Label();
            this.labelFramework = new System.Windows.Forms.Label();
            this.labelPath = new System.Windows.Forms.Label();
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.labelAppUrl = new System.Windows.Forms.Label();
            this.textBoxAppUrl = new System.Windows.Forms.TextBox();
            this.labelServices = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.imageComboBoxRuntimes = new ImageComboBox();
            this.imageComboBoxFrameworks = new ImageComboBox();
            this.labelStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInstances)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMemory)).BeginInit();
            this.SuspendLayout();
            // 
            // labelInstances
            // 
            this.labelInstances.AutoSize = true;
            this.labelInstances.Location = new System.Drawing.Point(522, 4);
            this.labelInstances.Name = "labelInstances";
            this.labelInstances.Size = new System.Drawing.Size(53, 13);
            this.labelInstances.TabIndex = 30;
            this.labelInstances.Text = "Instances";
            // 
            // numericUpDownInstances
            // 
            this.numericUpDownInstances.Location = new System.Drawing.Point(525, 20);
            this.numericUpDownInstances.Name = "numericUpDownInstances";
            this.numericUpDownInstances.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownInstances.TabIndex = 29;
            this.numericUpDownInstances.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonGetFramework
            // 
            this.buttonGetFramework.Location = new System.Drawing.Point(312, 18);
            this.buttonGetFramework.Name = "buttonGetFramework";
            this.buttonGetFramework.Size = new System.Drawing.Size(18, 23);
            this.buttonGetFramework.TabIndex = 28;
            this.buttonGetFramework.Text = "?";
            this.buttonGetFramework.UseVisualStyleBackColor = true;
            this.buttonGetFramework.TextChanged += new System.EventHandler(this.buttonGetFramework_Click);
            this.buttonGetFramework.Click += new System.EventHandler(this.buttonGetFramework_Click);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Service Name";
            this.columnHeaderName.Width = 232;
            // 
            // buttonAddService
            // 
            this.buttonAddService.Location = new System.Drawing.Point(815, 20);
            this.buttonAddService.Name = "buttonAddService";
            this.buttonAddService.Size = new System.Drawing.Size(44, 47);
            this.buttonAddService.TabIndex = 17;
            this.buttonAddService.Text = "Add...";
            this.buttonAddService.UseVisualStyleBackColor = true;
            this.buttonAddService.Click += new System.EventHandler(this.buttonAddService_Click);
            // 
            // numericUpDownMemory
            // 
            this.numericUpDownMemory.Location = new System.Drawing.Point(578, 20);
            this.numericUpDownMemory.Name = "numericUpDownMemory";
            this.numericUpDownMemory.Size = new System.Drawing.Size(55, 20);
            this.numericUpDownMemory.TabIndex = 24;
            this.numericUpDownMemory.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // labelMemory
            // 
            this.labelMemory.AutoSize = true;
            this.labelMemory.Location = new System.Drawing.Point(581, 5);
            this.labelMemory.Name = "labelMemory";
            this.labelMemory.Size = new System.Drawing.Size(58, 13);
            this.labelMemory.TabIndex = 25;
            this.labelMemory.Text = "Mem. (MB)";
            // 
            // listViewServices
            // 
            this.listViewServices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName});
            this.listViewServices.HideSelection = false;
            this.listViewServices.Location = new System.Drawing.Point(645, 20);
            this.listViewServices.Name = "listViewServices";
            this.listViewServices.Size = new System.Drawing.Size(164, 47);
            this.listViewServices.TabIndex = 18;
            this.listViewServices.UseCompatibleStateImageBehavior = false;
            this.listViewServices.View = System.Windows.Forms.View.SmallIcon;
            // 
            // buttonBrowsePath
            // 
            this.buttonBrowsePath.Location = new System.Drawing.Point(287, 18);
            this.buttonBrowsePath.Name = "buttonBrowsePath";
            this.buttonBrowsePath.Size = new System.Drawing.Size(24, 23);
            this.buttonBrowsePath.TabIndex = 18;
            this.buttonBrowsePath.Text = "...";
            this.buttonBrowsePath.UseVisualStyleBackColor = true;
            this.buttonBrowsePath.TextChanged += new System.EventHandler(this.buttonBrowsePath_Click);
            this.buttonBrowsePath.Click += new System.EventHandler(this.buttonBrowsePath_Click);
            // 
            // labelAppName
            // 
            this.labelAppName.AutoSize = true;
            this.labelAppName.Location = new System.Drawing.Point(4, 5);
            this.labelAppName.Name = "labelAppName";
            this.labelAppName.Size = new System.Drawing.Size(57, 13);
            this.labelAppName.TabIndex = 23;
            this.labelAppName.Text = "App Name";
            // 
            // textBoxAppName
            // 
            this.textBoxAppName.Location = new System.Drawing.Point(7, 20);
            this.textBoxAppName.Name = "textBoxAppName";
            this.textBoxAppName.Size = new System.Drawing.Size(75, 20);
            this.textBoxAppName.TabIndex = 22;
            this.textBoxAppName.TextChanged += new System.EventHandler(this.textBoxAppName_TextChanged);
            // 
            // labelRuntime
            // 
            this.labelRuntime.AutoSize = true;
            this.labelRuntime.Location = new System.Drawing.Point(432, 4);
            this.labelRuntime.Name = "labelRuntime";
            this.labelRuntime.Size = new System.Drawing.Size(46, 13);
            this.labelRuntime.TabIndex = 20;
            this.labelRuntime.Text = "Runtime";
            // 
            // labelFramework
            // 
            this.labelFramework.AutoSize = true;
            this.labelFramework.Location = new System.Drawing.Point(334, 4);
            this.labelFramework.Name = "labelFramework";
            this.labelFramework.Size = new System.Drawing.Size(59, 13);
            this.labelFramework.TabIndex = 18;
            this.labelFramework.Text = "Framework";
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(186, 5);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(29, 13);
            this.labelPath.TabIndex = 17;
            this.labelPath.Text = "Path";
            // 
            // textBoxPath
            // 
            this.textBoxPath.Location = new System.Drawing.Point(189, 20);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new System.Drawing.Size(95, 20);
            this.textBoxPath.TabIndex = 16;
            // 
            // labelAppUrl
            // 
            this.labelAppUrl.AutoSize = true;
            this.labelAppUrl.Location = new System.Drawing.Point(85, 4);
            this.labelAppUrl.Name = "labelAppUrl";
            this.labelAppUrl.Size = new System.Drawing.Size(51, 13);
            this.labelAppUrl.TabIndex = 15;
            this.labelAppUrl.Text = "App URL";
            // 
            // textBoxAppUrl
            // 
            this.textBoxAppUrl.Location = new System.Drawing.Point(88, 20);
            this.textBoxAppUrl.Name = "textBoxAppUrl";
            this.textBoxAppUrl.Size = new System.Drawing.Size(95, 20);
            this.textBoxAppUrl.TabIndex = 14;
            this.textBoxAppUrl.Tag = "auto";
            this.textBoxAppUrl.TextChanged += new System.EventHandler(this.textBoxAppUrl_TextChanged);
            // 
            // labelServices
            // 
            this.labelServices.AutoSize = true;
            this.labelServices.Location = new System.Drawing.Point(645, 4);
            this.labelServices.Name = "labelServices";
            this.labelServices.Size = new System.Drawing.Size(48, 13);
            this.labelServices.TabIndex = 31;
            this.labelServices.Text = "Services";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(7, 57);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(632, 10);
            this.progressBar.TabIndex = 32;
            // 
            // imageComboBoxRuntimes
            // 
            this.imageComboBoxRuntimes.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.imageComboBoxRuntimes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imageComboBoxRuntimes.FormattingEnabled = true;
            this.imageComboBoxRuntimes.ImageList = null;
            this.imageComboBoxRuntimes.Location = new System.Drawing.Point(435, 20);
            this.imageComboBoxRuntimes.Name = "imageComboBoxRuntimes";
            this.imageComboBoxRuntimes.Size = new System.Drawing.Size(84, 21);
            this.imageComboBoxRuntimes.TabIndex = 27;
            // 
            // imageComboBoxFrameworks
            // 
            this.imageComboBoxFrameworks.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.imageComboBoxFrameworks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imageComboBoxFrameworks.FormattingEnabled = true;
            this.imageComboBoxFrameworks.ImageList = null;
            this.imageComboBoxFrameworks.Location = new System.Drawing.Point(337, 20);
            this.imageComboBoxFrameworks.Name = "imageComboBoxFrameworks";
            this.imageComboBoxFrameworks.Size = new System.Drawing.Size(92, 21);
            this.imageComboBoxFrameworks.TabIndex = 26;
            this.imageComboBoxFrameworks.SelectedIndexChanged += new System.EventHandler(this.imageComboBoxFrameworks_SelectedIndexChanged);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(7, 42);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 13);
            this.labelStatus.TabIndex = 33;
            // 
            // AppPushControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.labelServices);
            this.Controls.Add(this.listViewServices);
            this.Controls.Add(this.labelInstances);
            this.Controls.Add(this.buttonAddService);
            this.Controls.Add(this.numericUpDownInstances);
            this.Controls.Add(this.buttonGetFramework);
            this.Controls.Add(this.imageComboBoxRuntimes);
            this.Controls.Add(this.textBoxAppUrl);
            this.Controls.Add(this.imageComboBoxFrameworks);
            this.Controls.Add(this.labelAppUrl);
            this.Controls.Add(this.labelMemory);
            this.Controls.Add(this.textBoxPath);
            this.Controls.Add(this.numericUpDownMemory);
            this.Controls.Add(this.labelPath);
            this.Controls.Add(this.buttonBrowsePath);
            this.Controls.Add(this.labelFramework);
            this.Controls.Add(this.labelAppName);
            this.Controls.Add(this.labelRuntime);
            this.Controls.Add(this.textBoxAppName);
            this.Name = "AppPushControl";
            this.Size = new System.Drawing.Size(865, 72);
            this.Load += new System.EventHandler(this.AppPushControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInstances)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMemory)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelInstances;
        private System.Windows.Forms.NumericUpDown numericUpDownInstances;
        private System.Windows.Forms.Button buttonGetFramework;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.Button buttonAddService;
        private ImageComboBox imageComboBoxRuntimes;
        private ImageComboBox imageComboBoxFrameworks;
        private System.Windows.Forms.NumericUpDown numericUpDownMemory;
        private System.Windows.Forms.Label labelMemory;
        private System.Windows.Forms.ListView listViewServices;
        private System.Windows.Forms.Button buttonBrowsePath;
        private System.Windows.Forms.Label labelAppName;
        private System.Windows.Forms.TextBox textBoxAppName;
        private System.Windows.Forms.Label labelRuntime;
        private System.Windows.Forms.Label labelFramework;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.TextBox textBoxPath;
        private System.Windows.Forms.Label labelAppUrl;
        private System.Windows.Forms.TextBox textBoxAppUrl;
        private System.Windows.Forms.Label labelServices;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelStatus;
    }
}
