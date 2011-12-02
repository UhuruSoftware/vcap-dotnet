using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Uhuru.Utilities
{
    public static class Logger
    {
        public static void Fatal(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Error);
        }

        public static void Error(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Error);

        }

        public static void Warning(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Warning);
        }

        public static void Info(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Information);
        }

        public static void Debug(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Information);
        }
    }
}
