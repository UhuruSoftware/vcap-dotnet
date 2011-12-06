// -----------------------------------------------------------------------
// <copyright file="ProcessData.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
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
        private long oldUserTime;
        private long oldKernelTime;
        private DateTime oldUpdate;
        private int workingSet;

        /// <summary>
        /// Initializes a new instance of the ProcessData class
        /// </summary>
        /// <param name="id">the process ID</param>
        /// <param name="name">the name of the process</param>
        /// <param name="oldUserTime">the user time</param>
        /// <param name="oldKernelTime">the kernel time</param>
        internal ProcessData(int id, string name, long oldUserTime, long oldKernelTime)
        {
            this.ProcessId = id;
            this.Name = name;
            this.oldUserTime = oldUserTime;
            this.oldKernelTime = oldKernelTime;
            this.oldUpdate = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the process id.
        /// </summary>
        public int ProcessId
        {
            get { return this.processId; }
            set { this.processId = value; }
        }

        /// <summary>
        /// Gets or sets the name of the process.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or sets the CPU usage of the process, as a percentage.
        /// </summary>
        public int Cpu
        {
            get { return this.cpu; }
            set { this.cpu = value; }
        }

        /// <summary>
        /// Gets or sets the memory amount used by the process.
        /// </summary>
        public int WorkingSet
        {
            get { return this.workingSet; }
            set { this.workingSet = value; }
        }

        /// <summary>
        /// updates the cpu usage (cpu usage = UserTime + KernelTime) 
        /// </summary>
        /// <param name="newUserTime">the new user time</param>
        /// <param name="newKernelTime">the new kernel time</param>
        /// <returns>the raw usage</returns>
        internal int UpdateCpuUsage(long newUserTime, long newKernelTime)
        {
            long updateDelay;
            long userTime = newUserTime - this.oldUserTime;
            long kernelTime = newKernelTime - this.oldKernelTime;
            int rawUsage;

            // eliminates "divided by zero"
            if (DateTime.Now.Ticks == this.oldUpdate.Ticks)
            {
                Thread.Sleep(100);
            }

            updateDelay = DateTime.Now.Ticks - this.oldUpdate.Ticks;

            rawUsage = (int)(((userTime + kernelTime) * 100) / updateDelay);
            this.Cpu = rawUsage;

            this.oldUserTime = newUserTime;
            this.oldKernelTime = newKernelTime;
            this.oldUpdate = DateTime.Now;

            return rawUsage;
        }
    }
}
