using NLog;

namespace Panoramas_Editor
{
    internal interface ILoggerVM
    {
        void Add(string message, LogLevel logLevel);
    }
}
