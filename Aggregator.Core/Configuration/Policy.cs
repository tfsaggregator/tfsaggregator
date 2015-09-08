using System.Collections.Generic;

namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// Represents a Policy of <see cref="TFSAggregatorSettings"/>.
    /// </summary>
    public class Policy
    {
        /// <summary>
        /// The name of the policy for easier reference
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Collection of zero or more scope rules which allow the user
        /// to scope the policy to a specific collection or project (for example).
        /// </summary>
        public IEnumerable<PolicyScope> Scope { get; set; }

        /// <summary>
        /// The rules that are part of this policy.
        /// </summary>
        public IEnumerable<Rule> Rules { get; set; }
    }
}
