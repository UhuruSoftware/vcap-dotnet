// -----------------------------------------------------------------------
// <copyright file="ProcessInformation.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.ProcessPerformance
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security;
    using System.Threading;
    
    /// <summary>
    /// This is a helper class used to get CPU and memory usage information for processes.
    /// </summary>
    public static class ProcessInformation
    {
        /// <summary>
        /// Number of snapshots to take.
        /// </summary>
        private const int SnapshotCount = 2;

        /// <summary>
        /// Gets process usage information for a process.
        /// </summary>
        /// <param name="processId">The process id for which to get the data.</param>
        /// <returns>A ProcessData object containing metrics.</returns>
        public static ProcessData GetProcessUsage(int processId)
        {
            ProcessData returnValue = null;
            ProcessTimes processTimes = new ProcessTimes();
            IntPtr processHandle = NativeMethods.ProcessHandleError;
            int snapshotIndex = 0;

            while (snapshotIndex < SnapshotCount)
            {
                Process currentProcess = Process.GetProcessById(processId);

                try
                {
                    processHandle = NativeMethods.OpenProcess(NativeMethods.ProcessAllAccess, false, (uint)currentProcess.Id);
                    bool snapshotSucceeded = NativeMethods.GetProcessTimes(processHandle, out processTimes.RawCreationTime, out processTimes.RawExitTime, out processTimes.RawKernelTime, out processTimes.RawUserTime);

                    if (snapshotSucceeded)
                    {
                        processTimes.ConvertTime();

                        if (returnValue != null)
                        {
                            returnValue.UpdateCpuUsage(processTimes.UserTime.Ticks, processTimes.KernelTime.Ticks);
                        }
                        else
                        {
                            returnValue = new ProcessData(currentProcess.Id, currentProcess.ProcessName, processTimes.UserTime.Ticks, processTimes.KernelTime.Ticks, currentProcess.WorkingSet64);
                        }

                        snapshotIndex++;
                    }
                }
                finally
                {
                    if (processHandle != NativeMethods.ProcessHandleError)
                    {
                        NativeMethods.CloseHandle(processHandle);
                    }
                }

                Thread.Sleep(250);
            }

            return returnValue;
        }

        /// <summary>
        /// Gets process usage information for the processes on the local machine.
        /// </summary>
        /// <returns>An array of ProcessData objects.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Method is marked SecurityCritical, but the warning is still generated"), 
        SecurityCritical]
        public static ProcessData[] GetProcessUsage()
        {
            Dictionary<int, ProcessData> processes = new Dictionary<int, ProcessData>();
            ProcessTimes processTimes = new ProcessTimes();
            IntPtr processHandle = NativeMethods.ProcessHandleError;
            ProcessData currentProcessData;
            int total = 0;
            int retryCount = 0;

            for (int snapshotIndex = 0; snapshotIndex < SnapshotCount; snapshotIndex++)
            {
                Process[] currentProcesses = Process.GetProcesses();

                foreach (Process process in currentProcesses)
                {
                    try
                    {
                        processHandle = NativeMethods.OpenProcess(NativeMethods.ProcessAllAccess, false, (uint)process.Id);
                        if (processHandle != NativeMethods.ProcessHandleError && processHandle != NativeMethods.ProcessHandleZero)
                        {
                            bool snapshotSucceeded = NativeMethods.GetProcessTimes(processHandle, out processTimes.RawCreationTime, out processTimes.RawExitTime, out processTimes.RawKernelTime, out processTimes.RawUserTime);
                            retryCount = 0;

                            // spend a max. of 2 seconds trying to get the usage for a process
                            while (!snapshotSucceeded && retryCount < 8)
                            {
                                retryCount++;
                                snapshotSucceeded = NativeMethods.GetProcessTimes(processHandle, out processTimes.RawCreationTime, out processTimes.RawExitTime, out processTimes.RawKernelTime, out processTimes.RawUserTime);
                            }

                            processTimes.ConvertTime();

                            if (processes.TryGetValue(process.Id, out currentProcessData))
                            {
                                total += currentProcessData.UpdateCpuUsage(processTimes.UserTime.Ticks, processTimes.KernelTime.Ticks);
                            }
                            else
                            {
                                processes[process.Id] = new ProcessData(process.Id, process.ProcessName, processTimes.UserTime.Ticks, processTimes.KernelTime.Ticks, process.WorkingSet64);
                            }
                        }
                    }
                    finally
                    {
                        if (processHandle != NativeMethods.ProcessHandleError)
                        {
                            NativeMethods.CloseHandle(processHandle);
                        }
                    }
                }

                for (int i = 0; i < processes.Count; i++)
                {
                    if (!currentProcesses.Any(p => p.Id == processes.Keys.ElementAt(i)))
                    {
                        processes.Remove(processes.Keys.ElementAt(i));
                    }
                }

                Thread.Sleep(250);
            }

            return processes.Values.ToArray();
        }
    }
}
