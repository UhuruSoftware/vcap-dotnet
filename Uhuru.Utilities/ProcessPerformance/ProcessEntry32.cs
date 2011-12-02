using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Uhuru.Utilities.ProcessPerformance
{
    // holds the process info.
    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessEntry32
    {
        private uint size;
        private uint usage;
        private uint id;
        private IntPtr defaultHeapId;
        private uint moduleId;
        private uint threads;
        private uint parentProcessId;
        private int priorityClassBase;
        private uint flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private string exeFileName;

        public uint Size
        {
            get { return size; }
            set { size = value; }
        }

        public uint Usage
        {
            get { return usage; }
            set { usage = value; }
        }

        public uint Id
        {
            get { return id; }
            set { id = value; }
        }

        public IntPtr DefaultHeapId
        {
            get { return defaultHeapId; }
            set { defaultHeapId = value; }
        }

        public uint ModuleId
        {
            get { return moduleId; }
            set { moduleId = value; }
        }

        public uint Threads
        {
            get { return threads; }
            set { threads = value; }
        }

        public uint ParentProcessId
        {
            get { return parentProcessId; }
            set { parentProcessId = value; }
        }

        public int PriorityClassBase
        {
            get { return priorityClassBase; }
            set { priorityClassBase = value; }
        }

        public uint Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        public string ExeFileName
        {
            get { return exeFileName; }
            set { exeFileName = value; }
        }

        public override bool Equals(object obj)
        {
            ProcessEntry32 otherObject = (ProcessEntry32)obj;
            return id == otherObject.id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(ProcessEntry32 firstValue, ProcessEntry32 secondValue)
        {
            return firstValue.Equals(secondValue);
        }

        public static bool operator !=(ProcessEntry32 firstValue, ProcessEntry32 secondValue)
        {
            return !firstValue.Equals(secondValue);
        }
    }
}
