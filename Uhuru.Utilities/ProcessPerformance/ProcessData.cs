using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Uhuru.Utilities.ProcessPerformance
{

    // holds the process data
    public class ProcessData
    {
        private uint processId;
        private string name;
        private int cpu;
        private int index;

        public uint ProcessId
        {
            get { return processId; }
            set { processId = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Cpu
        {
            get { return cpu; }
            set { cpu = value; }
        }

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        long oldUserTime;
        long oldKernelTime;
        DateTime oldUpdate;

        public ProcessData(uint id, string name, long oldUserTime, long oldKernelTime)
        {
            this.ProcessId = id;
            this.Name = name;
            this.oldUserTime = oldUserTime;
            this.oldKernelTime = oldKernelTime;
            oldUpdate = DateTime.Now;
        }

        public int UpdateCpuUsage(long newUserTime, long newKernelTime)
        {
            // updates the cpu usage (cpu usgae = UserTime + KernelTime)
            long updateDelay;
            long userTime = newUserTime - oldUserTime;
            long kernelTime = newKernelTime - oldKernelTime;
            int rawUsage;

            // eliminates "divided by zero"
            if (DateTime.Now.Ticks == oldUpdate.Ticks) Thread.Sleep(100);

            updateDelay = DateTime.Now.Ticks - oldUpdate.Ticks;

            rawUsage = (int)(((userTime + kernelTime) * 100) / updateDelay);
            //CpuUsage = ((UserTime + KernelTime) * 100) / UpdateDelay + "%";
            Cpu = rawUsage;

            oldUserTime = newUserTime;
            oldKernelTime = newKernelTime;
            oldUpdate = DateTime.Now;

            return rawUsage;
        }
    }
}
