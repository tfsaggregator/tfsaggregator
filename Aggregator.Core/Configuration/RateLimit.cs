using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Configuration
{
    public class RateLimit
    {
        public int Changes { get; set; }

        public TimeSpan Interval { get; set; }
    }
}
