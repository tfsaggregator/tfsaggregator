using Aggregator.Core;
using Aggregator.Core.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Aggregator.ServerPlugin
{
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
