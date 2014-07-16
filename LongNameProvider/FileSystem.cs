using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LongNameProvider
{
    sealed static class FileSystem
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

        #region Enumerate files

        IEnumerable<TResult> EnumerateFiles<TResult>(string path, string pattern, IFindResultHandler<TResult> findResultHandler, bool caseSensitive)
        {
            string filename = CombinePath(path, pattern);
            Win32.FindData findData = new Win32.FindData();
            Win32.SearchAdditionalFlags flags = Win32.SearchAdditionalFlags.LargeFetch;
            if (caseSensitive)
                flags |= Win32.SearchAdditionalFlags.CaseSensitive;

            using (Win32.SafeFindHandle findHandle = Win32.Native.FindFirstFileEx(filename, Win32.FIndexInfoLevels.Basic, findData, Win32.FIndexSearchOps.NameMatch, IntPtr.Zero, flags))
            {
                bool ok = !findHandle.IsInvalid;
                while (ok)
                {
                    if (findResultHandler.IsResultOK(path, findData))
                        yield return findResultHandler.GetResult(path, findData);
                    ok = Win32.Native.FindNextFile(findHandle, findData);
                }
                int errorCode = Marshal.GetLastWin32Error();
                if (!(errorCode == Win32.ErrorCodes.ERROR_FILE_NOT_FOUND || errorCode == Win32.ErrorCodes.ERROR_NO_MORE_FILES || errorCode == Win32.ErrorCodes.ERROR_PATH_NOT_FOUND))
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        interface IFindResultHandler<TResult>
        {
            bool IsResultOK(string path, Win32FindData findData);
            TResult GetResult(string path, Win32FindData findData);
        }

        class StringFindResultHandler : IFindResultHandler<string>
        {
            bool includeFiles;
            bool includeDirectories;

            public StringFindResultHandler(bool includeFiles, bool includeDirectories)
            {
                this.includeFiles = includeFiles;
                this.includeDirectories = includeDirectories;
            }

            public bool IsResultOK(string path, Win32.FindData findData)
            {
                if ((findData.FileAttributes & System.IO.FileAttributes.Directory) != 0)
                    return includeDirectories;
                else
                    return includeFiles;
            }

            public string GetResult(string path, Win32.FindData findData)
            {
                return findData.FileName;
            }
        }

        class FSIFindResultHandler : IFindResultHandler<System.IO.FileSystemInfo>
        {
            bool includeFiles;
            bool includeDirectories;

            public FSIFindResultHandler(bool includeFiles, bool includeDirectories)
            {
                this.includeFiles = includeFiles;
                this.includeDirectories = includeDirectories;
            }

            public bool IsResultOK(string path, Win32.FindData findData)
            {
                if ((findData.FileAttributes & System.IO.FileAttributes.Directory) != 0)
                    return includeDirectories;
                else
                    return includeFiles;
            }

            public System.IO.FileSystemInfo GetResult(string path, Win32.FindData findData)
            {
                string filename = CombinePath(path, findData.FileName);
                if ((findData.FileAttributes & System.IO.FileAttributes.Directory) != 0)
                    return new System.IO.DirectoryInfo(filename);
                else
                    return new System.IO.FileInfo(filename);
            }
        }

        #endregion

        #region Path

        public static string CombinePath(string p1, string p2)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
