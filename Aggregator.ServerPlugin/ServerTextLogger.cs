using Aggregator.Core;
using Aggregator.Core.Monitoring;
using System.Diagnostics;

namespace Aggregator.ServerPlugin
{
    internal class ServerTextLogger : ITextLogger
    {
        private LogLevel minLevel;

        internal ServerTextLogger(LogLevel level)
        {
            this.minLevel = level;
        }

        public LogLevel Level
        {
            get { return minLevel; }
            set { minLevel = value; }
        }

        public void Log(LogLevel level, string format, params object[] args)
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