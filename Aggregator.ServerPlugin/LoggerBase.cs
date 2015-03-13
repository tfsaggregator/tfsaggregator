using Aggregator.Core;
using System.Diagnostics;

namespace Aggregator.ServerPlugin
{
    class LoggerBase
    {
        private LogLevel minLevel;

        protected LoggerBase(LogLevel level)
        {
            this.minLevel = level;
        }

        public LogLevel Level
        {
            get { return minLevel; }
            set { minLevel = value; }
        }

        protected void Log(LogLevel level, string format, params object[] args)
        {
            if (level > this.minLevel)
                return;

            string message = args != null ? string.Format(format, args: args) : format;

            Debug.Write("TFSAggregator: ");
            Debug.WriteLine(message);

            Trace.Write("TFSAggregator: ");
            Trace.WriteLine(message);
        }

    }
}