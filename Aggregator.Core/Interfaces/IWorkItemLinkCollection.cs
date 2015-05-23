namespace Aggregator.Core
{
    using System.Collections.Generic;

    public interface IWorkItemLinkCollection : IEnumerable<IWorkItemLink>
    {
        void Add(IWorkItemLink link);
    }
}
