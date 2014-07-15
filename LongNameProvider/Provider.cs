using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace LongNameProvider
{
    [CmdletProvider("LongNameProvider", ProviderCapabilities.Include | ProviderCapabilities.ShouldProcess)]
    public class LongNameProvider : NavigationCmdletProvider
    {
        #region Drive methods

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            var psdrives = new Collection<PSDriveInfo>();
            var drives = System.IO.DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                int pos = drive.Name.IndexOf(':');
                string name = "_" + (pos < 0 ? drive.Name : drive.Name.Substring(0, pos));
                string description = String.Empty;
                string root = "\\\\?\\" + drive.Name; // чтобы не пересекались с FileSystemProvider
                string displayRoot = null;

                try
                {
                    description = drive.VolumeLabel;
                }
                catch(System.IO.IOException)
                { }
                catch(System.Security.SecurityException)
                { }
                catch(UnauthorizedAccessException)
                { }

                if (drive.DriveType == System.IO.DriveType.Network)
                    displayRoot = FileSystem.GetUNCForNetworkDrive(name);

                try
                {
                    var psdrive = new PSDriveInfo(name, ProviderInfo, root, drive.Name, null, displayRoot);
                    if (drive.DriveType == System.IO.DriveType.Network)
                    {
                        // MS скрыла свойство IsNetworkDrive, мы его все-таки установим
                        Service.SetPrivateProperty(psdrive, "IsNetworkDrive", true);
                    }
                    if (drive.DriveType != System.IO.DriveType.Fixed)
                    {
                        Service.SetPrivateProperty(psdrive, "IsAutoMounted", true);
                    }
                    psdrives.Add(psdrive);
                }
                catch (System.IO.IOException)
                { }
                catch (System.Security.SecurityException)
                { }
                catch (System.UnauthorizedAccessException)
                { }
            }

            return psdrives;
        }

        #endregion

        #region Item methods

        protected override void GetItem(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new PSArgumentNullException("path");

            try
            {
                bool isContainer;
                var fsi = GetFileSystemItem(path, out isContainer);
                WriteItemObject(fsi, path, isContainer);
            }
            catch(System.IO.IOException ex)
            {
                WriteError(new ErrorRecord(ex, "ItemNotFound", ErrorCategory.ReadError, path));
            }
            catch(UnauthorizedAccessException ex)
            {
                WriteError(new ErrorRecord(ex, "GetItemUnauthorizedAccessError", ErrorCategory.PermissionDenied, path));
            }
        }

        protected override bool ItemExists(string path)
        {
            ErrorRecord error;
            bool result = ItemExists(path, out error);
            if (error != null)
                WriteError(error);
            return result;
        }

        protected static bool ItemExists(string path, out bool isDirectory, out Exception exception)
        {
            exception = null;
            isDirectory = false;
            bool result;

            if (String.IsNullOrEmpty(path))
                result = false;
            else
            {
                Win32.FindData findData = new Win32.FindData();
                using (var findHandle = Win32.Native.FindFirstFile(path, findData))
                {
                    if (findHandle.IsInvalid)
                    {
                        int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                        if (errorCode == Win32.ErrorCodes.ERROR_ACCESS_DENIED)
                        {
                            Exception win32ex = new System.ComponentModel.Win32Exception(errorCode);
                            exception = new UnauthorizedAccessException(win32ex.Message, win32ex);
                        }
                        result = false;
                    }
                    else
                    {
                        isDirectory = ((findData.FileAttributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory);
                        result = true;
                    }
                }
            }
            return result;
        }

        protected static bool ItemExists(string path, out ErrorRecord error)
        {
            if (String.IsNullOrEmpty(path))
                throw new PSArgumentNullException("path");

            error = null;
            bool isDirectory;
            Exception exception;
            bool result = ItemExists(path, out isDirectory, out exception);
            if (exception != null)
            {
                error = CreateErrorForException(exception, path);
                if (error == null)
                    // Неизвестное исключение, выбросим его
                    throw exception;
            }
            return result;
        }

        protected static ErrorRecord CreateErrorForException(Exception ex, object targetObject)
        {
            ErrorRecord error = null;
            if (ex is System.Security.SecurityException)
                error = new ErrorRecord(ex, "SecurityError", ErrorCategory.PermissionDenied, targetObject);
            else if (ex is ArgumentException)
                error = new ErrorRecord(ex, "ArgumentError", ErrorCategory.InvalidArgument, targetObject);
            else if (ex is UnauthorizedAccessException)
                error = new ErrorRecord(ex, "UnauthorizedAccessError", ErrorCategory.PermissionDenied, targetObject);
            else if (ex is System.IO.PathTooLongException)
                error = new ErrorRecord(ex, "PathTooLongError", ErrorCategory.InvalidArgument, targetObject);
            else if (ex is NotSupportedException)
                error = new ErrorRecord(ex, "NotSupportedError", ErrorCategory.InvalidOperation, targetObject);
            return error;
        }

        protected static System.IO.FileSystemInfo GetFileSystemItem(string path, out bool isContainer)
        {
            var fsi = FileSystem.GetFileSystemInfo(path);
            isContainer = (fsi is System.IO.DirectoryInfo);
            return fsi;
        }

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            base.CopyItem(path, copyPath, recurse);
        }

        #endregion

        #region Path methods

        protected override bool IsValidPath(string path)
        {
            if (String.IsNullOrEmpty(path))
                return false;

            try
            {
                new System.IO.FileInfo(path);
            }
            catch(Exception ex)
            {
                if (ex is ArgumentException || ex is System.Security.SecurityException || ex is UnauthorizedAccessException || ex is System.IO.PathTooLongException || ex is NotSupportedException)
                    return false;
                throw;
            }
            return true;
        }

        #endregion
    }
}
