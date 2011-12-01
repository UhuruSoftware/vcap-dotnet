using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ManagementConsole;
using CloudFoundry.Net.Management.MMCSnapIn.Controls;

namespace CloudFoundry.Net.Management.MMCSnapIn.FormViews
{
    class CloudInfoFormView : FormView
    {
        CloudInfoContainer contentContainer = new CloudInfoContainer();

        protected override void OnInitialize(AsyncStatus status)
        {
            // Call the parent method.
            base.OnInitialize(status);

            // Get a typed reference to the hosted control
            // that is set up by the form view description.
            contentContainer = (CloudInfoContainer)this.Control;
            Refresh();
        }

        private void Refresh()
        {
            Client api = (Client)this.ScopeNode.Tag;
            contentContainer.Refresh(api);
        }

    }
}
