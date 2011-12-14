using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Timers;

namespace MemoryBomb
{
    public partial class Bomb : System.Web.UI.Page
    {

        List<object> bomb = new List<object>();

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Timers.Timer newTimer = new System.Timers.Timer(1);
            newTimer.AutoReset = true;
            newTimer.Elapsed += new ElapsedEventHandler(delegate(object tsender, ElapsedEventArgs args)
            {
                bomb.Add(new byte[100 * 1000]);
            });
            newTimer.Enabled = true;
        }
    }
}