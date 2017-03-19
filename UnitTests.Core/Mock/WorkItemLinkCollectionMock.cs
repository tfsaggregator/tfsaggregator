using System;
using System.Collections;
using System.Collections.Generic;

using Aggregator.Core.Interfaces;

namespace UnitTests.Core.Mock
{
    internal class WorkItemLinkCollectionMock : IWorkItemLinkCollection
    {
        private readonly List<IWorkItemLink> links = new List<IWorkItemLink>();

        public bool Contains(IWorkItemLink link)
        {
            return this.links.Contains(link);
        }

        public void Add(IWorkItemLink link)
        {
            this.links.Add(link);
        }

        public IEnumerator<IWorkItemLink> GetEnumerator()
        {
            return this.links.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.links.GetEnumerator();
        }

        internal void Remove(WorkItemLinkMock relationship)
        {
            throw new NotImplementedException();
        }
    }
}
