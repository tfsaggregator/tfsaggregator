using Aggregator.Core.Monitoring;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Aggregator.WebHooks.Utils
{
    internal class DiagnosticTraceLogger : ITextLogger
    {
        private readonly Stopwatch clock = new Stopwatch();

        internal DiagnosticTraceLogger(string correlationId, LogLevel minimumLogLevel)
        {
            this.MinimumLogLevel = minimumLogLevel;
            this.CorrelationId = correlationId;
            this.clock.Restart();
        }

        public LogLevel MinimumLogLevel { get; set; }
        public string CorrelationId { get; private set; }

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

                const int LogLevelMaximumStringLength = 11; // Len(LogLevel.Information)
                string levelAsString = level.ToString();

                string traceMessage = string.Format(
                        "[{0}]{1} {2:00}.{3:000} {4} {5}",
                        levelAsString,
                        string.Empty.PadLeft(LogLevelMaximumStringLength - levelAsString.Length),
                        this.clock.ElapsedMilliseconds / 1000,
                        this.clock.ElapsedMilliseconds % 1000,
                        this.CorrelationId,
                        message);

                switch (level)
                {
                    case LogLevel.Critical:
                        System.Diagnostics.Trace.TraceError(traceMessage);
                        break;
                    case LogLevel.Error:
                        System.Diagnostics.Trace.TraceError(traceMessage);
                        break;
                    case LogLevel.Warning:
                        System.Diagnostics.Trace.TraceWarning(traceMessage);
                        break;
                    case LogLevel.Information: // case LogLevel.Normal:
                        System.Diagnostics.Trace.TraceInformation(traceMessage);
                        break;
                    case LogLevel.Verbose:
                        System.Diagnostics.Trace.WriteLine(traceMessage);
                        break;
                    case LogLevel.Diagnostic:
                        System.Diagnostics.Trace.WriteLine(traceMessage);
                        break;
                    default:
                        System.Diagnostics.Trace.WriteLine(traceMessage);
                        break;
                }
                
            }
            finally
            {
                this.clock.Start();
            }//try
        }

        public void UserLog(LogLevel level, string ruleName, string message)
        {
            this.Log(level, "{0}: {1}", ruleName, message);
        }
    }
}