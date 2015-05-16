namespace Aggregator.ServerPlugin
{
    using Aggregator.Core;
    using Aggregator.Core.Monitoring;

    class ServerEventLogger : TextLogComposer
    {
        public ServerEventLogger(LogLevel level)
            : base(new ServerTextLogger(level))
        { }

        public LogLevel Level
        {
            get { return base.TextLogger.Level; }
            set { base.TextLogger.Level = value; }
        }
    }
}
