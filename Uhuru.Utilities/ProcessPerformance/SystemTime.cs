using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Uhuru.Utilities.ProcessPerformance
{
    //holds the time data
    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemTime
    {
        [MarshalAs(UnmanagedType.U2)]
        short year;
        [MarshalAs(UnmanagedType.U2)]
        short month;
        [MarshalAs(UnmanagedType.U2)]
        short dayOfWeek;
        [MarshalAs(UnmanagedType.U2)]
        short day;
        [MarshalAs(UnmanagedType.U2)]
        short hour;
        [MarshalAs(UnmanagedType.U2)]
        short minute;
        [MarshalAs(UnmanagedType.U2)]
        short second;
        [MarshalAs(UnmanagedType.U2)]
        short milliseconds;

        public override bool Equals(object obj)
        {
            SystemTime otherObject = (SystemTime)obj;
            return year == otherObject.year &&
                month == otherObject.month &&
                dayOfWeek == otherObject.dayOfWeek &&
                day == otherObject.day &&
                hour == otherObject.hour &&
                minute == otherObject.minute &&
                second == otherObject.second &&
                milliseconds == otherObject.milliseconds;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(SystemTime firstValue, SystemTime secondValue)
        {
            return firstValue.Equals(secondValue);
        }

        public static bool operator !=(SystemTime firstValue, SystemTime secondValue)
        {
            return !firstValue.Equals(secondValue);
        }
    }
}
