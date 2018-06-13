namespace Aggregator.WebHooks.Utils
{
    using Aggregator.Core.Monitoring;

    internal class AspNetEventLogger : TextLogComposer, ILogEvents2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticTraceLogger"/> class.
        /// </summary>
        /// <param name="correlationId">Request identifier</param>
        /// <param name="minimumLogLevel">The minimum log level to show.</param>
        public AspNetEventLogger(string correlationId, LogLevel minimumLogLevel)
            : base(new DiagnosticTraceLogger(correlationId, minimumLogLevel))
        {
        }

        public void BasicAuthenticationSucceeded(string userName)
        {
            this.TextLogger.Log(LogLevel.Verbose, $"User '{userName}' authenticated");
        }

        public void BasicAuthenticationFailed(string userName)
        {
            this.TextLogger.Log(LogLevel.Warning, $"User '{userName}' failed to authenticate!");
        }
    }
}