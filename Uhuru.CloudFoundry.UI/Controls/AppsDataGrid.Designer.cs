namespace Uhuru.CloudFoundry.UI
{
    partial class AppsDataGrid
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.Column1 = new Uhuru.CloudFoundry.UI.DataGridViewImageAndTextColumn();
            this.Column2 = new Uhuru.CloudFoundry.UI.DataGridViewProgressColumn();
            this.Column3 = new Uhuru.CloudFoundry.UI.DataGridViewUriListColumn();
            this.Column4 = new Uhuru.CloudFoundry.UI.DataGridViewImageAndTextColumn();
            this.Column5 = new Uhuru.CloudFoundry.UI.DataGridViewImageAndTextColumn();
            this.Column6 = new Uhuru.CloudFoundry.UI.DataGridViewImageAndTextColumn();
            this.dataGridViewImageAndTextColumn1 = new Uhuru.CloudFoundry.UI.DataGridViewImageAndTextColumn();
            this.dataGridViewProgressColumn1 = new Uhuru.CloudFoundry.UI.DataGridViewProgressColumn();
            this.dataGridViewUriListColumn1 = new Uhuru.CloudFoundry.UI.DataGridViewUriListColumn();
            this.dataGridViewImageAndTextColumn2 = new Uhuru.CloudFoundry.UI.DataGridViewImageAndTextColumn();
            this.dataGridViewImageAndTextColumn3 = new Uhuru.CloudFoundry.UI.DataGridViewImageAndTextColumn();
            this.dataGridViewImageAndTextColumn4 = new Uhuru.CloudFoundry.UI.DataGridViewImageAndTextColumn();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.startApps = new System.Windows.Forms.ToolStripMenuItem();
            this.stopApps = new System.Windows.Forms.ToolStripMenuItem();
            this.removeApps = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.browseApps = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGrid
            // 
            this.dataGrid.AllowUserToAddRows = false;
            this.dataGrid.AllowUserToDeleteRows = false;
            this.dataGrid.AllowUserToResizeColumns = false;
            this.dataGrid.AllowUserToResizeRows = false;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5,
            this.Column6});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGrid.DefaultCellStyle = dataGridViewCellStyle8;
            this.dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGrid.Location = new System.Drawing.Point(0, 0);
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.ReadOnly = true;
            this.dataGrid.RowHeadersVisible = false;
            this.dataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGrid.Size = new System.Drawing.Size(745, 323);
            this.dataGrid.TabIndex = 2;
            this.dataGrid.SelectionChanged += new System.EventHandler(this.dataGrid_SelectionChanged);
            this.dataGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dataGrid_MouseClick);
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.HeaderText = "App Name";
            this.Column1.MinimumWidth = 100;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Health";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "URLs";
            this.Column3.MinimumWidth = 50;
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 150;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Services";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 170;
            // 
            // Column5
            // 
            this.Column5.HeaderText = "Framework";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            // 
            // Column6
            // 
            this.Column6.HeaderText = "Runtime";
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            // 
            // dataGridViewImageAndTextColumn1
            // 
            this.dataGridViewImageAndTextColumn1.Frozen = true;
            this.dataGridViewImageAndTextColumn1.HeaderText = "App Name";
            this.dataGridViewImageAndTextColumn1.Name = "dataGridViewImageAndTextColumn1";
            // 
            // dataGridViewProgressColumn1
            // 
            this.dataGridViewProgressColumn1.HeaderText = "Health";
            this.dataGridViewProgressColumn1.Name = "dataGridViewProgressColumn1";
            // 
            // dataGridViewUriListColumn1
            // 
            this.dataGridViewUriListColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewUriListColumn1.HeaderText = "URLs";
            this.dataGridViewUriListColumn1.MinimumWidth = 50;
            this.dataGridViewUriListColumn1.Name = "dataGridViewUriListColumn1";
            // 
            // dataGridViewImageAndTextColumn2
            // 
            this.dataGridViewImageAndTextColumn2.HeaderText = "Services";
            this.dataGridViewImageAndTextColumn2.Name = "dataGridViewImageAndTextColumn2";
            // 
            // dataGridViewImageAndTextColumn3
            // 
            this.dataGridViewImageAndTextColumn3.HeaderText = "Framework";
            this.dataGridViewImageAndTextColumn3.Name = "dataGridViewImageAndTextColumn3";
            // 
            // dataGridViewImageAndTextColumn4
            // 
            this.dataGridViewImageAndTextColumn4.HeaderText = "Runtime";
            this.dataGridViewImageAndTextColumn4.Name = "dataGridViewImageAndTextColumn4";
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startApps,
            this.stopApps,
            this.toolStripSeparator1,
            this.browseApps,
            this.toolStripSeparator2,
            this.removeApps});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(192, 126);
            // 
            // startApps
            // 
            this.startApps.Name = "startApps";
            this.startApps.Size = new System.Drawing.Size(191, 22);
            this.startApps.Text = "Start selected apps";
            this.startApps.Click += new System.EventHandler(this.startApps_Click);
            // 
            // stopApps
            // 
            this.stopApps.Name = "stopApps";
            this.stopApps.Size = new System.Drawing.Size(191, 22);
            this.stopApps.Text = "Stop selected apps";
            this.stopApps.Click += new System.EventHandler(this.stopApps_Click);
            // 
            // removeApps
            // 
            this.removeApps.Name = "removeApps";
            this.removeApps.Size = new System.Drawing.Size(191, 22);
            this.removeApps.Text = "Remove selected apps";
            this.removeApps.Click += new System.EventHandler(this.removeApps_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(188, 6);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(188, 6);
            // 
            // browseApps
            // 
            this.browseApps.Name = "browseApps";
            this.browseApps.Size = new System.Drawing.Size(191, 22);
            this.browseApps.Text = "Browse selected apps";
            this.browseApps.Click += new System.EventHandler(this.browseApps_Click);
            // 
            // AppsDataGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGrid);
            this.Name = "AppsDataGrid";
            this.Size = new System.Drawing.Size(745, 323);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGrid;
        private DataGridViewImageAndTextColumn dataGridViewImageAndTextColumn1;
        private DataGridViewProgressColumn dataGridViewProgressColumn1;
        private DataGridViewUriListColumn dataGridViewUriListColumn1;
        private DataGridViewImageAndTextColumn dataGridViewImageAndTextColumn2;
        private DataGridViewImageAndTextColumn dataGridViewImageAndTextColumn3;
        private DataGridViewImageAndTextColumn dataGridViewImageAndTextColumn4;
        private DataGridViewImageAndTextColumn Column1;
        private DataGridViewProgressColumn Column2;
        private DataGridViewUriListColumn Column3;
        private DataGridViewImageAndTextColumn Column4;
        private DataGridViewImageAndTextColumn Column5;
        private DataGridViewImageAndTextColumn Column6;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem startApps;
        private System.Windows.Forms.ToolStripMenuItem stopApps;
        private System.Windows.Forms.ToolStripMenuItem removeApps;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem browseApps;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}
