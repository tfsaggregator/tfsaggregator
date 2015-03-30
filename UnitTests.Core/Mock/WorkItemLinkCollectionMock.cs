using Aggregator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core.Mock
{
    class WorkItemLinkCollectionMock : IWorkItemLinkCollection
    {
        List<IWorkItemLink> links = new List<IWorkItemLink>();

        public void Add(IWorkItemLink link)
        {
            links.Add(link);
        }

        public IEnumerator<IWorkItemLink> GetEnumerator()
        {
            return links.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return links.GetEnumerator();
        }
    }
}
