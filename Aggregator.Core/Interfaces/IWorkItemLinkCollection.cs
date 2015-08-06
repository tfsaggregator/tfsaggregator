namespace Aggregator.Core
{
    using System.Collections.Generic;

    public interface IWorkItemLinkCollection : IEnumerable<IWorkItemLink>
    {
        bool Contains(IWorkItemLink link);
        // Add is only valid on mocks
        void Add(IWorkItemLink link);
    }
}
