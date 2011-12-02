using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.Utilities.ProcessPerformance
{
    public struct ProcessInformationEntry
    {
        long workingSet;
        int cpu;
        int processId;

        public long WorkingSet
        {
            get
            {
                return workingSet;
            }
        }

        public int Cpu
        {
            get { return cpu; }
        }

        public int ProcessId
        {
            get { return processId; }
        }

        public ProcessInformationEntry(long workingSet, int cpu, int processId)
        {
            this.workingSet = workingSet;
            this.cpu = cpu;
            this.processId = processId;
        }


        public override bool Equals(object obj)
        {
            ProcessInformationEntry otherObject = (ProcessInformationEntry)obj;
            return processId == otherObject.processId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(ProcessInformationEntry firstValue, ProcessInformationEntry secondValue)
        {
            return firstValue.Equals(secondValue);
        }

        public static bool operator !=(ProcessInformationEntry firstValue, ProcessInformationEntry secondValue)
        {
            return !firstValue.Equals(secondValue);
        }
    }
}
