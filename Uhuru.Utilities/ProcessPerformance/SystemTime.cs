// -----------------------------------------------------------------------
// <copyright file="SystemTime.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.ProcessPerformance
{
    using System.Runtime.InteropServices;
    
    /// <summary>
    /// A class to hold the properties of a particular time.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemTime
    {
        /// <summary>
        /// the year
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        private short year;
        
        /// <summary>
        /// the month
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        private short month;
        
        /// <summary>
        /// the day of the week
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        private short dayOfWeek;
        
        /// <summary>
        /// the day of the month
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        private short day;
        
        /// <summary>
        /// the hour
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        private short hour;
        
        /// <summary>
        /// the minute
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        private short minute;
        
        /// <summary>
        /// the second
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        private short second;
        
        /// <summary>
        /// the milliseconds
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        private short milliseconds;

        /// <summary>
        /// overload of == for this particular type
        /// </summary>
        /// <param name="firstValue">the first parameter</param>
        /// <param name="secondValue">the second parameter</param>
        /// <returns>the intended result of ==</returns>
        public static bool operator ==(SystemTime firstValue, SystemTime secondValue)
        {
            return firstValue.Equals(secondValue);
        }

        /// <summary>
        /// overload of != for this particular type
        /// </summary>
        /// <param name="firstValue">the first parameter</param>
        /// <param name="secondValue">the second parameter</param>
        /// <returns>the intended result of !=</returns>
        public static bool operator !=(SystemTime firstValue, SystemTime secondValue)
        {
            return !firstValue.Equals(secondValue);
        }

        /// <summary>
        /// determines whether the current instance equals another object or not
        /// </summary>
        /// <param name="obj">the object to compare the current instance to</param>
        /// <returns>the result of the comparison</returns>
        public override bool Equals(object obj)
        {
            SystemTime otherObject = (SystemTime)obj;
            return this.year == otherObject.year &&
                this.month == otherObject.month &&
                this.dayOfWeek == otherObject.dayOfWeek &&
                this.day == otherObject.day &&
                this.hour == otherObject.hour &&
                this.minute == otherObject.minute &&
                this.second == otherObject.second &&
                this.milliseconds == otherObject.milliseconds;
        }

        /// <summary>
        /// returns the hashcode for this instance
        /// </summary>
        /// <returns>the requested hashcode</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
