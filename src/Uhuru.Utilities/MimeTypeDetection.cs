// -----------------------------------------------------------------------
// <copyright file="MimeTypeDetection.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// This is a helper class that tries to detect the MIME type of a file.
    /// </summary>
    public static class MimeTypeDetection
    {
        /// <summary>
        /// Gets the MIME type from file.
        /// </summary>
        /// <param name="fileName">The filename.</param>
        /// <returns>A string containing the detected MIME type.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If something goes wrong, just give up and return an unkown mime type.")]
        public static string GetMimeFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName + " not found");
            }

            byte[] buffer = new byte[256];
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                if (fs.Length >= 256)
                {
                    fs.Read(buffer, 0, 256);
                }
                else
                {
                    fs.Read(buffer, 0, (int)fs.Length);
                }
            }

            try
            {
                IntPtr mimeTypePtr;
                int ret = FindMimeFromData(IntPtr.Zero, null, buffer, 256, null, 0, out mimeTypePtr, 0);
                
                string mime = "unknown/unknown";

                if (ret == 0 && mimeTypePtr != IntPtr.Zero)
                {
                    mime = Marshal.PtrToStringUni(mimeTypePtr);
                    Marshal.FreeCoTaskMem(mimeTypePtr);
                }

                return mime;
            }
            catch
            {
                return "unknown/unknown";
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Keeping WinAPI definitions in the cs file that needs them."),
        DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        private static extern int FindMimeFromData(
            IntPtr bindInterface,
            [MarshalAs(UnmanagedType.LPWStr)] string url,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)] byte[] buffer,
            int size,
            [MarshalAs(UnmanagedType.LPWStr)] string mimeProposed,
            int mimeFlags,
            out IntPtr mimeOut,
            int reserved);
    }
}
