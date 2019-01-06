namespace Aggregator.Core.Monitoring
{
    public interface ILogger
    {
        LogLevel MinimumLogLevel { get; set; }
    }
}