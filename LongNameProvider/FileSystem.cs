using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LongNameProvider
{
    sealed class FileSystem
    {
        #region Network

        /// <summary>
        /// Retrieves the name of the network resource associated with a local device.
        /// </summary>
        /// <param name="localName">The name of the local device to get the network name for.</param>
        /// <returns>The remote name used to make the connection.</returns>
        public static string GetUNCForNetworkDrive(string localName)
        {
            int capacity = 300;
            StringBuilder sb = new StringBuilder(capacity);
            int result = Win32.Native.WNetGetConnection(localName, sb, ref capacity);
            if (result == Win32.ErrorCodes.ERROR_MORE_DATA)
            {
                sb = new StringBuilder(capacity);
                result = Win32.Native.WNetGetConnection(localName, sb, ref capacity);
            }
            if (result != Win32.ErrorCodes.ERROR_SUCCESS)
                throw new System.ComponentModel.Win32Exception(result);
            return sb.ToString();
        }

        #endregion

        #region File

        public static System.IO.FileAttributes GetAttributes(string path)
        {
            int attrs = Win32.Native.GetFileAttributes(path);
            if (attrs == Win32.ErrorCodes.INVALID_FILE_ATTRIBUTES)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            return (System.IO.FileAttributes)attrs;
        }

        public static System.IO.FileSystemInfo GetFileSystemInfo(string path)
        {
            var attrs = FileSystem.GetAttributes(path);
            System.IO.FileSystemInfo fsi;
            if ((attrs & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                fsi = new System.IO.DirectoryInfo(path);
            else
                fsi = new System.IO.FileInfo(path);
            return fsi;
        }

        #endregion
    }
}
