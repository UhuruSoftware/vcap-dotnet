using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSecurityTest
{
    interface ISecurityTest
    {
        bool IsLowerScoreBetter
        {
            get;
        }

        string Description
        {
            get;
        }

        string Metric
        {
            get;
        }

        void RunTest();
    }
}
