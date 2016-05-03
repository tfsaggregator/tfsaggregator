using System;
using System.Diagnostics;

using Aggregator.Core.Monitoring;

namespace Aggregator.ConsoleApp
{
    internal class ConsoleTextLogger : ITextLogger
    {
        private readonly Stopwatch clock = new Stopwatch();

        internal ConsoleTextLogger(LogLevel minimumLogLevel)
        {
            this.MinimumLogLevel = minimumLogLevel;
            this.clock.Restart();
        }

        public LogLevel MinimumLogLevel { get; set; }

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

                ConsoleColor save = Console.ForegroundColor;
                Console.ForegroundColor = MapColor(level);

                const int LogLevelMaximumStringLength = 11; // Len(Information)
                string levelAsString = level.ToString();

#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
                Console.Write(
                    "[{0}]{1} {2:00}.{3:000} ",
                    levelAsString,
                    string.Empty.PadLeft(LogLevelMaximumStringLength - levelAsString.Length),
                    this.clock.ElapsedMilliseconds / 1000,
                    this.clock.ElapsedMilliseconds % 1000);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"

                Console.WriteLine(message);

                Console.ForegroundColor = save;
            }
            finally
            {
                this.clock.Start();
            }
        }

        public void UserLog(LogLevel level, string ruleName, string message)
        {
            this.Log(level, "{0}: {1}", ruleName, message);
        }

        internal static ConsoleColor MapColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Error:
                case LogLevel.Critical:
                    return ConsoleColor.Red;

                case LogLevel.Warning:
                    return ConsoleColor.Yellow;

                case LogLevel.Verbose:
                    return ConsoleColor.Cyan;

                case LogLevel.Diagnostic:
                    return ConsoleColor.DarkCyan;

                case LogLevel.Information:
                default:
                    return ConsoleColor.White;
            }
        }
    }
}