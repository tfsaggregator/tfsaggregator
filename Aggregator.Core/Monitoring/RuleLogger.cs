using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Monitoring
{
    public class RuleLogger : IRuleLogger
    {
        private readonly ILogEvents logger;

        public RuleLogger(ILogEvents logger)
        {
            this.logger = logger;
        }

        public string RuleName { get; set; }

        // forward to implementation
        public LogLevel MinimumLogLevel
        {
            get
            {
                return this.logger.MinimumLogLevel;
            }

            set
            {
                this.logger.MinimumLogLevel = value;
            }
        }

        public void Log(string format, params object[] args)
        {
            var s = string.Format(format, args);
            this.Log(s);
        }

        public void Log(string s)
        {
            this.logger.ScriptLog(this.RuleName, s);
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            this.Log(format, args);
        }
    }
}
