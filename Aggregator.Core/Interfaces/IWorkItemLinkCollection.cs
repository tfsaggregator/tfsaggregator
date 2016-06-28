using System.Collections.Generic;

namespace Aggregator.Core.Interfaces
{
    public interface IWorkItemLinkCollection : IEnumerable<IWorkItemLink>
    {
        bool Contains(IWorkItemLink link);

        IEnumerable<IWorkItemLink> Filter(string linkTypeName);

        void Add(IWorkItemLink link);

        void Remove(IWorkItemLink link);
    }
}
