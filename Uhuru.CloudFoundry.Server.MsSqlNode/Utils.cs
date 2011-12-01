using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Uhuru.CloudFoundry.Server.MsSqlNode
{
    public delegate void StreamWriterDelegate(StreamWriter stream);
    public delegate void ProcessDoneDelegate(string output, int statuscode);

    public class Utils
    {
        public static int DateTimeToEpochSeconds(DateTime date)
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public static DateTime DateTimeFromEpochSeconds(int seconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0) + new TimeSpan(0, 0, seconds);
        }

        public static string DateTimeToRubyString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss zzz");
        }

        public static DateTime DateTimeFromRubyString(string date)
        {
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.SetAllDateTimePatterns(new string[] { "yyyy-MM-dd HH:mm:ss zzz" }, 'Y');
            return DateTime.Parse(date, dateFormat);
        }

        public static string GetStack()
        {
            StackTrace stackTrace = new StackTrace();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            StringBuilder sb = new StringBuilder();

            // write call stack method names
            foreach (StackFrame stackFrame in stackFrames)
            {
                sb.AppendLine(stackFrame.GetMethod().Name);   // write method name
            }

            return sb.ToString();
        }

        //returns the ip used by the OS to connect to the RouteIPAddress. Pointing to a interface address will return that address
        public static string GetLocalIpAddress(string RouteIPAddress = "198.41.0.4")
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(RouteIPAddress, 1);
            IPEndPoint ep = (IPEndPoint)udpClient.Client.LocalEndPoint;
            udpClient.Close();
            return ep.Address.ToString();
        }
    }
}
