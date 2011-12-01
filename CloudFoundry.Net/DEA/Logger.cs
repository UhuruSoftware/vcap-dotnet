using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace CloudFoundry.Net.DEA
{
    class Logger
    {
        public static void fatal(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Error);
        }

        public static void error(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Error);

        }

        public static void warn(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Warning);
        }

        public static void info(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Information);
        }

        public static void debug(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Information);
        }


        private string filename;

        public Logger(string filename)
        {
            this.filename = filename;
        }

        public void ffatal(string message)
        {
            File.AppendAllText(filename, String.Format("[FATAL] {0}\r\n", message));
        }

        public void ferror(string message)
        {
            File.AppendAllText(filename, String.Format("[ERROR] {0}\r\n", message));
        }

        public void fwarn(string message)
        {
            File.AppendAllText(filename, String.Format("[WARN] {0}\r\n", message));
        }

        public void finfo(string message)
        {
            File.AppendAllText(filename, String.Format("[INFO] {0}\r\n", message));
        }

        public void fdebug(string message)
        {
            File.AppendAllText(filename, String.Format("[DEBUG] {0}\r\n", message));
        }
    }
}
