namespace UnitTests.Core.Mock
{
    using System.Collections;
    using System.Collections.Generic;

    using Aggregator.Core;

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return links.GetEnumerator();
        }
    }
}
