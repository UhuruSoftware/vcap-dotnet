using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.DEA
{
    class FileViewer
    {
        private int StartAttempts;
        public event EventHandler OnStartError;
    
        public bool IsRunning
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public int Port
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public string[] Credentials
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public string Uri
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public void Start(string DropletsPath)
        {
            throw new System.NotImplementedException();
        }
    }
}
