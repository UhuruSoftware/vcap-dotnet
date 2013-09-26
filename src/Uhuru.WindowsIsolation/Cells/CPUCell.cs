using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uhuru.WindowsIsolation.Cells
{
    class CPUCell : Cell
    {
        public override void Lockdown(Prison prison)
        {
            throw new NotImplementedException();
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
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
            return CellType.CPU;
        }
    }
}
