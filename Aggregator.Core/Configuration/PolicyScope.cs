using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// Represents a Scope clause of <see cref="TFSAggregatorSettings"/>.
    /// </summary>
    public abstract class PolicyScope
    {
        /// <summary>
        /// Each <see cref="PolicyScope"/> must implement this method to decide whether the policy should activate
        /// for the given requestcontext.
        /// </summary>
        /// <param name="requestContext">The requestcontext of the TFS event</param>
        /// <param name="notification">The notification holding the WorkItemChangedEvent.</param>
        /// <returns>True if the policy should trigger on this request.</returns>
        public abstract bool Matches(IRequestContext requestContext, INotification notification);
    }
}
