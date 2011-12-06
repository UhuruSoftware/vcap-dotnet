// -----------------------------------------------------------------------
// <copyright file="DropletInstanceUsage.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using Uhuru.Utilities;

    public class DropletInstanceUsage : JsonConvertibleObject
    {
        [JsonName("mem")]
        public long MemoryKbytes
        {
            get;
            set;
        }

        [JsonName("cpu")]
        public long Cpu
        {
            get;
            set;
        }

        [JsonName("disk")]
        public long DiskBytes
        {
            get;
            set;
        }

        [JsonName("time")]
        public int TimeInterchangeableFormat
        {
            get { return Utils.DateTimeToEpochSeconds(Time); }
            set { Time = Utils.DateTimeFromEpochSeconds(value); }
        }

        public DateTime Time
        {
            get;
            set;
        }
    }
}
