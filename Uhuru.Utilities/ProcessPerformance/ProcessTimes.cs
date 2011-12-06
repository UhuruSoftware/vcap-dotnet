// -----------------------------------------------------------------------
// <copyright file="ProcessData.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.ProcessPerformance
{
    using System;
    using ComType = System.Runtime.InteropServices.ComTypes;
    
    // holds the process time data.
    struct ProcessTimes
    {
        public DateTime CreationTime, ExitTime, KernelTime, UserTime;
        public ComType.FILETIME RawCreationTime, RawExitTime, RawKernelTime, RawUserTime;

        public void ConvertTime()
        {
            this.CreationTime = FiletimeToDateTime(this.RawCreationTime);
            this.ExitTime = FiletimeToDateTime(this.RawExitTime);
            this.KernelTime = FiletimeToDateTime(this.RawKernelTime);
            this.UserTime = FiletimeToDateTime(this.RawUserTime);
        }

        private static DateTime FiletimeToDateTime(ComType.FILETIME fileTime)
        {
            try
            {
                if (fileTime.dwLowDateTime < 0)
                {
                    fileTime.dwLowDateTime = 0;
                }

                if (fileTime.dwHighDateTime < 0)
                { 
                    fileTime.dwHighDateTime = 0; 
                }

                long rawFileTime = (((long)fileTime.dwHighDateTime) << 32) + fileTime.dwLowDateTime;
                return DateTime.FromFileTimeUtc(rawFileTime);
            }
            catch (ArgumentOutOfRangeException) 
            { 
                return new DateTime(); 
            }
        }
    }
}
