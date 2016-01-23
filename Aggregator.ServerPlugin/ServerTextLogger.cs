using System;
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
        private readonly TraceSource traceSource = new TraceSource("TfsAggregator.ServerPlugin", SourceLevels.Information);
        private readonly TraceSource userTraceSource = new TraceSource("TfsAggregator.User", SourceLevels.Information);

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
                int id = System.Math.Abs(format.GetHashCode()) % 65000;

                this.traceSource.TraceEvent(
                    ConvertToTraceEventType(level), id, format, args: args);

                OutputDebug(level, "TFSAggregator", message);

                WriteToEventLog(level, message, id);
            }
            finally
            {
                this.clock.Start();
            }
        }

        public void UserLog(LogLevel level, string ruleName, string userMessage)
        {
            if (level > this.MinimumLogLevel)
            {
                return;
            }

            this.clock.Stop();
            try
            {
                string message = ruleName + ": " + userMessage;
                const int id = 42;

                this.userTraceSource.TraceEvent(
                    ConvertToTraceEventType(level), id, message);

                OutputDebug(level, "TFSAggregator.User", message);

                WriteToEventLog(level, message, id);
            }
            finally
            {
                this.clock.Start();
            }
        }

        private static void OutputDebug(LogLevel level, string tag, string message)
        {
            // default trace use OutputDebugString but requires a single call
            const int LogLevelMaximumStringLength = 11; // Len(Information)
            string levelAsString = level.ToString();
            string formattedMessage = string.Format(
                "{3}: [{0}]{1} {2}",
                levelAsString,
                string.Empty.PadLeft(LogLevelMaximumStringLength - levelAsString.Length),
                message,
                tag);

            DebugOutput.WriteLine(formattedMessage);
        }

        private static void WriteToEventLog(LogLevel level, string message, int id)
        {
            if (level <= LogLevel.Warning)
            {
                EventLog.WriteEntry(
                    "TFSAggregator",
                    message,
                    ConvertToEventLogEntryType(level),
                    id);
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

        /// <summary>
        /// converts a <see cref="LogLevel"/> to an <see cref="EventLogEntryType"/>
        /// </summary>
        /// <param name="level">LogLevel to convert</param>
        /// <returns>EventLogEntryType that corresponds with the LogLevel</returns>
        public static TraceEventType ConvertToTraceEventType(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Critical:
                    return TraceEventType.Critical;
                case LogLevel.Error:
                    return TraceEventType.Error;
                case LogLevel.Warning:
                    return TraceEventType.Warning;
                case LogLevel.Information:
                    return TraceEventType.Information;
                case LogLevel.Verbose:
                case LogLevel.Diagnostic:
                default:
                    return TraceEventType.Verbose;
            }
        }
    }
}