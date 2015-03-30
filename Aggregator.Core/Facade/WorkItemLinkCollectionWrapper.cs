using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core.Facade
{
    class WorkItemLinkCollectionWrapper : IWorkItemLinkCollection
    {
        ILogEvents logger;
        IWorkItemRepository store;
        private WorkItemLinkCollection workItemLinkCollection;

        public WorkItemLinkCollectionWrapper(WorkItemLinkCollection workItemLinkCollection, IWorkItemRepository store, ILogEvents logger)
        {
            this.logger = logger;
            this.workItemLinkCollection = workItemLinkCollection;
            this.store = store;
        }

        public IEnumerator<IWorkItemLink> GetEnumerator()
        {
            foreach (WorkItemLink item in this.workItemLinkCollection)
            {
                yield return new WorkItemLinkWrapper(item, this.store, this.logger);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(IWorkItemLink link)
        {
            throw new InvalidOperationException("Add is valid on mocks only");
        }
    }
}
