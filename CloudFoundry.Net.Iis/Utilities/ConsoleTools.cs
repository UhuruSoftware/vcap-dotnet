using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace CloudFoundry.Net.IIS.Utilities
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ConsoleTools
    {
        private AutoResetEvent closing = null;

        public ConsoleTools(AutoResetEvent closing)
        {
            this.closing = closing;
        }

        public bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // Put your own handler here
            switch (ctrlType)
            {
                case CtrlTypes.Ctrl_C_Event:
                case CtrlTypes.Ctrl_Break_Event:
                case CtrlTypes.Ctrl_Close_Eevent:
                case CtrlTypes.Ctrl_LogOff_Event:
                case CtrlTypes.Ctrl_ShutDown_Event:
                    if (closing != null)
                    {
                        closing.Set();
                    }
                    break;
            }
            return true;
        }

        #region unmanaged

        public const UInt32 Ctrl_C_Event = 0;
        public const UInt32 Ctrl_Break_Event = 1;

        [DllImport("kernel32")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent,
           uint dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool FreeConsole();

        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate. 
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine 
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            Ctrl_C_Event = 0,
            Ctrl_Break_Event,
            Ctrl_Close_Eevent,
            Ctrl_LogOff_Event = 5,
            Ctrl_ShutDown_Event
        }
        #endregion


    }
}
