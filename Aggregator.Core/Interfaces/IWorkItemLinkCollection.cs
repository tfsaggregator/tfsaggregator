using System.Collections.Generic;

namespace Aggregator.Core.Interfaces
{
    public interface IWorkItemLinkCollection : IEnumerable<IWorkItemLink>
    {
        bool Contains(IWorkItemLink link);

        /// <remarks>Add is only valid on mocks</remarks>
        void Add(IWorkItemLink link);
    }
}
