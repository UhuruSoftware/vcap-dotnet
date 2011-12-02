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
    public class ProcessInformation
    {
        const ProcessData PROCESS_DATA_NOT_FOUND = null;
        ArrayList processDataList = new ArrayList();
        ArrayList idList = new ArrayList();

        private ProcessInformation()
        {

        }

        [SecurityCritical]
        public static ProcessInformationEntry[] GetProcessInformation(int processId)
        {
            List<ProcessInformationEntry> result = new List<ProcessInformationEntry>();

            ProcessInformation perf = new ProcessInformation();
            perf.GetUsage();
            Thread.Sleep(250);
            perf.GetUsage();

            foreach (ProcessData entry in perf.processDataList)
            {
                if (entry.ProcessId != 0)
                {
                    if (processId != 0)
                    {
                        if (processId == entry.ProcessId)
                        {
                            return new ProcessInformationEntry[] { new ProcessInformationEntry(
                            Process.GetProcessById((int)entry.ProcessId).WorkingSet64,
                            (int)entry.Cpu, (int)entry.ProcessId)};
                        }
                    }
                    else
                    {
                        ProcessInformationEntry newEntry = new ProcessInformationEntry(
                            Process.GetProcessById((int)entry.ProcessId).WorkingSet64,
                            (int)entry.Cpu, (int)entry.ProcessId);
                        result.Add(newEntry);
                    }
                }
            }

            return result.ToArray();
        }

        // this is where it all happens
        public void GetUsage()
        {

            ProcessEntry32 ProcessInfo = new ProcessEntry32();
            ProcessTimes ProcessTimes = new ProcessTimes();
            IntPtr ProcessList, ProcessHandle = NativeMethods.PROCESS_HANDLE_ERROR;
            ProcessData CurrentProcessData;
            int Index;
            int Total = 0;
            bool NoError;

            // this creates a pointer to the current process list
            ProcessList = NativeMethods.CreateToolhelp32Snapshot(NativeMethods.TH32CS_SNAPPROCESS, 0);

            if (ProcessList == NativeMethods.PROCESS_LIST_ERROR) { return; }

            // we usage Process32First, Process32Next to loop threw the processes
            ProcessInfo.Size = NativeMethods.PROCESS_ENTRY_32_SIZE;
            NoError = NativeMethods.Process32First(ProcessList, ref ProcessInfo);
            idList.Clear();

            while (NoError)
                try
                {
                    // we need a process handle to pass it to GetProcessTimes function
                    // the OpenProcess function will provide us the handle by the id
                    ProcessHandle = NativeMethods.OpenProcess(NativeMethods.PROCESS_ALL_ACCESS, false, ProcessInfo.Id);

                    // here's what we are looking for, this gets the kernel and user time
                    NativeMethods.GetProcessTimes(
                        ProcessHandle,
                        out ProcessTimes.RawCreationTime,
                        out ProcessTimes.RawExitTime,
                        out ProcessTimes.RawKernelTime,
                        out ProcessTimes.RawUserTime);

                    // convert the values to DateTime values
                    ProcessTimes.ConvertTime();

                    //from here is just managing the gui for the process list
                    CurrentProcessData = ProcessExists(ProcessInfo.Id);
                    idList.Add(ProcessInfo.Id);

                    if (CurrentProcessData == PROCESS_DATA_NOT_FOUND)
                    {
                        Index = processDataList.Add(new ProcessData(
                            ProcessInfo.Id,
                            ProcessInfo.ExeFileName,
                            ProcessTimes.UserTime.Ticks,
                            ProcessTimes.KernelTime.Ticks));
                    }
                    else
                        Total += CurrentProcessData.UpdateCpuUsage(
                                    ProcessTimes.UserTime.Ticks,
                                    ProcessTimes.KernelTime.Ticks);
                }
                finally
                {
                    if (ProcessHandle != NativeMethods.PROCESS_HANDLE_ERROR)
                        NativeMethods.CloseHandle(ProcessHandle);

                    NoError = NativeMethods.Process32Next(ProcessList, ref ProcessInfo);
                }

            NativeMethods.CloseHandle(ProcessList);

            Index = 0;

            while (Index < processDataList.Count)
            {
                ProcessData TempProcess = (ProcessData)processDataList[Index];

                if (idList.Contains(TempProcess.ProcessId))
                    Index++;
                else
                {
                    processDataList.RemoveAt(Index);
                }
            }
        }

        private ProcessData ProcessExists(uint ID)
        {
            foreach (ProcessData TempProcess in processDataList)
                if (TempProcess.ProcessId == ID) return TempProcess;

            return PROCESS_DATA_NOT_FOUND;
        }
    }
}
