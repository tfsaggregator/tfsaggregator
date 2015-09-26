using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Monitoring
{
    public class RuleLogger : IRuleLogger
    {
        private const LogLevel DefaultRuleLogLevel = LogLevel.Verbose;

        private readonly ILogEvents logger;

        public RuleLogger(ILogEvents logger)
        {
            this.logger = logger;
        }

        public string RuleName { get; set; }

        public void Log(string format, params object[] args)
        {
            var message = string.Format(format, args);
            this.CoreLog(DefaultRuleLogLevel, message);
        }

        public void Log(string message)
        {
            this.CoreLog(DefaultRuleLogLevel, message);
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            var message = string.Format(format, args);
            this.CoreLog(level, message);
        }

        public void Log(LogLevel level, string message)
        {
            this.CoreLog(level, message);
        }

        protected void CoreLog(LogLevel level, string message)
        {
            this.logger.ScriptLog(level, this.RuleName, message);
        }
    }
}
