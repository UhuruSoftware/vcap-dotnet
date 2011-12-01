using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CloudFoundry.Net.Management.MMCSnapIn.Controls
{
    public partial class AppViewControl : UserControl
    {
        private App theApp = null;
        private bool selected = false;
        private List<ProvisionedService> allServices = new List<ProvisionedService>();

        private const int SERVICE_ITEM_HEIGHT = 18;

        public event Action<object, MouseEventArgs> SomethingWasClicked;

        public AppViewControl()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            Assembly asm = Assembly.GetExecutingAssembly();
            imageList.Images.AddStrip(Bitmap.FromStream(asm.GetManifestResourceStream("CloudFoundry.Net.Management.MMCSnapIn.Images.smallicons.bmp")));

            labelWithImageFramework.ImageList = imageList;
            labelWithImageRuntime.ImageList = imageList;
            listViewServices.SmallImageList = imageList;
        }

        public App App
        {
            get { return theApp; }
            set { 
                    theApp = value;
                    FillFields();
                }
        }

        public List<ProvisionedService> AllServices
        {
            get { return allServices; }
            set { allServices = value; }
        }

        private void FillFields()
        {
            labelAppName.Text = theApp.Name;
            labelAppHealth.Text = string.Format("{0}/{1}", theApp.RunningInstances, theApp.Instances);
                                    
            healthBar.Maximum = Convert.ToInt32(theApp.Instances);
            healthBar.Value = Convert.ToInt32(theApp.RunningInstances);

            labelWithImageFramework.SetTextAndImageIndex(theApp.StagingModel, Utils.GetFrameworkImageIndex(theApp.StagingModel));
            labelWithImageRuntime.SetTextAndImageIndex(theApp.StagingStack, Utils.GetRuntimeImageIndex(theApp.StagingStack));

            if (theApp.RunningInstances == theApp.Instances)
            {
                pic.Image = imageList.Images[(int)Utils.SmallImages.AllOK];
            }
            else if (theApp.RunningInstances == "0")
            {
                pic.Image = imageList.Images[(int)Utils.SmallImages.Error];
            }
            else
            {
                pic.Image = imageList.Images[(int)Utils.SmallImages.Warning];
            }

            listViewServices.Items.Clear();
            string[] serviceNames = theApp.ServiceNames;
            foreach (string serviceName in serviceNames)
            {
                ProvisionedService service = allServices.Find(s => s.Name == serviceName);
                ListViewItem item = new ListViewItem(new string[] { serviceName }, service == null ? (int)Utils.SmallImages.DefaultImage : Utils.GetServiceImageIndex(service.Vendor));
                listViewServices.Items.Add(item);
            }
            
            listBoxAppURLs.Items.Clear();
            listBoxAppURLs.Items.AddRange(theApp.UriList);
            
            //compute lists height so that all elements will be shown
            listBoxAppURLs.Height = listBoxAppURLs.ItemHeight * listBoxAppURLs.Items.Count;
            listViewServices.Height = listViewServices.Items.Count == 0 ? 0 : SERVICE_ITEM_HEIGHT * listViewServices.Items.Count;
            
            //set the height of the cotrol as needed in order to fit all component controls
            this.Height = new List<int>() { pic.Bottom, listBoxAppURLs.Bottom, listViewServices.Bottom }.Max();
        }


        public bool Selected 
        {
            get { return selected; }
            set {
                    selected = value;
                    Color backColor = value ? Color.Azure : Color.White; 

                    this.BackColor = backColor;
                    foreach (Control c in this.Controls)
                    {
                        c.BackColor = backColor;
                    }
                } 
        }

        private void Control_MouseClick(object sender, MouseEventArgs e)
        {
            if (sender == listBoxAppURLs)
            {
                //if an url was clicked, open it
                int clickedItemIndex = listBoxAppURLs.IndexFromPoint(new Point(e.X, e.Y));
                if (clickedItemIndex != ListBox.NoMatches)
                {
                    //check if the actual item was clicked or the click was outside the text
                    SizeF size = this.CreateGraphics().MeasureString(listBoxAppURLs.Items[clickedItemIndex].ToString(), listBoxAppURLs.Font);
                    if (e.X <= size.Width)
                        Utils.OpenLink(listBoxAppURLs.Items[clickedItemIndex].ToString());
                }
            }
            
            //if one of the controls was clicked, notify the parent
            if (SomethingWasClicked != null)
                SomethingWasClicked(this, e);
        }

        private void AppViewControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (SomethingWasClicked != null)
                SomethingWasClicked(this, e);
        }

       
    }
}
