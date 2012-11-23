// -----------------------------------------------------------------------
// <copyright file="Worker.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Uhuru.CloudFoundry.ServiceBase;
    using Uhuru.Configuration;
    using Uhuru.CloudFoundry.ServiceBase.Worker;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Worker : WorkerBase
    {
        public override void Start(ServiceElement options)
        {
            base.Start(options);
        }

        public void Stop()
        {
            base.Stop();
        }
    }
}
