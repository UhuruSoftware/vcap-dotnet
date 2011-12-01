using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ManagementConsole;
using System.Windows.Forms;
using System.Xml;
using CloudFoundry.Net;

namespace CloudFoundry.Net.Management.MMCSnapIn.Lists
{
    public class UsersList : MmcListView
    {
        public UsersList()
        {
            
        }

        /// <summary>
        /// Defines the structure of the list view.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnInitialize(AsyncStatus status)
        {
            // do default handling
            base.OnInitialize(status);

            this.Columns[0].Title = "E-mail";
            this.Columns[0].SetWidth(300);

            // Add detail columns
            this.Columns.Add(new MmcListViewColumn("Is Admin", 50));
            this.Columns.Add(new MmcListViewColumn("Applications", 200));

            // Set to show all columns
            this.Mode = MmcListViewMode.Report;  // default (set for clarity)

            // Set to show refresh as an option
            this.SelectionData.EnabledStandardVerbs = StandardVerbs.Refresh;

        }

        protected override void OnShow()
        {
            Refresh();
        }


        /// <summary>
        /// Defines actions for selection.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnSelectionChanged(SyncStatus status)
        {
        }

        /// <summary>
        /// Placeholder.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnRefresh(AsyncStatus status)
        {
            MessageBox.Show("The method or operation is not implemented.");
        }
        /// <summary>
        /// Handles menu actions.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="status"></param>
        protected override void OnSelectionAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
            }
        }
        /// <summary>
        /// Shows selected items.
        /// </summary>
        private void ShowSelected()
        {
        }


        /// <summary>
        /// Loads the list view with data.
        /// </summary>
        public void Refresh()
        {
            Client api = (Client)this.ScopeNode.Tag;
            List<User> users = api.Users();

            this.ResultNodes.Clear();

            // Populate the list.
            foreach (User user in users)
            {
                ResultNode rNode = new ResultNode();
                rNode.DisplayName = user.Email;
                rNode.SubItemDisplayNames.Add(user.IsAdmin);
                rNode.SubItemDisplayNames.Add(user.Apps);
                rNode.ImageIndex = 21;
                this.ResultNodes.Add(rNode);
            }
        }

    }
}
