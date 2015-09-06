namespace Aggregator.Core.Monitoring
{
    public interface IRuleLogger : ITextLogger
    {
        string RuleName { get; set; }

        void Log(string s);

        void Log(string format, params object[] args);
    }
}
