namespace CloudFoundry.Net.Management.MMCSnapIn.Controls
{
    partial class AppViewControl
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
            this.components = new System.ComponentModel.Container();
            this.labelAppName = new System.Windows.Forms.Label();
            this.labelAppHealth = new System.Windows.Forms.Label();
            this.listBoxAppURLs = new System.Windows.Forms.ListBox();
            this.healthBar = new System.Windows.Forms.ProgressBar();
            this.pic = new System.Windows.Forms.PictureBox();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.listViewServices = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelWithImageRuntime = new CloudFoundry.Net.Management.MMCSnapIn.Controls.LabelWithImage();
            this.labelWithImageFramework = new CloudFoundry.Net.Management.MMCSnapIn.Controls.LabelWithImage();
            ((System.ComponentModel.ISupportInitialize)(this.pic)).BeginInit();
            this.SuspendLayout();
            // 
            // labelAppName
            // 
            this.labelAppName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAppName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAppName.Location = new System.Drawing.Point(22, 2);
            this.labelAppName.Name = "labelAppName";
            this.labelAppName.Size = new System.Drawing.Size(175, 13);
            this.labelAppName.TabIndex = 0;
            this.labelAppName.Text = "appName";
            this.labelAppName.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Control_MouseClick);
            // 
            // labelAppHealth
            // 
            this.labelAppHealth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAppHealth.AutoSize = true;
            this.labelAppHealth.Location = new System.Drawing.Point(288, 0);
            this.labelAppHealth.Name = "labelAppHealth";
            this.labelAppHealth.Size = new System.Drawing.Size(56, 13);
            this.labelAppHealth.TabIndex = 1;
            this.labelAppHealth.Text = "appHealth";
            this.labelAppHealth.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Control_MouseClick);
            // 
            // listBoxAppURLs
            // 
            this.listBoxAppURLs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxAppURLs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBoxAppURLs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxAppURLs.FormattingEnabled = true;
            this.listBoxAppURLs.Location = new System.Drawing.Point(368, 0);
            this.listBoxAppURLs.Name = "listBoxAppURLs";
            this.listBoxAppURLs.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.listBoxAppURLs.Size = new System.Drawing.Size(197, 65);
            this.listBoxAppURLs.TabIndex = 4;
            this.listBoxAppURLs.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Control_MouseClick);
            // 
            // healthBar
            // 
            this.healthBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.healthBar.Location = new System.Drawing.Point(230, 0);
            this.healthBar.Name = "healthBar";
            this.healthBar.Size = new System.Drawing.Size(55, 13);
            this.healthBar.TabIndex = 5;
            // 
            // pic
            // 
            this.pic.Location = new System.Drawing.Point(0, 2);
            this.pic.Name = "pic";
            this.pic.Size = new System.Drawing.Size(16, 16);
            this.pic.TabIndex = 6;
            this.pic.TabStop = false;
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // listViewServices
            // 
            this.listViewServices.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewServices.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewServices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName});
            this.listViewServices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewServices.Location = new System.Drawing.Point(571, 0);
            this.listViewServices.MultiSelect = false;
            this.listViewServices.Name = "listViewServices";
            this.listViewServices.Scrollable = false;
            this.listViewServices.Size = new System.Drawing.Size(112, 67);
            this.listViewServices.TabIndex = 19;
            this.listViewServices.UseCompatibleStateImageBehavior = false;
            this.listViewServices.View = System.Windows.Forms.View.SmallIcon;
            this.listViewServices.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Control_MouseClick);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Service Name";
            this.columnHeaderName.Width = 232;
            // 
            // labelWithImageRuntime
            // 
            this.labelWithImageRuntime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWithImageRuntime.AutoSize = true;
            this.labelWithImageRuntime.Location = new System.Drawing.Point(776, 0);
            this.labelWithImageRuntime.Name = "labelWithImageRuntime";
            this.labelWithImageRuntime.Size = new System.Drawing.Size(86, 13);
            this.labelWithImageRuntime.TabIndex = 8;
            this.labelWithImageRuntime.Text = "labelWithImage2";
            // 
            // labelWithImageFramework
            // 
            this.labelWithImageFramework.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWithImageFramework.AutoSize = true;
            this.labelWithImageFramework.Location = new System.Drawing.Point(689, 0);
            this.labelWithImageFramework.Name = "labelWithImageFramework";
            this.labelWithImageFramework.Size = new System.Drawing.Size(86, 13);
            this.labelWithImageFramework.TabIndex = 7;
            this.labelWithImageFramework.Text = "labelWithImage1";
            // 
            // AppViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.listViewServices);
            this.Controls.Add(this.labelWithImageRuntime);
            this.Controls.Add(this.labelWithImageFramework);
            this.Controls.Add(this.pic);
            this.Controls.Add(this.healthBar);
            this.Controls.Add(this.listBoxAppURLs);
            this.Controls.Add(this.labelAppHealth);
            this.Controls.Add(this.labelAppName);
            this.Name = "AppViewControl";
            this.Size = new System.Drawing.Size(862, 80);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.AppViewControl_MouseClick);
            ((System.ComponentModel.ISupportInitialize)(this.pic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelAppName;
        private System.Windows.Forms.Label labelAppHealth;
        private System.Windows.Forms.ListBox listBoxAppURLs;
        private System.Windows.Forms.ProgressBar healthBar;
        private System.Windows.Forms.PictureBox pic;
        private System.Windows.Forms.ImageList imageList;
        private LabelWithImage labelWithImageFramework;
        private LabelWithImage labelWithImageRuntime;
        private System.Windows.Forms.ListView listViewServices;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
    }
}
