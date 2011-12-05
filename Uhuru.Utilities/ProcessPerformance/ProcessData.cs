// -----------------------------------------------------------------------
// <copyright file="ProcessData.cs" company="Uhuru Software">
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.ProcessPerformance
{
    using System;
    using System.Threading;

    /// <summary>
    /// This class contains process data.
    /// </summary>
    public class ProcessData
    {
        private int processId;
        private string name;
        private int cpu;
        long oldUserTime;
        long oldKernelTime;
        DateTime oldUpdate;
        int workingSet;

        /// <summary>
        /// The process id.
        /// </summary>
        public int ProcessId
        {
            get { return processId; }
            set { processId = value; }
        }

        /// <summary>
        /// The name of the process.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Cpu usage of the process, as a procent.
        /// </summary>
        public int Cpu
        {
            get { return cpu; }
            set { cpu = value; }
        }

        /// <summary>
        /// Memory used by the process.
        /// </summary>
        public int WorkingSet
        {
            get { return workingSet; }
            set { workingSet = value; }
        }

        internal ProcessData(int id, string name, long oldUserTime, long oldKernelTime)
        {
            this.ProcessId = id;
            this.Name = name;
            this.oldUserTime = oldUserTime;
            this.oldKernelTime = oldKernelTime;
            oldUpdate = DateTime.Now;
        }

        internal int UpdateCpuUsage(long newUserTime, long newKernelTime)
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
            Cpu = rawUsage;

            oldUserTime = newUserTime;
            oldKernelTime = newKernelTime;
            oldUpdate = DateTime.Now;

            return rawUsage;
        }
    }
}
