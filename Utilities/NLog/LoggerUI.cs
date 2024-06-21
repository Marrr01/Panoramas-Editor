using NLog;
using NLog.Targets;

namespace Panoramas_Editor
{
    [Target("LogUI")]
    internal class LoggerUI : TargetWithLayout
    {
        private ILoggerVM _loggerVM;
        public LoggerUI(ILoggerVM loggerVM)
        {
            _loggerVM = loggerVM;
        }
        protected override void Write(LogEventInfo logEvent)
        {
            //base.Write(logEvent);
            _loggerVM.Add(logEvent.Message, logEvent.Level);
        }
    }
}
