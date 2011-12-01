using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA
{
    public class DropletInstance
    {
        public DropletInstanceProperties Properties
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public DropletInstanceUsage Usage
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public int IsRunning
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public void Stage()
        {
            throw new System.NotImplementedException();
        }

        public void Run()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// returns the instances heartbeat json message
        /// </summary>
        public object GenerateHearbeatMessage()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// returns the instances unregister json message
        /// </summary>
        public object GenerateRouterUnregisterMessage()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// returns the instances exited message
        /// </summary>
        public object GenerateDropletExitedMessage()
        {
            throw new System.NotImplementedException();
        }

        public void GenerateDropletStatusResponse()
        {
            throw new System.NotImplementedException();
        }

        public void GenerateDeaFindDropletResponse()
        {
            throw new System.NotImplementedException();
        }

        public void GenerateRouterRegisterMessage()
        {
            throw new System.NotImplementedException();
        }

        public void StopDroplet()
        {
            throw new System.NotImplementedException();
        }

        public void SetupInstanceEnvironment()
        {
            throw new System.NotImplementedException();
        }

        private void CreateDebugForEnvironment()
        {
            throw new System.NotImplementedException();
        }

        private void CreateInstanceForEnvironment()
        {
            throw new System.NotImplementedException();
        }

        private void CreateLegacyServicesForEnvironment()
        {
            throw new System.NotImplementedException();
        }

        private void CreateServicesForEnvironment()
        {
            throw new System.NotImplementedException();
        }

        public void DetectAppPid()
        {
            throw new System.NotImplementedException();
        }

        public void DetectAppReady()
        {
            throw new System.NotImplementedException();
        }
    }
}
