using Aggregator.Core;
using Aggregator.Core.Monitoring;
using System.Diagnostics;

namespace Aggregator.ServerPlugin
{
    internal class ServerTextLogger : ITextLogger
    {
        private LogLevel minLevel;
        private Stopwatch clock = new Stopwatch();

        internal ServerTextLogger(LogLevel level)
        {
            this.minLevel = level;
            this.clock.Restart();
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

            clock.Stop();
            try
            {
                string message = args != null ? string.Format(format, args: args) : format;

                Debug.Write("TFSAggregator: ");
                Debug.WriteLine(message);

                Trace.Write("TFSAggregator: ");
                Trace.WriteLine(message);

                if (level >= LogLevel.Warning)
                {
                    //create with Powershell
                    // New -EventLog -LogName "Application" -Source  -ErrorAction SilentlyContinue
                    EventLog.WriteEntry(
                        "TFSAggregator",
                        message,
                        ConvertToEventLogEntryType(level));
                }//if
            }
            finally
            {
                clock.Start();
            }//try
        }

        public static EventLogEntryType ConvertToEventLogEntryType(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Critical:
                    return EventLogEntryType.Error;
                case LogLevel.Error:
                    return EventLogEntryType.Error;
                case LogLevel.Warning:
                    return EventLogEntryType.Warning;
                case LogLevel.Information:
                    return EventLogEntryType.Information;
                case LogLevel.Verbose:
                    return EventLogEntryType.Information;
                case LogLevel.Diagnostic:
                    return EventLogEntryType.Information;
                default:
                    return EventLogEntryType.Information;
            }//switch
        }
    }
}