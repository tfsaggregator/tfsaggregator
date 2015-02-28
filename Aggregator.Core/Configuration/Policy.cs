using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core.Configuration
{
    public class Policy
    {
        public string Name { get; set; }
        public Scope Scope { get; set; }
        public IEnumerable<Rule> Rules { get; set; }
    }
}
