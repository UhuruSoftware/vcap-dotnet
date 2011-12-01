using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace Uhuru.CloudFoundry.UI.VS2010
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
	[ProvideToolWindow(typeof(ToolWindowCloudManagerToolWindow), Orientation=ToolWindowOrientation.Bottom, Style=VsDockStyle.Tabbed, MultiInstances = false, Transient = false, PositionX = 100 , PositionY = 100 , Width = 300 , Height = 500 , Window = /* Error List */ "d78612c7-9962-4b83-95d9-268046dad23a" )]
	[ProvideToolWindowVisibility(typeof(ToolWindowCloudManagerToolWindow), VSConstants.UICONTEXT.NoSolution_string)]
	[ProvideToolWindow(typeof(ToolWindowDeploymentStatusToolWindow), Orientation=ToolWindowOrientation.Bottom, Style=VsDockStyle.Tabbed, MultiInstances = false, Transient = false, PositionX = 100 , PositionY = 100 , Width = 300 , Height = 500 , Window = /* ToolWindowCloudManager */ "f3bebe36-84db-4237-b11e-9eae03bce312" )]
	[ProvideToolWindowVisibility(typeof(ToolWindowDeploymentStatusToolWindow), VSConstants.UICONTEXT.NoSolution_string)]
	[Guid(GuidList.guidUhuruCloudFoundryUIVS2010PkgString)]
    public abstract class UhuruCloudFoundryUIVS2010PackageBase : Package
    {
		/// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public UhuruCloudFoundryUIVS2010PackageBase()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

			// Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
				CommandID commandId;
				OleMenuCommand menuItem;

				// Create the command for button ButtonBuildAllAndPush
                commandId = new CommandID(GuidList.guidUhuruCloudFoundryUIVS2010CmdSet, (int)PkgCmdIDList.ButtonBuildAllAndPush);
                menuItem = new OleMenuCommand(ButtonBuildAllAndPushExecuteHandler, ButtonBuildAllAndPushChangeHandler, ButtonBuildAllAndPushQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ButtonCloudManager
                commandId = new CommandID(GuidList.guidUhuruCloudFoundryUIVS2010CmdSet, (int)PkgCmdIDList.ButtonCloudManager);
                menuItem = new OleMenuCommand(ButtonCloudManagerExecuteHandler, ButtonCloudManagerChangeHandler, ButtonCloudManagerQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ButtonBuildAndPushProject
                commandId = new CommandID(GuidList.guidUhuruCloudFoundryUIVS2010CmdSet, (int)PkgCmdIDList.ButtonBuildAndPushProject);
                menuItem = new OleMenuCommand(ButtonBuildAndPushProjectExecuteHandler, ButtonBuildAndPushProjectChangeHandler, ButtonBuildAndPushProjectQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ButtonDeploymentWindow
                commandId = new CommandID(GuidList.guidUhuruCloudFoundryUIVS2010CmdSet, (int)PkgCmdIDList.ButtonDeploymentWindow);
                menuItem = new OleMenuCommand(ButtonDeploymentWindowExecuteHandler, ButtonDeploymentWindowChangeHandler, ButtonDeploymentWindowQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ButtonBuildAndPushWebSite
                commandId = new CommandID(GuidList.guidUhuruCloudFoundryUIVS2010CmdSet, (int)PkgCmdIDList.ButtonBuildAndPushWebSite);
                menuItem = new OleMenuCommand(ButtonBuildAndPushProjectExecuteHandler, ButtonBuildAndPushWebSiteChangeHandler, ButtonBuildAndPushWebSiteQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);

			}
		}
		
		#endregion

		#region Handlers for Button: ButtonBuildAllAndPush

		protected virtual void ButtonBuildAllAndPushExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ButtonBuildAllAndPushChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ButtonBuildAllAndPushQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ButtonCloudManager

		protected virtual void ButtonCloudManagerExecuteHandler(object sender, EventArgs e)
		{
			ShowToolWindowToolWindowCloudManager(sender, e);
		}
		
		protected virtual void ButtonCloudManagerChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ButtonCloudManagerQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ButtonBuildAndPushProject

		protected virtual void ButtonBuildAndPushProjectExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ButtonBuildAndPushProjectChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ButtonBuildAndPushProjectQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ButtonDeploymentWindow

		protected virtual void ButtonDeploymentWindowExecuteHandler(object sender, EventArgs e)
		{
			ShowToolWindowToolWindowDeploymentStatus(sender, e);
		}
		
		protected virtual void ButtonDeploymentWindowChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ButtonDeploymentWindowQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ButtonBuildAndPushWebSite

		protected virtual void ButtonBuildAndPushWebSiteExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ButtonBuildAndPushWebSiteChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ButtonBuildAndPushWebSiteQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindowToolWindowCloudManager(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(ToolWindowCloudManagerToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(String.Format("Can not create Toolwindow: ToolWindowCloudManager"));
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindowToolWindowDeploymentStatus(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(ToolWindowDeploymentStatusToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(String.Format("Can not create Toolwindow: ToolWindowDeploymentStatus"));
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        protected void ShowMessage(string message)
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "UhuruCloudFoundryUIVS2010",
                       string.Format(CultureInfo.CurrentCulture, message, this.ToString()),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }
    }
}
