namespace Aggregator.Core
{
    using System.Collections.Generic;

    public interface IWorkItemLinkCollection : IEnumerable<IWorkItemLink>
    {
        // Add is only valid on mocks
        void Add(IWorkItemLink link);
    }
}
