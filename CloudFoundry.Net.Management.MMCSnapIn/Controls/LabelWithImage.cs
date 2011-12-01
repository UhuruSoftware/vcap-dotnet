using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CloudFoundry.Net.Management.MMCSnapIn.Controls
{
    public partial class LabelWithImage : Label
    {
        private const int SPACING = 5;
        
        public LabelWithImage()
        {
            InitializeComponent();
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                AdjustTextAndImage();
            }
        }

        public void SetTextAndImageIndex(string text, int imageIndex)
        {
            this.ImageIndex = imageIndex;
            this.Text = text;
        }

        private void AdjustTextAndImage()
        {
            //basically the idea behind this label is that if an image is set (via the imageIndex) the length of the label is increased and the text right aligned so that it doesn't overlap the image
            if (this.ImageIndex < 0 || this.ImageList == null) return;

            this.ImageAlign = ContentAlignment.MiddleLeft;
            this.TextAlign = ContentAlignment.MiddleRight;

            if (this.AutoSize)
            {
                this.AutoSize = false;
                Graphics g = this.CreateGraphics();
                SizeF size = g.MeasureString(base.Text, this.Font);

                this.Width = this.ImageList.Images[this.ImageIndex].Width + SPACING + (int)Math.Ceiling(size.Width);
                this.Height = this.ImageList.Images[this.ImageIndex].Height;
            }
        }
    }
}
