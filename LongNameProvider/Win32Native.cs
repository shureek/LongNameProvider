using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace LongNameProvider.Win32
{
    static class Native
    {
        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        public static extern int WNetGetConnection(string localName, StringBuilder remoteName, ref int remoteNameLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetFileAttributes(string fileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
        public static extern bool GetFileAttributesEx(string fileName, int fileInfoLevel, ref FileAttributeData fileInfo);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFindHandle FindFirstFile(string fileName, [In, Out]FindData data);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool FindNextFile(SafeFindHandle findHandle, [MarshalAs(UnmanagedType.LPStruct), In, Out]FindData data);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        public static extern SafeFindHandle FindFirstFileEx(string fileName, FIndexInfoLevels infoLevel, [In, Out]Win32.FindData fineFileData, FIndexSearchOps searchOp, IntPtr searchFilter, SearchAdditionalFlags flags);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr handle);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FileAttributeData
    {
        public System.IO.FileAttributes FileAttributes;
        public long CreationFileTime;
        public long LastAccessFileTime;
        public long LastWriteFileTime;
        public long FileSize;

        public DateTime CreationTime
        {
            get { return DateTime.FromFileTime(CreationFileTime); }
            set { CreationFileTime = value.ToFileTime(); }
        }

        public DateTime LastAccessTime
        {
            get { return DateTime.FromFileTime(LastAccessFileTime); }
            set { LastAccessFileTime = value.ToFileTime(); }
        }

        public DateTime LastWriteTime
        {
            get { return DateTime.FromFileTime(LastWriteFileTime); }
            set { LastWriteFileTime = value.ToFileTime(); }
        }

        public void PopulateFrom(FindData findData)
        {
            FileAttributes = findData.FileAttributes;
            CreationFileTime = findData.CreationFileTime;
            LastAccessFileTime = findData.LastAccessFileTime;
            LastWriteFileTime = findData.LastWriteFileTime;
            FileSize = findData.FileSize;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), BestFitMapping(false)]
    struct FindData
    {
        public System.IO.FileAttributes FileAttributes;
        public long CreationFileTime;
        public long LastAccessFileTime;
        public long LastWriteFileTime;
        public long FileSize;
        long reserved;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string FileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string AlternateFileName;

        public DateTime CreationTime
        {
            get { return DateTime.FromFileTime(CreationFileTime); }
            set { CreationFileTime = value.ToFileTime(); }
        }

        public DateTime LastAccessTime
        {
            get { return DateTime.FromFileTime(LastAccessFileTime); }
            set { LastAccessFileTime = value.ToFileTime(); }
        }

        public DateTime LastWriteTime
        {
            get { return DateTime.FromFileTime(LastWriteFileTime); }
            set { LastWriteFileTime = value.ToFileTime(); }
        }
    }

    enum FIndexInfoLevels
    {
        /// <summary>
        /// Standard set of attribute information.
        /// </summary>
        Standard,
        /// <summary>
        /// Does not query short file names.
        /// </summary>
        Basic
    }

    enum FIndexSearchOps
    {
        /// <summary>
        /// The search for a file that matches a specified file name.
        /// </summary>
        NameMatch,
        /// <summary>
        /// If the file system supports directory filtering, the functions searches for directories only.
        /// </summary>
        /// <remarks>
        /// The searchFilter parameter must be <value>null</value> when this search value is used.
        /// </remarks>
        LimitToDirectories
    }

    [Flags]
    enum SearchAdditionalFlags
    {
        None = 0,
        /// <summary>
        /// Searches are case-sensitive.
        /// </summary>
        CaseSensitive = 1,
        /// <summary>
        /// Uses larger buffer for directory queries, which can increase performance of the find operation.
        /// </summary>
        LargeFetch = 2
    }

    sealed class SafeFindHandle : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeFindHandle() : base(true) { }
        protected override bool ReleaseHandle()
        {
            return Native.FindClose(handle);
        }
    }
}
