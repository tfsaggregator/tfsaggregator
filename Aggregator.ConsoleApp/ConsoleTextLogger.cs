namespace Aggregator.ConsoleApp
{
    using System;
    using System.Diagnostics;

    using Core;
    using Core.Monitoring;

    internal class ConsoleTextLogger : ITextLogger
    {
        private LogLevel minimumLogLevel;
        private readonly Stopwatch clock = new Stopwatch();

        internal ConsoleTextLogger(LogLevel minimumLogLevel)
        {
            this.minimumLogLevel = minimumLogLevel;
            this.clock.Restart();
        }

        static ConsoleColor MapColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Error:
                case LogLevel.Critical:
                    return ConsoleColor.Red;

                case LogLevel.Warning:
                    return ConsoleColor.Yellow;

                case LogLevel.Verbose:
                case LogLevel.Diagnostic:
                    return ConsoleColor.Cyan;

                case LogLevel.Information:
                default:
                    return ConsoleColor.Gray;
            }//switch
        }

        public LogLevel MinimumLogLevel
        {
            get { return this.minimumLogLevel; }
            set { this.minimumLogLevel = value; }
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            if (level > this.minimumLogLevel)
                return;

            clock.Stop();
            try
            {
                string message = args != null ? string.Format(format, args: args) : format;

                ConsoleColor save = Console.ForegroundColor;
                Console.ForegroundColor = MapColor(level);

                const int LogLevelMaximumStringLength = 11; // Len(Information)
                string levelAsString = level.ToString();
                Console.Write("[{0}]{1} {2:00}.{3:000} "
                    , levelAsString, string.Empty.PadLeft(LogLevelMaximumStringLength - levelAsString.Length)
                    , clock.ElapsedMilliseconds / 1000, clock.ElapsedMilliseconds % 1000);
                Console.WriteLine(message);

                Console.ForegroundColor = save;
            }
            finally
            {
                clock.Start();
            }//try
        }

    }
}