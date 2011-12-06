using System;
using System.Runtime.InteropServices;

namespace Uhuru.Utilities.ProcessPerformance
{
    // holds the process info.
    [StructLayout(LayoutKind.Sequential)]
    internal struct ProcessEntry32
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
