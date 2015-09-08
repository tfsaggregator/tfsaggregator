using Aggregator.Core.Monitoring;

namespace UnitTests.Core
{
    /// <summary>
    /// Handy class for Unit testing: all logging appears in the Debug output
    /// </summary>
    internal class DebugTextLogger : ITextLogger
    {
        public LogLevel MinimumLogLevel
        {
            get;
            set;
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            string message = args != null ? string.Format(format, args: args) : format;
            string levelAsString = level.ToString();
            System.Diagnostics.Debug.WriteLine(message, levelAsString);
        }
    }
}
