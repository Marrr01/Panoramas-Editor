using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Targets;

namespace Panoramas_Editor
{
    internal class UITarget : TargetWithLayout
    {
        private ILoggerVM _loggerVM;
        public UITarget()
        {
            _loggerVM = App.Current.Services.GetService<ExecutionVM>();
        }
        protected override void Write(LogEventInfo logEvent)
        {
            _loggerVM.Add(RenderLogEvent(Layout, logEvent), logEvent.Level);
        }
    }
}
