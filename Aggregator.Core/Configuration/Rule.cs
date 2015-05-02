using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// Represents a Rule of <see cref="TFSAggregatorSettings"/>.
    /// </summary>
    public class Rule
    {
        public string Name { get; set; }
        
        public RuleScope[] Scope { get; set; }
        
        public string Script { get; set; }
    }
}
