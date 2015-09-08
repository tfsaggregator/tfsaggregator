using System.Collections.Generic;

namespace Aggregator.Core.Interfaces
{
    public interface IWorkItemLinkCollection : IEnumerable<IWorkItemLink>
    {
        bool Contains(IWorkItemLink link);

        void Add(IWorkItemLink link);
    }
}
