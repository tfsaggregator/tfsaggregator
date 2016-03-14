using Aggregator.Core.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aggregator.WebHooks.Utils
{
    internal class AspNetEventLogger : TextLogComposer
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
    }
}