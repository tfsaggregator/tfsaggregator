using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    public abstract class RuleScope
    {
        public abstract bool Matches(IWorkItem item);
    }
}