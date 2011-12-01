
namespace Uhuru.CloudFoundry.UI.VS2010
{
    partial class ToolWindowCloudManagerControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }


        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelContainer = new System.Windows.Forms.Panel();
            this.buttonTargetManager = new System.Windows.Forms.Button();
            this.comboBoxTargets = new System.Windows.Forms.ComboBox();
            this.labelTarget = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitterGrids = new System.Windows.Forms.Splitter();
            this.appsDataGrid = new Uhuru.CloudFoundry.UI.AppsDataGrid();
            this.checkBoxShowProjectsInSolution = new System.Windows.Forms.CheckBox();
            this.cloudTargetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.panelContainer.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cloudTargetBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // panelContainer
            // 
            this.panelContainer.BackColor = System.Drawing.SystemColors.Control;
            this.panelContainer.Controls.Add(this.checkBoxShowProjectsInSolution);
            this.panelContainer.Controls.Add(this.buttonTargetManager);
            this.panelContainer.Controls.Add(this.comboBoxTargets);
            this.panelContainer.Controls.Add(this.labelTarget);
            this.panelContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelContainer.Location = new System.Drawing.Point(0, 0);
            this.panelContainer.Name = "panelContainer";
            this.panelContainer.Size = new System.Drawing.Size(699, 25);
            this.panelContainer.TabIndex = 4;
            // 
            // buttonTargetManager
            // 
            this.buttonTargetManager.Location = new System.Drawing.Point(324, 1);
            this.buttonTargetManager.Name = "buttonTargetManager";
            this.buttonTargetManager.Size = new System.Drawing.Size(107, 23);
            this.buttonTargetManager.TabIndex = 2;
            this.buttonTargetManager.Text = "Target Manager";
            this.buttonTargetManager.UseVisualStyleBackColor = true;
            this.buttonTargetManager.Click += new System.EventHandler(this.buttonTargetManager_Click);
            // 
            // comboBoxTargets
            // 
            this.comboBoxTargets.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.cloudTargetBindingSource, "Username", true));
            this.comboBoxTargets.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.cloudTargetBindingSource, "TargetId", true));
            this.comboBoxTargets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTargets.FormattingEnabled = true;
            this.comboBoxTargets.Location = new System.Drawing.Point(58, 2);
            this.comboBoxTargets.Name = "comboBoxTargets";
            this.comboBoxTargets.Size = new System.Drawing.Size(260, 21);
            this.comboBoxTargets.TabIndex = 1;
            this.comboBoxTargets.SelectedIndexChanged += new System.EventHandler(this.comboBoxTargets_SelectedIndexChanged);
            // 
            // labelTarget
            // 
            this.labelTarget.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTarget.AutoSize = true;
            this.labelTarget.Location = new System.Drawing.Point(6, 5);
            this.labelTarget.Name = "labelTarget";
            this.labelTarget.Size = new System.Drawing.Size(38, 13);
            this.labelTarget.TabIndex = 0;
            this.labelTarget.Text = "Target";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 340);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip.Size = new System.Drawing.Size(699, 22);
            this.statusStrip.TabIndex = 5;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // splitterGrids
            // 
            this.splitterGrids.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitterGrids.Location = new System.Drawing.Point(694, 25);
            this.splitterGrids.Name = "splitterGrids";
            this.splitterGrids.Size = new System.Drawing.Size(5, 315);
            this.splitterGrids.TabIndex = 6;
            this.splitterGrids.TabStop = false;
            // 
            // checkBoxShowProjectsInSolution
            // 
            this.checkBoxShowProjectsInSolution.AutoSize = true;
            this.checkBoxShowProjectsInSolution.Location = new System.Drawing.Point(449, 5);
            this.checkBoxShowProjectsInSolution.Name = "checkBoxShowProjectsInSolution";
            this.checkBoxShowProjectsInSolution.Size = new System.Drawing.Size(200, 17);
            this.checkBoxShowProjectsInSolution.TabIndex = 3;
            this.checkBoxShowProjectsInSolution.Text = "show only the projects in this solution";
            this.checkBoxShowProjectsInSolution.UseVisualStyleBackColor = true;
            this.checkBoxShowProjectsInSolution.CheckedChanged += new System.EventHandler(this.checkBoxShowProjectsInSolution_CheckedChanged);
            // 
            // appsDataGrid
            // 
            this.appsDataGrid.AllowDrop = true;
            this.appsDataGrid.ContextMenuEnabled = true;
            this.appsDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.appsDataGrid.Location = new System.Drawing.Point(0, 25);
            this.appsDataGrid.Name = "appsDataGrid";
            this.appsDataGrid.Size = new System.Drawing.Size(699, 315);
            this.appsDataGrid.TabIndex = 2;
            // 
            // cloudTargetBindingSource
            // 
            this.cloudTargetBindingSource.DataSource = typeof(Uhuru.CloudFoundry.UI.CloudTarget);
            // 
            // ToolWindowCloudManagerControl
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.splitterGrids);
            this.Controls.Add(this.appsDataGrid);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panelContainer);
            this.Name = "ToolWindowCloudManagerControl";
            this.Size = new System.Drawing.Size(699, 362);
            this.panelContainer.ResumeLayout(false);
            this.panelContainer.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cloudTargetBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private AppsDataGrid appsDataGrid;
        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.Button buttonTargetManager;
        private System.Windows.Forms.ComboBox comboBoxTargets;
        private System.Windows.Forms.BindingSource cloudTargetBindingSource;
        private System.Windows.Forms.Label labelTarget;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Splitter splitterGrids;
        private System.Windows.Forms.CheckBox checkBoxShowProjectsInSolution;

    }
}