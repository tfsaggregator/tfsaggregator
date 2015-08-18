using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace Aggregator.Core.Facade
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    class WorkItemLinkCollectionWrapper : IWorkItemLinkCollection
    {
        readonly ILogEvents logger;

        readonly IWorkItemRepository store;
        private readonly WorkItemLinkCollection workItemLinkCollection;

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(IWorkItemLink link)
        {
            throw new InvalidOperationException("Add is valid on mocks only");
        }

        public bool Contains(IWorkItemLink link)
        {
            WorkItemLink item = ((WorkItemLinkWrapper)link).WorkItemLink;
            return this.workItemLinkCollection.Contains(item);
        }
    }
}
