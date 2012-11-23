// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase.Worker.AsyncJob
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
using Uhuru.Configuration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class Config
    {
        public static ResqueElement RedisConfig { get; set; }
        public static string TempFolder { get; set; }
    }
}
