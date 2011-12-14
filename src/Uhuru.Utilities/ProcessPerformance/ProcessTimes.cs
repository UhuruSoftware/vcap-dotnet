// -----------------------------------------------------------------------
// <copyright file="ProcessTimes.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.ProcessPerformance
{
    using System;
    using ComType = System.Runtime.InteropServices.ComTypes;
    
    /// <summary>
    /// holds the process time data. 
    /// </summary>
    internal struct ProcessTimes
    {
        /// <summary>
        /// Creation time of the process.
        /// </summary>
        public DateTime CreationTime;

        /// <summary>
        /// Exit time of the process.
        /// </summary>
        public DateTime ExitTime;

        /// <summary>
        /// Kernel CPU time the process spent.
        /// </summary>
        public DateTime KernelTime;

        /// <summary>
        /// User CPU time the process spent.
        /// </summary>
        public DateTime UserTime;

        /// <summary>
        /// Creation time of the process in its raw (native) format.
        /// </summary>
        public ComType.FILETIME RawCreationTime;
        
        /// <summary>
        /// Exit time of the process in its raw (native) format.
        /// </summary>
        public ComType.FILETIME RawExitTime;

        /// <summary>
        /// Kernel CPU time the process spent in its raw (native) format.
        /// </summary>
        public ComType.FILETIME RawKernelTime;

        /// <summary>
        /// User CPU time the process spent in its raw (native) format.
        /// </summary>
        public ComType.FILETIME RawUserTime;

        /// <summary>
        /// Converts the FILETIME fields to DateTime and stores the results in their respective properties.
        /// </summary>
        public void ConvertTime()
        {
            this.CreationTime = FiletimeToDateTime(this.RawCreationTime);
            this.ExitTime = FiletimeToDateTime(this.RawExitTime);
            this.KernelTime = FiletimeToDateTime(this.RawKernelTime);
            this.UserTime = FiletimeToDateTime(this.RawUserTime);
        }

        /// <summary>
        /// Filetimes a FILETIME to a DateTime.
        /// </summary>
        /// <param name="fileTime">The FILETIME object.</param>
        /// <returns>A DateTime object cointaining the converted value.</returns>
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
