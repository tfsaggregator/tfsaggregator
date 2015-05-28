namespace Aggregator.Core.Monitoring
{
    public interface ITextLogger : ILogger
    {
        void Log(LogLevel level, string format, params object[] args);
    }
}
