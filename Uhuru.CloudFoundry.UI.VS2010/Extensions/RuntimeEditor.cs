using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Reflection;
using System.ComponentModel;
using CloudFoundry.Net;
using System.Drawing;

namespace Uhuru.CloudFoundry.UI.VS2010.Extensions
{
    public class RuntimeEditor : UITypeEditor
    {
        ImageList imageList = new ImageList();


        public RuntimeEditor()
            : base()
        {
            Assembly assembly = Assembly.Load("Uhuru.CloudFoundry.UI");
            imageList.Images.AddStrip(Bitmap.FromStream(assembly.GetManifestResourceStream("Uhuru.CloudFoundry.UI.Images.smallicons.bmp")));
        }


        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs pe)
        {
            int imageIndex = 0;
            string runtime = (string)pe.Value;

            imageIndex = Utils.GetRuntimeImageIndex(runtime);

            pe.Graphics.DrawImage(imageList.Images[imageIndex], pe.Bounds);
        }
    }
}
