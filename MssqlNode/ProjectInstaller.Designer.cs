namespace mssqlnode
{
    partial class ProjectInstaller
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
            this.serviceProcessInstallerMssqlNode = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstallerMssqlNode = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstallerMssqlNode
            // 
            this.serviceProcessInstallerMssqlNode.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstallerMssqlNode.Password = null;
            this.serviceProcessInstallerMssqlNode.Username = null;
            this.serviceProcessInstallerMssqlNode.BeforeInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstallerMssqlNode_BeforeInstall);
            // 
            // serviceInstallerMssqlNode
            // 
            this.serviceInstallerMssqlNode.Description = "CloudFoundry Mssql Service Node";
            this.serviceInstallerMssqlNode.DisplayName = "Uhuru Mssql Node";
            this.serviceInstallerMssqlNode.ServiceName = "MssqlNodeService";
            this.serviceInstallerMssqlNode.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstallerMssqlNode,
            this.serviceInstallerMssqlNode});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstallerMssqlNode;
        private System.ServiceProcess.ServiceInstaller serviceInstallerMssqlNode;
    }
}