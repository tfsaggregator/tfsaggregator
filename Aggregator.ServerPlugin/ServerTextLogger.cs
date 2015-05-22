namespace Aggregator.ServerPlugin
{
    using System.Diagnostics;

    using Aggregator.Core;
    using Aggregator.Core.Monitoring;

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
            get { return this.minLevel; }
            set {
                this.minLevel = value; }
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            if (level > this.minLevel)
                return;

            this.clock.Stop();
            try
            {
                string message = args != null ? string.Format(format, args: args) : format;

                // default trace use OutputDebugString but requires a single call
                const int LogLevelMaximumStringLength = 11; // Len(Information)
                string levelAsString = level.ToString();
                string formattedMessage = string.Format(
                    "TFSAggregator: [{0}]{1} {2}"
                    , levelAsString, string.Empty.PadLeft(LogLevelMaximumStringLength - levelAsString.Length)
                    , message);

                Trace.WriteLine(formattedMessage);

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
                this.clock.Start();
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