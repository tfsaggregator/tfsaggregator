using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// Represents a Policy of <see cref="TFSAggregatorSettings"/>.
    /// </summary>
    public class Policy
    {
        public string Name { get; set; }
        public IEnumerable<PolicyScope> Scope { get; set; }
        public IEnumerable<Rule> Rules { get; set; }
    }
}
