using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA
{
    public class DeaReactor
    {

        public event EventHandler OnRouterStart;

        public event EventHandler OnHealthManagerStart;

        public event EventHandler OnDeaStart;

        public event EventHandler OnDeaStop;

        public event EventHandler OnDeaStatus;

        public event EventHandler OnDropletStatus;

        public event EventHandler OnDeaDiscover;

        public event EventHandler OnDeaFindDroplet;

        public event EventHandler OnDeaUpdate;

        public void SendDeaHeartbeat()
        {
            throw new System.NotImplementedException();
        }

        public void SendDeaStart()
        {
            throw new System.NotImplementedException();
        }

        public void SendDopletExited()
        {
            throw new System.NotImplementedException();
        }

        public void SendReply()
        {
            throw new System.NotImplementedException();
        }

        public void SendRouterRegister()
        {
            throw new System.NotImplementedException();
        }

        public void SendRouterUnregister()
        {
            throw new System.NotImplementedException();
        }
    }
}
