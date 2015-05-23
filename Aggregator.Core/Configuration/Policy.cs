namespace Aggregator.Core.Configuration
{
    using System.Collections.Generic;

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
