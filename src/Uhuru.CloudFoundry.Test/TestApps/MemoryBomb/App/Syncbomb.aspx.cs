using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;

namespace MemoryBomb
{
    public partial class syncbomb : System.Web.UI.Page
    {

        List<object> bomb = new List<object>();

        protected void Page_Load(object sender, EventArgs e)
        {
            while(true)
            {
                //bomb.Add(new byte[100 * 1000]);
                bomb.Add(Enumerable.Repeat<byte>(1, 100 * 1000).ToArray());
                Thread.Sleep(20);
            }
        }
    }
}