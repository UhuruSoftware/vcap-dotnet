using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Uhuru.CloudFoundry.Server.DEA
{
    class Stager
    {
        public Dictionary<string, DeaRuntime> Runtimes {get; set;}

        public string DropletDir {get; set;}
        public string StagedDir {get; set; }
        public string AppsDir {get; set;}
        public string DbDir {get; set;}
            
        public bool ForeHttpFileSharing{get; set; }

        public Stager()
        {
            Runtimes = new Dictionary<string, DeaRuntime>();
        }

        public void RuntimesSupportes()
        {
            throw new System.NotImplementedException();
        }

        public void GetRuntimeEnvironment()
        {
            throw new System.NotImplementedException();
        }

        public void SetupRuntimes()
        {
            throw new System.NotImplementedException();
        }

        public void StageAppDirectory()
        {
            throw new System.NotImplementedException();
        }

        private void BindLocalRuntimes()
        {
            throw new System.NotImplementedException();
        }

        private void DownloadAppBits()
        {
            throw new System.NotImplementedException();
        }
    }
}
