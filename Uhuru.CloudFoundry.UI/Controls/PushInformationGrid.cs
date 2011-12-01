using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Uhuru.CloudFoundry.UI.Packaging;
using System.Reflection;
using CloudFoundry.Net;

namespace Uhuru.CloudFoundry.UI.Controls
{
    public partial class PushInformationGrid : UserControl
    {
        private PackagePusher packagePusher;

        public void Reset()
        {
            dataGrid.Rows.Clear();
            progressBar.Value = 0;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public PackagePusher PackagePusher
        {
            get { return packagePusher; }
            set
            {
                packagePusher = value;
                this.dataGrid.Rows.Clear();
                packagePusher.OnPushEvent += new EventHandler<PushEventArgs>(packagePusher_OnPushEvent);
            }
        }

        public PushInformationGrid()
        {
            InitializeComponent();

            Assembly asm = Assembly.GetExecutingAssembly();
            imageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("Uhuru.CloudFoundry.UI.Images.smallicons.bmp")));
            imageList.TransparentColor = Color.White;
        }

        void packagePusher_OnPushEvent(object sender, PushEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                                {
                                    UpdateInformation(e);
                                }));
            }
            else
            {
                UpdateInformation(e);
            }
        }

        private void UpdateInformation(PushEventArgs e)
        {
            bool isLastRowSelected = (dataGrid.SelectedRows.Count > 0 && dataGrid.SelectedRows[0].Index == dataGrid.Rows.Count - 1);
            
            progressBar.Value = e.Progress;
            int nextMsgNumber = dataGrid.Rows.Count + 1;
            dataGrid.Rows.Add(GetEventTypeImage(e.EventType), e.Message, e.ApplicationName, nextMsgNumber);

            if (isLastRowSelected) //in this case we want to keep the selection on the last row
                dataGrid.Rows[dataGrid.Rows.Count - 1].Selected = true;
        }

        private Image GetEventTypeImage(PushEventType pushEventType)
        {
            Utils.SmallImages imageIndex = Utils.SmallImages.DefaultImage;
            switch (pushEventType)
            {
                case PushEventType.INFORMATION:
                    imageIndex = Utils.SmallImages.VSInfo;
                    break;
                case PushEventType.WARNING:
                    imageIndex = Utils.SmallImages.VSWarning;
                    break;
                case PushEventType.ERROR:
                    imageIndex = Utils.SmallImages.VSError;
                    break;
                default:
                    break;
            }

            return imageList.Images[(int)imageIndex];
        }
      


    }
}
