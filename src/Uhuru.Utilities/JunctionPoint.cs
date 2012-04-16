// -----------------------------------------------------------------------
// <copyright file="JunctionPoint.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Provides access to NTFS junction points in .Net.
    /// </summary>
    public static class JunctionPoint
    {
        /// <summary>
        /// The file or directory is not a reparse point.
        /// </summary>
        private const int ErrorNotAReparsePoint = 4390;

        /// <summary>
        /// The reparse point attribute cannot be set because it conflicts with an existing attribute.
        /// </summary>
        private const int ErrorReparseAttributeConflict = 4391;

        /// <summary>
        /// The data present in the reparse point buffer is invalid.
        /// </summary>
        private const int ErrorInvalidReparseData = 4392;

        /// <summary>
        /// The tag present in the reparse point buffer is invalid.
        /// </summary>
        private const int ErrorReparseTagInvalid = 4393;

        /// <summary>
        /// There is a mismatch between the tag specified in the request and the tag present in the reparse point.
        /// </summary>
        private const int ErrorReparseTagMismatch = 4394;

        /// <summary>
        /// Command to set the reparse point data block.
        /// </summary>
        private const int FsctlSetReparsePoint = 0x000900A4;

        /// <summary>
        /// Command to get the reparse point data block.
        /// </summary>
        private const int FsctlGetReparsePoint = 0x000900A8;

        /// <summary>
        /// Command to delete the reparse point data base.
        /// </summary>
        private const int FsctlDeleteReparsePoint = 0x000900AC;

        /// <summary>
        /// Reparse point tag used to identify mount points and junction points.
        /// </summary>
        private const uint IoReparseTagMountPoint = 0xA0000003;

        /// <summary>
        /// This prefix indicates to NTFS that the path is to be treated as a non-interpreted
        /// path in the virtual file system.
        /// </summary>
        private const string NonInterpretedPathPrefix = @"\??\";

        /// <summary>
        /// File access enumeration.
        /// </summary>
        [Flags]
        private enum EFileAccess : uint
        {
            /// <summary>
            /// Read file access.
            /// </summary>
            GenericRead = 0x80000000,
            
            /// <summary>
            /// Write file access.
            /// </summary>
            GenericWrite = 0x40000000,
            
            /// <summary>
            /// Execute file access.
            /// </summary>
            GenericExecute = 0x20000000,

            /// <summary>
            /// File access for everything.
            /// </summary>
            GenericAll = 0x10000000,
        }

        /// <summary>
        /// File sharing access enum.
        /// </summary>
        [Flags]
        private enum EFileShare : uint
        {
            /// <summary>
            /// No sharing.
            /// </summary>
            None = 0x00000000,

            /// <summary>
            /// Read share access.
            /// </summary>
            Read = 0x00000001,

            /// <summary>
            /// Write share access.
            /// </summary>
            Write = 0x00000002,

            /// <summary>
            /// Delete share access.
            /// </summary>
            Delete = 0x00000004,
        }

        /// <summary>
        /// Creation disposition enum.
        /// </summary>
        private enum ECreationDisposition : uint
        {
            /// <summary>
            /// Create new.
            /// </summary>
            New = 1,

            /// <summary>
            /// Create always.
            /// </summary>
            CreateAlways = 2,

            /// <summary>
            /// Open existing.
            /// </summary>
            OpenExisting = 3,

            /// <summary>
            /// Open always.
            /// </summary>
            OpenAlways = 4,

            /// <summary>
            /// Truncate existing.
            /// </summary>
            TruncateExisting = 5,
        }

        /// <summary>
        /// File attributes enum.
        /// </summary>
        [Flags]
        private enum EFileAttributes : uint
        {
            /// <summary>
            /// Read only.
            /// </summary>
            Readonly = 0x00000001,

            /// <summary>
            /// Hidden attribute.
            /// </summary>
            Hidden = 0x00000002,

            /// <summary>
            /// System attribute.
            /// </summary>
            System = 0x00000004,

            /// <summary>
            /// Directory attribute.
            /// </summary>
            Directory = 0x00000010,

            /// <summary>
            /// Archive attribute.
            /// </summary>
            Archive = 0x00000020,

            /// <summary>
            /// Device attribute.
            /// </summary>
            Device = 0x00000040,

            /// <summary>
            /// Normal attribute.
            /// </summary>
            Normal = 0x00000080,

            /// <summary>
            /// Temporary attribute.
            /// </summary>
            Temporary = 0x00000100,

            /// <summary>
            /// Sparse file.
            /// </summary>
            SparseFile = 0x00000200,

            /// <summary>
            /// Reparse point.
            /// </summary>
            ReparsePoint = 0x00000400,

            /// <summary>
            /// Compressed attribute.
            /// </summary>
            Compressed = 0x00000800,

            /// <summary>
            /// Offline attribute.
            /// </summary>
            Offline = 0x00001000,

            /// <summary>
            /// Not content indexed.
            /// </summary>
            NotContentIndexed = 0x00002000,

            /// <summary>
            /// Encrypted attribute.
            /// </summary>
            Encrypted = 0x00004000,

            /// <summary>
            /// Write through.
            /// </summary>
            Write_Through = 0x80000000,

            /// <summary>
            /// Overlapped attribute.
            /// </summary>
            Overlapped = 0x40000000,

            /// <summary>
            /// No buffering.
            /// </summary>
            NoBuffering = 0x20000000,

            /// <summary>
            /// Random access.
            /// </summary>
            RandomAccess = 0x10000000,

            /// <summary>
            /// Sequential scan.
            /// </summary>
            SequentialScan = 0x08000000,

            /// <summary>
            /// Delete on close.
            /// </summary>
            DeleteOnClose = 0x04000000,

            /// <summary>
            /// Backup semantics.
            /// </summary>
            BackupSemantics = 0x02000000,

            /// <summary>
            /// Posix semantics.
            /// </summary>
            PosixSemantics = 0x01000000,

            /// <summary>
            /// Open reparse point.
            /// </summary>
            OpenReparsePoint = 0x00200000,

            /// <summary>
            /// Open no recall.
            /// </summary>
            OpenNoRecall = 0x00100000,

            /// <summary>
            /// First pipe instance.
            /// </summary>
            FirstPipeInstance = 0x00080000
        }

        /// <summary>
        /// Creates a junction point from the specified directory to the specified target directory.
        /// </summary>
        /// <remarks>
        /// Only works on NTFS.
        /// </remarks>
        /// <param name="junctionPoint">The junction point path</param>
        /// <param name="targetDir">The target directory</param>
        /// <param name="overwrite">If true overwrites an existing reparse point or empty directory</param>
        /// <exception cref="IOException">Thrown when the junction point could not be created or when
        /// an existing directory was found and <paramref name="overwrite" /> if false</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "Needed for working with junction points")]
        public static void Create(string junctionPoint, string targetDir, bool overwrite)
        {
            targetDir = Path.GetFullPath(targetDir);

            if (!Directory.Exists(targetDir))
            {
                throw new IOException("Target path does not exist or is not a directory.");
            }

            if (Directory.Exists(junctionPoint))
            {
                if (!overwrite)
                {
                    throw new IOException("Directory already exists and overwrite parameter is false.");
                }
            }
            else
            {
                Directory.CreateDirectory(junctionPoint);
            }

            using (SafeFileHandle handle = OpenReparsePoint(junctionPoint, EFileAccess.GenericWrite))
            {
                byte[] targetDirBytes = Encoding.Unicode.GetBytes(NonInterpretedPathPrefix + Path.GetFullPath(targetDir));

                REPARSE_DATA_BUFFER reparseDataBuffer = new REPARSE_DATA_BUFFER();

                reparseDataBuffer.ReparseTag = IoReparseTagMountPoint;
                reparseDataBuffer.ReparseDataLength = (ushort)(targetDirBytes.Length + 12);
                reparseDataBuffer.SubstituteNameOffset = 0;
                reparseDataBuffer.SubstituteNameLength = (ushort)targetDirBytes.Length;
                reparseDataBuffer.PrintNameOffset = (ushort)(targetDirBytes.Length + 2);
                reparseDataBuffer.PrintNameLength = 0;
                reparseDataBuffer.PathBuffer = new byte[0x3ff0];
                Array.Copy(targetDirBytes, reparseDataBuffer.PathBuffer, targetDirBytes.Length);

                int inBufferSize = Marshal.SizeOf(reparseDataBuffer);
                IntPtr inBuffer = Marshal.AllocHGlobal(inBufferSize);

                try
                {
                    Marshal.StructureToPtr(reparseDataBuffer, inBuffer, false);

                    int bytesReturned;
                    bool result = DeviceIoControl(
                        handle.DangerousGetHandle(), 
                        FsctlSetReparsePoint,
                        inBuffer, 
                        targetDirBytes.Length + 20, 
                        IntPtr.Zero, 
                        0, 
                        out bytesReturned, 
                        IntPtr.Zero);

                    if (!result)
                    {
                        ThrowLastWin32Error(Strings.UnableToCreateJunctionPoint);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(inBuffer);
                }
            }
        }

        /// <summary>
        /// Deletes a junction point at the specified source directory along with the directory itself.
        /// Does nothing if the junction point does not exist.
        /// </summary>
        /// <remarks>
        /// Only works on NTFS.
        /// </remarks>
        /// <param name="junctionPoint">The junction point path</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "Needed for working with junction points")]
        public static void Delete(string junctionPoint)
        {
            if (!Directory.Exists(junctionPoint))
            {
                if (File.Exists(junctionPoint))
                {
                    throw new IOException("Path is not a junction point.");
                }

                return;
            }

            using (SafeFileHandle handle = OpenReparsePoint(junctionPoint, EFileAccess.GenericWrite))
            {
                REPARSE_DATA_BUFFER reparseDataBuffer = new REPARSE_DATA_BUFFER();

                reparseDataBuffer.ReparseTag = IoReparseTagMountPoint;
                reparseDataBuffer.ReparseDataLength = 0;
                reparseDataBuffer.PathBuffer = new byte[0x3ff0];

                int inBufferSize = Marshal.SizeOf(reparseDataBuffer);
                IntPtr inBuffer = Marshal.AllocHGlobal(inBufferSize);
                try
                {
                    Marshal.StructureToPtr(reparseDataBuffer, inBuffer, false);

                    int bytesReturned;
                    bool result = DeviceIoControl(handle.DangerousGetHandle(), FsctlDeleteReparsePoint, inBuffer, 8, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero);

                    if (!result)
                    {
                        ThrowLastWin32Error(Strings.UnableToDeleteJunctionPoint);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(inBuffer);
                }

                try
                {
                    Directory.Delete(junctionPoint);
                }
                catch (IOException ex)
                {
                    throw new IOException(Strings.UnableToDeleteJunctionPoint, ex);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified path exists and refers to a junction point.
        /// </summary>
        /// <param name="path">The junction point path</param>
        /// <returns>True if the specified path represents a junction point</returns>
        /// <exception cref="IOException">Thrown if the specified path is invalid
        /// or some other error occurs</exception>
        public static bool Exists(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            using (SafeFileHandle handle = OpenReparsePoint(path, EFileAccess.GenericRead))
            {
                string target = InternalGetTarget(handle);
                return target != null;
            }
        }

        /// <summary>
        /// Gets the target of the specified junction point.
        /// </summary>
        /// <remarks>
        /// Only works on NTFS.
        /// </remarks>
        /// <param name="junctionPoint">The junction point path</param>
        /// <returns>The target of the junction point</returns>
        /// <exception cref="IOException">Thrown when the specified path does not
        /// exist, is invalid, is not a junction point, or some other error occurs</exception>
        public static string GetTarget(string junctionPoint)
        {
            using (SafeFileHandle handle = OpenReparsePoint(junctionPoint, EFileAccess.GenericRead))
            {
                string target = InternalGetTarget(handle);
                if (target == null)
                {
                    throw new IOException("Path is not a junction point.");
                }

                return target;
            }
        }

        /// <summary>
        /// Sends a control code directly to a specified device driver, causing the corresponding device to perform the corresponding operation.
        /// </summary>
        /// <param name="device">A handle to the device on which the operation is to be performed.</param>
        /// <param name="controlCode">The control code for the operation. </param>
        /// <param name="inBuffer">A pointer to the input buffer that contains the data required to perform the operation.</param>
        /// <param name="inBufferSize">The size of the input buffer, in bytes.</param>
        /// <param name="outBuffer">A pointer to the output buffer that is to receive the data returned by the operation.</param>
        /// <param name="outBufferSize">The size of the output buffer, in bytes.</param>
        /// <param name="bytesReturned">A pointer to a variable that receives the size of the data stored in the output buffer, in bytes.</param>
        /// <param name="overlapped">A pointer to an OVERLAPPED structure.</param>
        /// <returns>If the operation completes successfully, the return value is nonzero.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Needed for working with junction points"), 
        DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeviceIoControl(IntPtr device, uint controlCode, IntPtr inBuffer, int inBufferSize, IntPtr outBuffer, int outBufferSize, out int bytesReturned, IntPtr overlapped);

        /// <summary>
        /// Creates or opens a file or I/O device.
        /// </summary>
        /// <param name="fileName">The name of the file or device to be created or opened.</param>
        /// <param name="desiredAccess">The requested access to the file or device, which can be summarized as read, write, both or neither zero).</param>
        /// <param name="shareMode">The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or none.</param>
        /// <param name="securityAttributes">A pointer to a SECURITY_ATTRIBUTES structure that contains two separate but related data members: an optional security descriptor, and a Boolean value that determines whether the returned handle can be inherited by child processes.</param>
        /// <param name="creationDisposition">An action to take on a file or device that exists or does not exist.</param>
        /// <param name="flagsAndAttributes">The file or device attributes and flags, FILE_ATTRIBUTE_NORMAL being the most common default value for files.</param>
        /// <param name="templateFile">A valid handle to a template file with the GENERIC_READ access right.</param>
        /// <returns>If the function succeeds, the return value is an open handle to the specified file, device, named pipe, or mail slot.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Needed for working with junction points"), 
        DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string fileName, EFileAccess desiredAccess, EFileShare shareMode, IntPtr securityAttributes, ECreationDisposition creationDisposition, EFileAttributes flagsAndAttributes, IntPtr templateFile);

        /// <summary>
        /// Gets target directory.
        /// </summary>
        /// <param name="handle">A file safe handle.</param>
        /// <returns>The target directory.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "Needed for working with junction points")]
        private static string InternalGetTarget(SafeFileHandle handle)
        {
            int outBufferSize = Marshal.SizeOf(typeof(REPARSE_DATA_BUFFER));
            IntPtr outBuffer = Marshal.AllocHGlobal(outBufferSize);

            try
            {
                int bytesReturned;
                bool result = DeviceIoControl(handle.DangerousGetHandle(), FsctlGetReparsePoint, IntPtr.Zero, 0, outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);

                if (!result)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == ErrorNotAReparsePoint)
                    {
                        return null;
                    }

                    ThrowLastWin32Error(Strings.UnableToGetJunctionInformation);
                }

                REPARSE_DATA_BUFFER reparseDataBuffer = (REPARSE_DATA_BUFFER)
                    Marshal.PtrToStructure(outBuffer, typeof(REPARSE_DATA_BUFFER));

                if (reparseDataBuffer.ReparseTag != IoReparseTagMountPoint)
                {
                    return null;
                }

                string targetDir = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer, reparseDataBuffer.SubstituteNameOffset, reparseDataBuffer.SubstituteNameLength);

                if (targetDir.StartsWith(NonInterpretedPathPrefix, StringComparison.Ordinal))
                {
                    targetDir = targetDir.Substring(NonInterpretedPathPrefix.Length);
                }

                return targetDir;
            }
            finally
            {
                Marshal.FreeHGlobal(outBuffer);
            }
        }

        /// <summary>
        /// Opens a reparse point.
        /// </summary>
        /// <param name="reparsePoint">The reparse point to open.</param>
        /// <param name="accessMode">Access mode use to open the reparse point.</param>
        /// <returns>A safe file handle.</returns>
        private static SafeFileHandle OpenReparsePoint(string reparsePoint, EFileAccess accessMode)
        {
            SafeFileHandle reparsePointHandle = null;
            try
            {
                IntPtr createFileResult = CreateFile(
                    reparsePoint, 
                    accessMode,
                    EFileShare.Read | EFileShare.Write | EFileShare.Delete,
                    IntPtr.Zero, 
                    ECreationDisposition.OpenExisting,
                    EFileAttributes.BackupSemantics | EFileAttributes.OpenReparsePoint, 
                    IntPtr.Zero);

                if (Marshal.GetLastWin32Error() != 0)
                {
                    ThrowLastWin32Error(Strings.UnableToOpenReparsePoint);
                }

                reparsePointHandle = new SafeFileHandle(createFileResult, true);

                return reparsePointHandle;
            }
            catch
            {
                if (reparsePointHandle != null)
                {
                    reparsePointHandle.Dispose();
                }

                throw;
            }
        }

        /// <summary>
        /// Throws an exception containing a messsage and the last win 32 API error.
        /// </summary>
        /// <param name="message">A message for the exception to be thrown.</param>
        private static void ThrowLastWin32Error(string message)
        {
            throw new IOException(message, Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
        }

        /// <summary>
        /// A structure for a reparse data buffer.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct REPARSE_DATA_BUFFER
        {
            /// <summary>
            /// Reparse point tag. Must be a Microsoft reparse point tag.
            /// </summary>
            public uint ReparseTag;

            /// <summary>
            /// Size, in bytes, of the data after the Reserved member. This can be calculated by:
            /// (4 * sizeof(ushort)) + SubstituteNameLength + PrintNameLength + 
            /// (namesAreNullTerminated ? 2 * sizeof(char) : 0);
            /// </summary>
            public ushort ReparseDataLength;

            /// <summary>
            /// Reserved; do not use. 
            /// </summary>
            public ushort Reserved;

            /// <summary>
            /// Offset, in bytes, of the substitute name string in the PathBuffer array.
            /// </summary>
            public ushort SubstituteNameOffset;

            /// <summary>
            /// Length, in bytes, of the substitute name string. If this string is null-terminated,
            /// SubstituteNameLength does not include space for the null character.
            /// </summary>
            public ushort SubstituteNameLength;

            /// <summary>
            /// Offset, in bytes, of the print name string in the PathBuffer array.
            /// </summary>
            public ushort PrintNameOffset;

            /// <summary>
            /// Length, in bytes, of the print name string. If this string is null-terminated,
            /// PrintNameLength does not include space for the null character. 
            /// </summary>
            public ushort PrintNameLength;

            /// <summary>
            /// A buffer containing the unicode-encoded path string. The path string contains
            /// the substitute name string and print name string.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
            public byte[] PathBuffer;
        }
    }
}