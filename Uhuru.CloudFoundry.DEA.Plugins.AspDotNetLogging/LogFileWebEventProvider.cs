// -----------------------------------------------------------------------
// <copyright file="LogFileWebEventProvider.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Plugins.AspDotNetLogging
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Management;
    using System.Configuration;
    
    public class LogFileWebEventProvider : BufferedWebEventProvider
    {
        private string logFilePath;
        private string providerName, buffer, bufferMode;
        private StringBuilder customInfo;

        /// <summary>
        /// Initializes a new instance of the LogFileWebEventProvider class
        /// </summary>
        public LogFileWebEventProvider()
        {
           
        }
        
        /// <summary>
        /// the path where the log info will be saved
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

            this.providerName = name;
            this.buffer = this.LogFileUseBuffering.ToString();
            this.bufferMode = BufferMode;

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
                string extraInfo = String.Empty;

                if (eventRaised is WebBaseErrorEvent)
                {
                    extraInfo = ((WebBaseErrorEvent)eventRaised).ErrorException.ToString();
                }

                this.customInfo.AppendLine(String.Format(CultureInfo.InvariantCulture,
                    "Event Time (UTC):[{0}] Event Code:[{1}] Event Id:[{2}] Event Message:[{3}]",
                    eventRaised.EventTimeUtc, eventRaised.EventCode, eventRaised.EventID,
                    eventRaised.Message + " " + extraInfo));

                this.StoreToFile(FileMode.Append);
            }
        }
        
        public override void ProcessEventFlush(WebEventBufferFlushInfo flushInfo)
        {
            if (flushInfo == null)
            {
                throw new ArgumentNullException("flushInfo");
            }

            foreach (WebBaseEvent eventRaised in flushInfo.Events)
            {
                string extraInfo = String.Empty;

                if (eventRaised is WebBaseErrorEvent)
                {
                    extraInfo = ((WebBaseErrorEvent)eventRaised).ErrorException.ToString();
                }

                this.customInfo.AppendLine(String.Format(CultureInfo.InvariantCulture,
                    "Event Time (UTC):[{0}] Event Code:[{1}] Event Id:[{2}] Event Message:[{3}]",
                    eventRaised.EventTimeUtc, eventRaised.EventCode, eventRaised.EventID,
                    eventRaised.Message + " " + extraInfo));
            }

            this.StoreToFile(FileMode.Append);
        }

        /// <summary>
        /// shuts down the current object
        /// </summary>
        public override void Shutdown()
        {
            Flush();
        }

        /// <summary>
        /// saves the currently gathered info to a file
        /// </summary>
        /// <param name="fileMode">the file mode to use when opening the file</param>
        private void StoreToFile(FileMode fileMode)
        {
            int writeBlock;
            int startIndex;
            const int SEP_LEN = 70;

            writeBlock = customInfo.Length + SEP_LEN;
            startIndex = 0;

            FileStream logFileStream = new FileStream(logFilePath, fileMode, FileAccess.Write);
            logFileStream.Lock(startIndex, writeBlock);

            StreamWriter writer = new StreamWriter(logFileStream);

            writer.BaseStream.Seek(0, SeekOrigin.Current);
            writer.Write(customInfo.ToString());
            writer.Flush();

            logFileStream.Unlock(startIndex, writeBlock);

            writer.Close();
            logFileStream.Close();
        }

        private WebBaseEventCollection GetEvents(WebEventBufferFlushInfo flushInfo)
        {
            return flushInfo.Events;
        }

        private int GetEventsDiscardedSinceLastNotification(WebEventBufferFlushInfo flushInfo)
        {
            return flushInfo.EventsDiscardedSinceLastNotification;
        }

        private int GetEventsInBuffer(WebEventBufferFlushInfo flushInfo)
        {
            return flushInfo.EventsInBuffer;
        }

        private DateTime GetLastNotificationTime(WebEventBufferFlushInfo flushInfo)
        {
            return flushInfo.LastNotificationUtc;
        }

        private int GetNotificationSequence(WebEventBufferFlushInfo flushInfo)
        {
            return flushInfo.NotificationSequence;
        }

        private EventNotificationType GetNotificationType(WebEventBufferFlushInfo flushInfo)
        {
            return flushInfo.NotificationType;
        }
       
    }
}
