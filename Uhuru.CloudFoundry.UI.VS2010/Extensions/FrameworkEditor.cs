using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Reflection;
using CloudFoundry.Net;
using System.ComponentModel;
using System.Drawing;

namespace Uhuru.CloudFoundry.UI.VS2010.Extensions
{
    public class FrameworkEditor : UITypeEditor
    {
        ImageList imageList = new ImageList();

        public FrameworkEditor() : base()
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
            string framework = (string)pe.Value;
            imageIndex = Utils.GetFrameworkImageIndex(framework);
            pe.Graphics.DrawImage(imageList.Images[imageIndex], pe.Bounds);
        }
    }
}
