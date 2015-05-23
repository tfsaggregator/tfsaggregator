namespace Aggregator.Core.Monitoring
{
    public interface ITextLogger
    {
        LogLevel Level { get; set; }
        void Log(LogLevel level, string format, params object[] args);
    }
}
