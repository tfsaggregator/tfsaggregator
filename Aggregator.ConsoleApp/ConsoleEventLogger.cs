using Aggregator.Core;
using Aggregator.Core.Monitoring;

namespace Aggregator.ConsoleApp
{
    /// <summary>
    /// Logs all events to the console.
    /// </summary>
    internal class ConsoleEventLogger : TextLogComposer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleEventLogger"/> class.
        /// </summary>
        /// <param name="minimumLogLevel">The minimum log level to show.</param>
        public ConsoleEventLogger(LogLevel minimumLogLevel)
            : base(new ConsoleTextLogger(minimumLogLevel))
        { }
    }
}
