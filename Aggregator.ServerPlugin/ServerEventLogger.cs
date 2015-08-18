using Aggregator.Core;
using Aggregator.Core.Monitoring;

namespace Aggregator.ServerPlugin
{
    internal class ServerEventLogger : TextLogComposer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerEventLogger"/> class.
        /// </summary>
        /// <param name="minLevel">The minimum level (inclusive) to write to the log.</param>
        public ServerEventLogger(LogLevel minLevel)
            : base(new ServerTextLogger(minLevel))
        {
        }
    }
}
