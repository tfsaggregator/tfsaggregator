using System.Diagnostics;

using Aggregator.Core;
using Aggregator.Core.Monitoring;

namespace Aggregator.ServerPlugin
{
    /// <summary>
    /// Logs events to the Event log
    /// </summary>
    internal class ServerTextLogger : ITextLogger
    {
        private readonly Stopwatch clock = new Stopwatch();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTextLogger"/> class.
        /// </summary>
        /// <param name="minimumLogLevel">The minimum level (inclusive) of log messages which is written to the log</param>
        internal ServerTextLogger(LogLevel minimumLogLevel)
        {
            this.MinimumLogLevel = minimumLogLevel;
            this.clock.Restart();
        }

        /// <summary>
        /// The minimum level (inclusive) of log messages which is written to the log
        /// </summary>
        /// <value>
        /// The minimum level (inclusive) of log messages which is written to the log
        /// </value>
        public LogLevel MinimumLogLevel { get; set; }

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="level"><see cref="LogLevel"/> of the message</param>
        /// <param name="format">Format string.</param>
        /// <param name="args">Arguments to be passed to string.format</param>
        public void Log(LogLevel level, string format, params object[] args)
        {
            if (level > this.MinimumLogLevel)
            {
                return;
            }

            this.clock.Stop();
            try
            {
                string message = args != null ? string.Format(format, args: args) : format;

                // default trace use OutputDebugString but requires a single call
                const int LogLevelMaximumStringLength = 11; // Len(Information)
                string levelAsString = level.ToString();
                string formattedMessage = string.Format(
                    "TFSAggregator: [{0}]{1} {2}",
                    levelAsString,
                    string.Empty.PadLeft(LogLevelMaximumStringLength - levelAsString.Length),
                    message);

                Debug.WriteLine(formattedMessage);

                EventLogEntryType eventLevel = ConvertToEventLogEntryType(level);

                Microsoft.TeamFoundation.Framework.Server.TeamFoundationApplicationCore.Log(
                    message, 0, eventLevel);

                if (level <= LogLevel.Warning)
                {
                    EventLog.WriteEntry(
                        "TFSAggregator",
                        message,
                        eventLevel);
                }
            }
            finally
            {
                this.clock.Start();
            }
        }

        /// <summary>
        /// converts a <see cref="LogLevel"/> to an <see cref="EventLogEntryType"/>
        /// </summary>
        /// <param name="level">LogLevel to convert</param>
        /// <returns>EventLogEntryType that corresponds with the LogLevel</returns>
        public static EventLogEntryType ConvertToEventLogEntryType(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    return EventLogEntryType.Error;

                case LogLevel.Warning:
                    return EventLogEntryType.Warning;

                case LogLevel.Information:
                case LogLevel.Verbose:
                case LogLevel.Diagnostic:
                default:
                    return EventLogEntryType.Information;
            }
        }
    }
}