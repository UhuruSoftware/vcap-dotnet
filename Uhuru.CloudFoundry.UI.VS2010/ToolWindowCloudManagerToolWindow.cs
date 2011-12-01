
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
    /// This class implements the tool window ToolWindowCloudManagerToolWindow exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("f3bebe36-84db-4237-b11e-9eae03bce312")]
    public class ToolWindowCloudManagerToolWindow : ToolWindowCloudManagerToolWindowBase
    {
		ToolWindowCloudManagerControl control = new ToolWindowCloudManagerControl();

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ToolWindowCloudManagerToolWindow()
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