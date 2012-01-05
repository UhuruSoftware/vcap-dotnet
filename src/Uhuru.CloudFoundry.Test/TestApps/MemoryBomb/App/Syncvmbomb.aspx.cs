using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;

namespace MemoryBomb
{
    public partial class Syncvmbomb : System.Web.UI.Page
    {

        List<object> bomb = new List<object>();

        protected void Page_Load(object sender, EventArgs e)
        {
            while (true)
            {
                bomb.Add(new byte[100 * 1000]);
                Thread.Sleep(20);
            }
        }
    }
}