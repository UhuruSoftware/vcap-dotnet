
namespace Uhuru.CloudFoundry.UI.VS2010
{
    partial class ToolWindowDeploymentStatusControl
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
            this.pushInformationGrid = new Uhuru.CloudFoundry.UI.Controls.PushInformationGrid();
            this.SuspendLayout();
            // 
            // pushInformationGrid
            // 
            this.pushInformationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pushInformationGrid.Location = new System.Drawing.Point(0, 0);
            this.pushInformationGrid.Name = "pushInformationGrid";
            this.pushInformationGrid.Size = new System.Drawing.Size(393, 214);
            this.pushInformationGrid.TabIndex = 1;
            // 
            // ToolWindowDeploymentStatusControl
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.pushInformationGrid);
            this.Name = "ToolWindowDeploymentStatusControl";
            this.Size = new System.Drawing.Size(393, 214);
            this.ResumeLayout(false);

        }
        #endregion

        private Controls.PushInformationGrid pushInformationGrid;

    }
}