using Aggregator.Core.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aggregator.Core;

namespace UnitTests.Core
{
    /// <summary>
    /// Handy class for Unit testing: all logging appears in the Debug output
    /// </summary>
    class DebugTextLogger : ITextLogger
    {
        public LogLevel MinimumLogLevel
        {
            get;
            set;
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            string message = args != null ? string.Format(format, args: args) : format;
            string levelAsString = level.ToString();
            System.Diagnostics.Debug.WriteLine(message, levelAsString);
        }
    }

    class DebugEventLogger : TextLogComposer
    {
        public DebugEventLogger()
            : base(new DebugTextLogger())
        { }
    }

}
