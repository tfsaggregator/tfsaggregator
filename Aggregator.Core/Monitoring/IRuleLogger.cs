namespace Aggregator.Core.Monitoring
{
    /// <summary>
    /// Logging feature offered to scripted Rules
    /// </summary>
    public interface IRuleLogger
    {
        string RuleName { get; set; }

        void Log(string message);

        void Log(string format, params object[] args);

        void Log(LogLevel level, string message);

        void Log(LogLevel level, string format, params object[] args);
    }
}
