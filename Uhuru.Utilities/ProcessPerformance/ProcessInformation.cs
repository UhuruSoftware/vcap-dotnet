using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Security.Permissions;
using System.Security;

namespace Uhuru.Utilities.ProcessPerformance
{
    /// <summary>
    /// This is a helper class used to get CPU and memory usage information for processes.
    /// </summary>
    public static class ProcessInformation
    {
        const int SNAPSHOT_COUNT = 2;

        /// <summary>
        /// Gets process usage information for a process.
        /// </summary>
        /// <param name="processId">The process id for which to get the data.</param>
        /// <returns>A ProcessData object containing metrics.</returns>
        public static ProcessData GetProcessUsage(int processId)
        {
            return GetProcessUsage().FirstOrDefault(process => process.ProcessId == processId);
        }

        /// <summary>
        /// Gets process usage information for the processes on the local machine.
        /// </summary>
        /// <returns>An array of ProcessData objects.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SecurityCritical]
        public static ProcessData[] GetProcessUsage()
        {
            Dictionary<int, ProcessData> processes = new Dictionary<int, ProcessData>();
            ProcessTimes ProcessTimes = new ProcessTimes();
            IntPtr ProcessHandle = NativeMethods.PROCESS_HANDLE_ERROR;
            ProcessData CurrentProcessData;
            int Total = 0;

            for (int snapshotIndex = 0; snapshotIndex < SNAPSHOT_COUNT; snapshotIndex++)
            {
                Process[] currentProcesses = Process.GetProcesses();

                foreach (Process process in currentProcesses)
                {
                    try
                    {
                        ProcessHandle = NativeMethods.OpenProcess(NativeMethods.PROCESS_ALL_ACCESS, false, (uint)process.Id);
                        NativeMethods.GetProcessTimes(ProcessHandle, out ProcessTimes.RawCreationTime, out ProcessTimes.RawExitTime,
                            out ProcessTimes.RawKernelTime, out ProcessTimes.RawUserTime);

                        ProcessTimes.ConvertTime();

                        if (processes.TryGetValue(process.Id, out CurrentProcessData))
                        {
                            Total += CurrentProcessData.UpdateCpuUsage(ProcessTimes.UserTime.Ticks, ProcessTimes.KernelTime.Ticks);
                        }
                        else
                        {
                            processes[process.Id] = new ProcessData(process.Id, process.ProcessName,
                                ProcessTimes.UserTime.Ticks, ProcessTimes.KernelTime.Ticks);
                        }
                    }
                    finally
                    {
                        if (ProcessHandle != NativeMethods.PROCESS_HANDLE_ERROR)
                        {
                            NativeMethods.CloseHandle(ProcessHandle);
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
