using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Timers;

namespace CpuBomb
{
    public partial class Bomb : System.Web.UI.Page
    {

        public volatile bool stopFlag;

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Timers.Timer newTimer = new System.Timers.Timer(1);
            newTimer.AutoReset = false;
            newTimer.Elapsed += new ElapsedEventHandler(delegate(object tsender, ElapsedEventArgs args)
            {
                while (!stopFlag)
                { }
            });
            newTimer.Enabled = true;
        }
    }
}