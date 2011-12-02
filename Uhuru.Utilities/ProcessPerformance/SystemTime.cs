using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Uhuru.Utilities.ProcessPerformance
{
    //holds the time data
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
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

        public short Year
        {
            get { return year; }
            set { year = value; }
        }

        public short Month
        {
            get { return month; }
            set { month = value; }
        }

        public short DayOfWeek
        {
            get { return dayOfWeek; }
            set { dayOfWeek = value; }
        }

        public short Day
        {
            get { return day; }
            set { day = value; }
        }

        public short Hour
        {
            get { return hour; }
            set { hour = value; }
        }

        public short Minute
        {
            get { return minute; }
            set { minute = value; }
        }

        public short Second
        {
            get { return second; }
            set { second = value; }
        }

        public short Milliseconds
        {
            get { return milliseconds; }
            set { milliseconds = value; }
        }

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
