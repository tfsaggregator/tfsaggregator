using Aggregator.Core.Monitoring;

namespace UnitTests.Core
{
    internal class DebugEventLogger : TextLogComposer
    {
        public DebugEventLogger()
            : base(new DebugTextLogger())
        {
        }
    }
}
