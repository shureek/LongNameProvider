using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongNameProvider.Win32
{
    static class ErrorCodes
    {
        public const int INVALID_FILE_ATTRIBUTES = -1;
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_PATH_NOT_FOUND = 3;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_NO_MORE_FILES = 18;
        public const int ERROR_NOT_READY = 21;
        public const int ERROR_MORE_DATA = 234;
    }
}
