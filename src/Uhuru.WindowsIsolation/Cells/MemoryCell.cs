﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uhuru.WindowsIsolation.Cells
{
    class MemoryCell : Cell
    {
        public override void Lockdown(Prison prison)
        {
            prison.JobObject.JobMemoryLimitBytes = prison.Rules.TotalPrivateMemoryLimitBytes;
            prison.JobObject.ActiveProcessesLimit = prison.Rules.RunningProcessesLimit;
        }

        public override void Destroy()
        {
        }

        public override CellInstanceInfo[] List()
        {
            return new CellInstanceInfo[0];
        }

        public override void Init()
        {
        }

        public override CellType GetFlag()
        {
            return CellType.Memory;
        }
    }
}
