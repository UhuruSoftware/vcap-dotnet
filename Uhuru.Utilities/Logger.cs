using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using log4net;
using System.Reflection;

namespace Uhuru.Utilities
{
    /// <summary>
    /// This is a helper logger class that is used throughout the code.
    /// </summary>
    public static class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(System.AppDomain.CurrentDomain.FriendlyName);
        private static bool isSourceConfigured = false;
        private static readonly object configureEventLogSourceLock = new object();

        private static void SetEventLogSource()
        {
            if (!isSourceConfigured)
            {
                lock (configureEventLogSourceLock)
                {
                    if (!isSourceConfigured)
                    {
                        isSourceConfigured = true;
                        EventLog.CreateEventSource(System.AppDomain.CurrentDomain.FriendlyName, ((log4net.Appender.EventLogAppender)log.Logger.Repository.GetAppenders().Single(a => a.Name == "EventLogAppender")).LogName);
                        ((log4net.Appender.EventLogAppender)log.Logger.Repository.GetAppenders().Single(a => a.Name == "EventLogAppender")).ApplicationName = System.AppDomain.CurrentDomain.FriendlyName;
                        ((log4net.Appender.EventLogAppender)log.Logger.Repository.GetAppenders().Single(a => a.Name == "EventLogAppender")).ActivateOptions();
                    }
                }
            }
        }

        /// <summary>
        /// Logs a fatal message.
        /// This indicates a really severe error, that will probably make the application crash.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Fatal(string message)
        {
            SetEventLogSource();
            log.Fatal(message);
        }

        /// <summary>
        /// Logs an error message.
        /// This indicates an error, but the application may be able to continue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Error(string message)
        {
            SetEventLogSource();
            log.Error(message);
        }

        /// <summary>
        /// Logs a warning message.
        /// This indicates a situation that could lead to some bad things.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Warning(string message)
        {
            SetEventLogSource();
            log.Warn(message);
        }

        /// <summary>
        /// Logs an information message.
        /// The message is used to indicate some progress.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Info(string message)
        {
            SetEventLogSource();
            log.Info(message);
        }

        /// <summary>
        /// Logs a debug message.
        /// This is an informational message, that is useful when debugging.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Debug(string message)
        {
            SetEventLogSource();
            log.Debug(message);
        }

        /// <summary>
        /// Logs a fatal message and formats it.
        /// This indicates a really severe error, that will probably make the application crash.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Fatal(string message, params object[] args)
        {
            SetEventLogSource();
            log.FatalFormat(CultureInfo.InvariantCulture, message, args);
        }

        /// <summary>
        /// Logs an error message and formats it.
        /// This indicates an error, but the application may be able to continue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Error(string message, params object[] args)
        {
            SetEventLogSource();
            log.ErrorFormat(CultureInfo.InvariantCulture, message, args);
        }

        /// <summary>
        /// Logs a warning message and formats it.
        /// This indicates a situation that could lead to some bad things.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Warning(string message, params object[] args)
        {
            SetEventLogSource();
            log.WarnFormat(CultureInfo.InvariantCulture, message, args);
        }

        /// <summary>
        /// Logs an information message and formats it.
        /// The message is used to indicate some progress.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Info(string message, params object[] args)
        {
            SetEventLogSource();
            log.InfoFormat(CultureInfo.InvariantCulture, message, args);
        }

        /// <summary>
        /// Logs a debug message and formats it.
        /// This is an informational message, that is useful when debugging.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Debug(string message, params object[] args)
        {
            SetEventLogSource();
            log.DebugFormat(CultureInfo.InvariantCulture, message, args);
        }
    }
}
