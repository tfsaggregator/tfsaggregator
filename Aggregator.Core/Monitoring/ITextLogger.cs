using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Monitoring
{
    public interface ITextLogger
    {
        LogLevel Level { get; set; }
        void Log(LogLevel level, string format, params object[] args);
    }
}
