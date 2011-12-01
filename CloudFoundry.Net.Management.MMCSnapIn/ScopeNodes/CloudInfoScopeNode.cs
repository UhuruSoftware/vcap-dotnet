using Microsoft.ManagementConsole;

namespace CloudFoundry.Net.Management.MMCSnapIn.ScopeNodes
{
    class CloudInfoScopeNode : ScopeNode
    {
        public CloudInfoScopeNode()
            : base(true)
        {
            this.DisplayName = "Cloud Info";
            this.ImageIndex = (int)Utils.SmallImages.Modules;
            this.SelectedImageIndex = (int)Utils.SmallImages.Modules;
            
            //this.EnabledStandardVerbs = StandardVerbs.Refresh;
        }

        protected override void OnRefresh(AsyncStatus status)
        {
            
        }

    }
}
