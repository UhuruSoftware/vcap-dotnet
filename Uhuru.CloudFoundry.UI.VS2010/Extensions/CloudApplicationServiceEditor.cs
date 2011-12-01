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
using Uhuru.CloudFoundry.UI.Packaging;

namespace Uhuru.CloudFoundry.UI.VS2010.Extensions
{
    public class CloudApplicationServiceEditor : UITypeEditor
    {
        ImageList imageList = new ImageList();


        public CloudApplicationServiceEditor()
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
            string sericeType = pe.Value as string;

            imageIndex = Utils.GetServiceImageIndex(sericeType);

            pe.Graphics.DrawImage(imageList.Images[imageIndex], pe.Bounds);
        }
    }
}
