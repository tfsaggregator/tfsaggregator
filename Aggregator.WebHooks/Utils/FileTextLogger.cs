namespace Aggregator.WebHooks.Utils
{
    using System.Diagnostics;
    using Aggregator.Core.Monitoring;

    internal class FileTextLogger : ITextLogger
    {
        private readonly Stopwatch clock = new Stopwatch();

        internal FileTextLogger(string logFile, LogLevel minimumLogLevel)
        {
            this.MinimumLogLevel = minimumLogLevel;
            this.TextLogFile = logFile;
            this.clock.Restart();
        }

        public LogLevel MinimumLogLevel { get; set; }

        public string TextLogFile { get; private set; }

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

                const int LogLevelMaximumStringLength = 11; // Len(Information)
                string levelAsString = level.ToString();

                using (var f = new System.IO.StreamWriter(this.TextLogFile, true))
                {
                    f.Write(
                        "[{0}]{1} {2:00}.{3:000} ",
                        levelAsString,
                        string.Empty.PadLeft(LogLevelMaximumStringLength - levelAsString.Length),
                        this.clock.ElapsedMilliseconds / 1000,
                        this.clock.ElapsedMilliseconds % 1000);

                    f.WriteLine(message);
                }//using
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