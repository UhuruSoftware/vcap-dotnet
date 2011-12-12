// -----------------------------------------------------------------------
// <copyright file="FileLogger.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Globalization;
    using System.IO;
    
    /// <summary>
    /// This is a helper logger class that writes to a file.
    /// </summary>
    public class FileLogger // TODO: vladi: make this use log4net
    {
        /// <summary>
        /// The path to the log file.
        /// </summary>
        private string fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogger"/> class.
        /// </summary>
        /// <param name="fileName">The file in which log events will be written.</param>
        public FileLogger(string fileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            this.fileName = fileName;
        }

        /// <summary>
        /// Logs a fatal message.
        /// This indicates a really severe error, that will probably make the application crash.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Fatal(string message)
        {
            File.AppendAllText(this.fileName, string.Format(CultureInfo.InvariantCulture, "[FATAL] [{0}] {1}\r\n", DateTime.Now, message));
        }

        /// <summary>
        /// Logs an error message.
        /// This indicates an error, but the application may be able to continue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Error(string message)
        {
            File.AppendAllText(this.fileName, string.Format(CultureInfo.InvariantCulture, "[ERROR] [{0}] {1}\r\n", DateTime.Now, message));
        }

        /// <summary>
        /// Logs a warning message.
        /// This indicates a situation that could lead to some bad things.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Warning(string message)
        {
            File.AppendAllText(this.fileName, string.Format(CultureInfo.InvariantCulture, "[WARN] [{0}] {1}\r\n", DateTime.Now, message));
        }

        /// <summary>
        /// Logs an information message.
        /// The message is used to indicate some progress.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Info(string message)
        {
            File.AppendAllText(this.fileName, string.Format(CultureInfo.InvariantCulture, "[INFO] [{0}] {1}\r\n", DateTime.Now, message));
        }

        /// <summary>
        /// Logs a debug message.
        /// This is an informational message, that is useful when debugging.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Debug(string message)
        {
            File.AppendAllText(this.fileName, string.Format(CultureInfo.InvariantCulture, "[DEBUG] [{0}] {1}\r\n", DateTime.Now, message));
        }

        /// <summary>
        /// Logs a fatal message and formats it.
        /// This indicates a really severe error, that will probably make the application crash.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public void Fatal(string message, params object[] args)
        {
            File.AppendAllText(this.fileName, string.Format(CultureInfo.InvariantCulture, "[FATAL] [{0}] {1}\r\n", DateTime.Now, string.Format(CultureInfo.InvariantCulture, message, args)));
        }

        /// <summary>
        /// Logs an error message and formats it.
        /// This indicates an error, but the application may be able to continue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public void Error(string message, params object[] args)
        {
            File.AppendAllText(this.fileName, string.Format(CultureInfo.InvariantCulture, "[ERROR] [{0}] {1}\r\n", DateTime.Now, string.Format(CultureInfo.InvariantCulture, message, args)));
        }

        /// <summary>
        /// Logs a warning message and formats it.
        /// This indicates a situation that could lead to some bad things.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public void Warning(string message, params object[] args)
        {
            File.AppendAllText(this.fileName, string.Format(CultureInfo.InvariantCulture, "[WARN] [{0}] {1}\r\n", DateTime.Now, string.Format(CultureInfo.InvariantCulture, message, args)));
        }

        /// <summary>
        /// Logs an information message and formats it.
        /// The message is used to indicate some progress.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public void Info(string message, params object[] args)
        {
            File.AppendAllText(this.fileName, string.Format(CultureInfo.InvariantCulture, "[INFO] [{0}] {1}\r\n", DateTime.Now, string.Format(CultureInfo.InvariantCulture, message, args)));
        }

        /// <summary>
        /// Logs a debug message and formats it.
        /// This is an informational message, that is useful when debugging.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public void Debug(string message, params object[] args)
        {
            File.AppendAllText(this.fileName, string.Format(CultureInfo.InvariantCulture, "[DEBUG] [{0}] {1}\r\n", DateTime.Now, string.Format(CultureInfo.InvariantCulture, message, args)));
        }
    }
}
