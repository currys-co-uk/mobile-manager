using System;
using System.Runtime.CompilerServices;

namespace MobileManager.Logging.Logger
{
    public interface IManagerLogger
    {
        void Info(string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0);

        void Error(string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0);

        void Error(string message,
            Exception e,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0);

        void Debug(string message,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0);
    }
}
