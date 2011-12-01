using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ManagementConsole;
using System.Security.Permissions;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using CloudFoundry.Net.Management.MMCSnapIn.ScopeNodes;

[assembly: PermissionSetAttribute(SecurityAction.RequestMinimum, Unrestricted = true)]
namespace CloudFoundry.Net.Management.MMCSnapIn
{
    [SnapInSettings("{6793E47B-A141-452D-BE58-CA051F9CD734}", DisplayName="Uhuru CloudFoundry Snap-In", Description="UI Interface to CloudFoundry")]
    public class CFSnapIn:SnapIn
    {
        public CFSnapIn()
        {
            this.RootNode = new ScopeNode() { DisplayName = "CloudFoundry Snap-In" };
            
            Assembly asm = Assembly.GetExecutingAssembly();

            this.SmallImages.TransparentColor = Color.White;
            this.SmallImages.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("CloudFoundry.Net.Management.MMCSnapIn.Images.smallicons.bmp")));

            TargetsScopeNode node = new TargetsScopeNode();
            this.RootNode.Children.Add(node);

        }
    }
}
