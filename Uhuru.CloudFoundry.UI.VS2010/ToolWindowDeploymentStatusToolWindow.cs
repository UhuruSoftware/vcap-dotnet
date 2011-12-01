
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace Uhuru.CloudFoundry.UI.VS2010
{
	/// <summary>
    /// This class implements the tool window ToolWindowDeploymentStatusToolWindow exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("f0816be4-4722-4c8b-af83-9dbfa8c02762")]
    public class ToolWindowDeploymentStatusToolWindow : ToolWindowDeploymentStatusToolWindowBase
    {
		ToolWindowDeploymentStatusControl control = new ToolWindowDeploymentStatusControl();

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ToolWindowDeploymentStatusToolWindow()
        {
        }

		public override System.Windows.Forms.IWin32Window Window
        {
            get
            {
                return control;
            }
        }
	}
}