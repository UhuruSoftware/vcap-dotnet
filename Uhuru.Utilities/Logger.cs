using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;

namespace Uhuru.Utilities
{
    /// <summary>
    /// This is a helper logger class that is used throughout the code.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Logs a fatal message.
        /// This indicates a really severe error, that will probably make the application crash.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Fatal(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Error);
        }

        /// <summary>
        /// Logs an error message.
        /// This indicates an error, but the application may be able to continue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Error(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Error);

        }

        /// <summary>
        /// Logs a warning message.
        /// This indicates a situation that could lead to some bad things.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Warning(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Warning);
        }

        /// <summary>
        /// Logs an information message.
        /// The message is used to indicate some progress.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Info(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Information);
        }

        /// <summary>
        /// Logs a debug message.
        /// This is an informational message, that is useful when debugging.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Debug(string message)
        {
            EventLog.WriteEntry("WinDEA", message, EventLogEntryType.Information);
        }

        /// <summary>
        /// Logs a fatal message and formats it.
        /// This indicates a really severe error, that will probably make the application crash.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Fatal(string message, params object[] args)
        {
            EventLog.WriteEntry("WinDEA", String.Format(CultureInfo.InvariantCulture, message, args), EventLogEntryType.Error);
        }

        /// <summary>
        /// Logs an error message and formats it.
        /// This indicates an error, but the application may be able to continue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Error(string message, params object[] args)
        {
            EventLog.WriteEntry("WinDEA", String.Format(CultureInfo.InvariantCulture, message, args), EventLogEntryType.Error);

        }

        /// <summary>
        /// Logs a warning message and formats it.
        /// This indicates a situation that could lead to some bad things.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Warning(string message, params object[] args)
        {
            EventLog.WriteEntry("WinDEA", String.Format(CultureInfo.InvariantCulture, message, args), EventLogEntryType.Warning);
        }

        /// <summary>
        /// Logs an information message and formats it.
        /// The message is used to indicate some progress.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Info(string message, params object[] args)
        {
            EventLog.WriteEntry("WinDEA", String.Format(CultureInfo.InvariantCulture, message, args), EventLogEntryType.Information);
        }

        /// <summary>
        /// Logs a debug message and formats it.
        /// This is an informational message, that is useful when debugging.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Debug(string message, params object[] args)
        {
            EventLog.WriteEntry("WinDEA", String.Format(CultureInfo.InvariantCulture, message, args), EventLogEntryType.Information);
        }
    }
}
