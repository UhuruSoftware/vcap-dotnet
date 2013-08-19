namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    public class ProcessPrison
    {
        public string Id
        {
            get;
            private set;
        }

        public ProcessPrison()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public void Create()
        {
            Create(new ProcessPrisonCreateInfo());
        }

        public void Create(ProcessPrisonCreateInfo createInfo)
        {
        }

        public void Destroy()
        {
        }

        public void RunProcess(string executablePath)
        {
            var runInfo = new ProcessPrisonRunInfo() { ExecutablePath = executablePath };
            this.RunProcess(runInfo);
        }

        public void RunProcess(ProcessPrisonRunInfo runInfo)
        {

        }

        public Process[] GetRunningProcesses()
        {
            throw new NotImplementedException();
        }

    }
}
