using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
    public interface IWorkItemLinkCollection : IEnumerable<IWorkItemLink>
    {
        void Add(IWorkItemLink link);
    }
}
