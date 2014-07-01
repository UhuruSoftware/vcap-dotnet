// -----------------------------------------------------------------------
// <copyright file="LogFileWebEventProvider.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Plugins.AspDotNetLogging
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Management;

    /// <summary>
    /// Event provider for droplet instances. Writes events to a file.
    /// </summary>
    public class LogFileWebEventProvider : BufferedWebEventProvider
    {
        /// <summary>
        /// The path to the log file.
        /// </summary>
        private string logFilePath;

        /// <summary>
        /// Buffer log data.
        /// </summary>
        private StringBuilder customInfo;

        /// <summary>
        /// Initializes a new instance of the LogFileWebEventProvider class
        /// </summary>
        public LogFileWebEventProvider()
        {
        }

        /// <summary>
        /// Gets or sets the path where the log info will be saved
        /// </summary>
        public string LogFilePath
        {
            get { return this.logFilePath; }
            set { this.logFilePath = value; }
        }

        /// <summary>
        /// Gets a value indicating whether buffering will be used or not
        /// </summary>
        public bool LogFileUseBuffering
        {
            get { return this.UseBuffering; }
        }

        /// <summary>
        /// Gets a value indicating the buffering mode of the provider
        /// </summary>
        public string LogFileBufferMode
        {
            get { return this.BufferMode; }
        }

        /// <summary>
        /// Moves the events from the provider's buffer into the event log.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// provides the initial settings to the object
        /// </summary>
        /// <param name="name">the provider name</param>
        /// <param name="config">the rest of the attributes we want to assign</param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            if (this.Name == "ErrorEventProvider")
            {
                this.logFilePath = ConfigurationManager.AppSettings["UHURU_ERROR_LOG_FILE"];
            }
            else
            {
                this.logFilePath = ConfigurationManager.AppSettings["UHURU_LOG_FILE"];
            }

            this.customInfo = new StringBuilder();
        }

        /// <summary>
        /// process a raised event
        /// </summary>
        /// <param name="eventRaised">the event to be processed</param>
        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            if (eventRaised == null)
            {
                throw new ArgumentNullException("eventRaised");
            }

            if (UseBuffering)
            {
                base.ProcessEvent(eventRaised);
            }
            else
            {
                string extraInfo = string.Empty;

                WebBaseErrorEvent errorEvent = eventRaised as WebBaseErrorEvent;

                if (errorEvent != null)
                {
                    extraInfo = errorEvent.ErrorException.ToString();
                }

                this.customInfo.AppendLine(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "Event Time (UTC):[{0}] Event Code:[{1}] Event Id:[{2}] Event Message:[{3}]",
                    eventRaised.EventTimeUtc,
                    eventRaised.EventCode,
                    eventRaised.EventID,
                    eventRaised.Message + " " + extraInfo));

                this.StoreToFile(FileMode.Append);
            }
        }

        /// <summary>
        /// Processes the buffered events.
        /// </summary>
        /// <param name="flushInfo">A <see cref="T:System.Web.Management.WebEventBufferFlushInfo"/> that contains buffering information.</param>
        public override void ProcessEventFlush(WebEventBufferFlushInfo flushInfo)
        {
            if (flushInfo == null)
            {
                throw new ArgumentNullException("flushInfo");
            }

            foreach (WebBaseEvent eventRaised in flushInfo.Events)
            {
                string extraInfo = string.Empty;
                WebBaseErrorEvent webBasedErrorEvent = eventRaised as WebBaseErrorEvent;

                if (webBasedErrorEvent != null)
                {
                    extraInfo = webBasedErrorEvent.ErrorException.ToString();
                }

                string line = string.Format(
                    CultureInfo.InvariantCulture,
                    "Event Time (UTC):[{0}] Event Code:[{1}] Event Id:[{2}] Event Message:[{3}]",
                    eventRaised.EventTimeUtc, 
                    eventRaised.EventCode, 
                    eventRaised.EventID,
                    eventRaised.Message + " " + extraInfo);
                
                if (this.Name == "ErrorEventProvider")
                {
                    Console.Error.WriteLine(line);                    
                }
                else
                {
                    Console.Out.WriteLine(line);
                }
            }            
        }

        /// <summary>
        /// shuts down the current object
        /// </summary>
        public override void Shutdown()
        {
            this.Flush();
        }

        /// <summary>
        /// saves the currently gathered info to a file
        /// </summary>
        /// <param name="fileMode">the file mode to use when opening the file</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "The object is only disposed once.")]
        private void StoreToFile(FileMode fileMode)
        {
            int writeBlock;
            int startIndex;
            const int SEP_LEN = 70;

            writeBlock = this.customInfo.Length + SEP_LEN;
            startIndex = 0;

            FileStream logFileStream = null;
            try
            {
                logFileStream = new FileStream(this.logFilePath, fileMode, FileAccess.Write);

                using (StreamWriter writer = new StreamWriter(logFileStream))
                {
                    logFileStream.Lock(startIndex, writeBlock);
                    writer.BaseStream.Seek(0, SeekOrigin.Current);
                    writer.Write(this.customInfo.ToString());
                    writer.Flush();
                    logFileStream.Unlock(startIndex, writeBlock);
                    logFileStream = null;
                }
            }
            finally
            {
                if (logFileStream != null)
                {
                    logFileStream.Close();
                }
            }
        }
    }
}
