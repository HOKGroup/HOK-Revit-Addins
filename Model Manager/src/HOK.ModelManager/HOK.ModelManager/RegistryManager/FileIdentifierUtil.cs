using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.IO;

namespace HOK.ModelManager.RegistryManager
{
    public static class FileIdentifierUtil
    {
        public static string GetIdentifier(string filename)
        {
            string identifier = "";
            try
            {
                WinAPI.BY_HANDLE_FILE_INFORMATION objectFileInfo = new WinAPI.BY_HANDLE_FILE_INFORMATION();

                if (File.Exists(filename))
                {
                    FileInfo fi = new FileInfo(filename);
                    FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    WinAPI.GetFileInformationByHandle(fs.Handle, out objectFileInfo);

                    fs.Close();

                    ulong fileIndex = ((ulong)objectFileInfo.FileIndexHigh << 32) + (ulong)objectFileInfo.FileIndexLow;
                    identifier = fileIndex.ToString();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return identifier;
        }
    }

    public class WinAPI
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern IntPtr NtQueryInformationFile(IntPtr fileHandle, ref IO_STATUS_BLOCK IoStatusBlock, IntPtr pInfoBlock, uint length, FILE_INFORMATION_CLASS fileInformation);

        public struct IO_STATUS_BLOCK
        {
            uint status;
            ulong information;
        }
        public struct _FILE_INTERNAL_INFORMATION
        {
            public ulong IndexNumber;
        }

        // Abbreviated, there are more values than shown
        public enum FILE_INFORMATION_CLASS
        {
            FileDirectoryInformation = 1,     // 1
            FileFullDirectoryInformation,     // 2
            FileBothDirectoryInformation,     // 3
            FileBasicInformation,         // 4
            FileStandardInformation,      // 5
            FileInternalInformation      // 6
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetFileInformationByHandle(IntPtr hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        public struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }
    }
}
