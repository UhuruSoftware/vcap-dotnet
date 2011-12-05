using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Management;
using Microsoft.Web.Administration;
using System.DirectoryServices;
using System.IO;
using System.Globalization;


namespace Uhuru.Autowiring
{
    public class LogFileWebEventProvider : BufferedWebEventProvider
    {
        private string logFilePath;

        public string LogFilePath
        {
            get { return logFilePath; }
            set { logFilePath = value; }
        }
        private FileStream logFs;
        private string providerName, buffer, bufferMode;
        private StringBuilder customInfo;

        public bool LogFileUseBuffering
        {
            get { return UseBuffering; }
        }

        public string LogFileBufferMode
        {
            get { return BufferMode; }
        }

        public LogFileWebEventProvider()
        {
            logFilePath = @"c:\Users\Public\Documents\WebErrorsLog.txt";

            customInfo = new StringBuilder();
        }

        public override void Flush()
        {
            customInfo.AppendLine("Perform Custom Flush");
            
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            providerName = name;
            buffer = LogFileUseBuffering.ToString();
            bufferMode = BufferMode;

            customInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, "Provider name: {0}", providerName));
            customInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, "Buffering: {0}", buffer));
            customInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, "Buffer mode: {0}", bufferMode));
        }

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
                customInfo.AppendLine("*** Buffering disabled ***");
                customInfo.AppendLine(eventRaised.ToString());
                StoreToFile(FileMode.Append);
                
            }
        }

        private void StoreToFile(FileMode fileMode)
        {
            int writeBlock;
            int startIndex;
            const int SEP_LEN = 70;

            writeBlock = customInfo.Length + SEP_LEN;
            startIndex = 0;

            logFs = new FileStream(logFilePath, fileMode, FileAccess.Write);

            logFs.Lock(startIndex, writeBlock);

            StreamWriter writer = new StreamWriter(logFs);

            writer.BaseStream.Seek(0, SeekOrigin.Current);

            writer.Write(customInfo.ToString());

            writer.WriteLine(new string('*', SEP_LEN));

            writer.Flush();

            logFs.Unlock(startIndex, writeBlock);

            writer.Close();

            logFs.Close();
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


        public override void ProcessEventFlush(WebEventBufferFlushInfo flushInfo)
        {
            if (flushInfo == null)
            {
                throw new ArgumentNullException("flushInfo");
            }
            customInfo.AppendLine("LogFileWebEventProvider buffer flush.");

            customInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, "NotificationType: {0}", GetNotificationType(flushInfo)));
            customInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, "EventsInBuffer: {0}", GetEventsInBuffer(flushInfo)));
            customInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, "EventsDiscardedSinceLastNotification: {0}", GetEventsDiscardedSinceLastNotification(flushInfo)));

            foreach (WebBaseEvent eventRaised in flushInfo.Events)
                customInfo.AppendLine(eventRaised.ToString());

            StoreToFile(FileMode.Append);
        }

        public override void Shutdown()
        {
            Flush();
        }

    }
}
