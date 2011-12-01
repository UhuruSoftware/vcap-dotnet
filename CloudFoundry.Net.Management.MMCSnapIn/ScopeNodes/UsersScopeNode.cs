using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ManagementConsole;
using System.Xml;
using System.IO;

namespace CloudFoundry.Net.Management.MMCSnapIn.ScopeNodes
{
    class UsersScopeNode:ScopeNode
    {

        public UsersScopeNode():base(true)
        {
            this.DisplayName = "Users";
            this.ImageIndex = 21;
            this.SelectedImageIndex = 21;
        }

   }
}
