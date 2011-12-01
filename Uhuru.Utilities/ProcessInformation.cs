using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.Utilities
{
    public struct ProcessInformationEntry
    {
        int workset;
        string commandLine;
        int cpu;
        int parentProcess;
        string user;
        int processId;

        public int Workset
        {
            get { return workset; }
        }

        public string CommandLine
        {
            get { return commandLine; }
        }

        public int Cpu
        {
            get { return cpu; }
        }

        public int ParentProcess
        {
            get { return parentProcess; }
        }

        public string User
        {
            get { return user; }
        }

        public int ProcessId
        {
            get { return processId; }
        }

        public ProcessInformationEntry(int workset, string commandLine, int cpu, int parentProcess, string user, int processId)
        {
            this.workset = workset;
            this.commandLine = commandLine;
            this.cpu = cpu;
            this.parentProcess = parentProcess;
            this.user = user;
            this.processId = processId;
        }

    }

    public class ProcessInformation
    {
   

        public static ProcessInformationEntry[] GetProcessInformation(bool getWorkset, bool getCmd, bool getCPU, bool getParentProcess, bool getUser, bool getPid, int processId)
        {
            List<ProcessInformationEntry> result = new List<ProcessInformationEntry>();

            WindowsProcess.ProcessInformationEntry[] entries = WindowsProcess.ProcessInformation.GetProcessInformation(getWorkset, getCmd, getCPU, getParentProcess, getUser, getPid, processId);


            foreach (WindowsProcess.ProcessInformationEntry entry in entries)
            {
                if (entry.ProcessId != 0)
                {
                    ProcessInformationEntry newEntry = new ProcessInformationEntry(
                        entry.Workset, entry.CommandLine, entry.CPU, entry.ParentProcess, entry.User, entry.ProcessId);

                    result.Add(newEntry);
                }
            }

            return result.ToArray();
        }
    }
}
