using System.Security.Permissions;
using System.Windows.Forms;

namespace Uhuru.CloudFoundry.UI.VS2010
{
    /// <summary>
    /// Summary description for ToolWindowDeploymentStatusControl.
    /// </summary>
    public partial class ToolWindowDeploymentStatusControl : UserControl
    {
        public ToolWindowDeploymentStatusControl()
        {
            InitializeComponent();
            IntegrationCenter.PushInfoGrid = this.pushInformationGrid;
        }

        /// <summary> 
        /// Let this control process the mnemonics.
        /// </summary>
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogChar(char charCode)
        {
              // If we're the top-level form or control, we need to do the mnemonic handling
              if (charCode != ' ' && ProcessMnemonic(charCode))
              {
                    return true;
              }
              return base.ProcessDialogChar(charCode);
        }

        /// <summary>
        /// Enable the IME status handling for this control.
        /// </summary>
        protected override bool CanEnableIme
        {
            get
            {
                return true;
            }
        }

    }
}