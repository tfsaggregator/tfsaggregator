namespace Aggregator.ServerPlugin
{
    using Aggregator.Core;
    using Aggregator.Core.Monitoring;

    class ServerEventLogger : TextLogComposer
    {
        public ServerEventLogger(LogLevel level)
            : base(new ServerTextLogger(level))
        { }
    }
}
