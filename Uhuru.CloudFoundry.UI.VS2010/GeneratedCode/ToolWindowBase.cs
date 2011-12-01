using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;

namespace Uhuru.CloudFoundry.UI.VS2010
{
	/// <summary>
    /// This class implements the tool window ToolWindowCloudManagerToolWindowBase exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("f3bebe36-84db-4237-b11e-9eae03bce312")]
    public class ToolWindowCloudManagerToolWindowBase : ToolWindowPane
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ToolWindowCloudManagerToolWindowBase()
            : base(null)
        {
			this.Caption = "Cloud Manager";
        }
    }
	/// <summary>
    /// This class implements the tool window ToolWindowDeploymentStatusToolWindowBase exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("f0816be4-4722-4c8b-af83-9dbfa8c02762")]
    public class ToolWindowDeploymentStatusToolWindowBase : ToolWindowPane
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ToolWindowDeploymentStatusToolWindowBase()
            : base(null)
        {
			this.Caption = "Cloud Foundry Deployment Status";
        }
    }
}