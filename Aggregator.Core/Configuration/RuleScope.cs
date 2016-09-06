using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// Represents a Scope clause at Rule level of <see cref="TFSAggregatorSettings"/>.
    /// </summary>
    public abstract class RuleScope
    {
        /// <summary>
        /// Descriptive string that shortly describe the Rule scope.
        /// Used in logging.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Each <see cref="RuleScope"/> must implement this method to decide whether the rule should activate
        /// for the given work item.
        /// </summary>
        /// <param name="item">The Work Item of the TFS event</param>
        /// <returns>True if the policy should trigger on this request.</returns>
        public abstract ScopeMatchResult Matches(IWorkItem item, INotification notification);
    }
}