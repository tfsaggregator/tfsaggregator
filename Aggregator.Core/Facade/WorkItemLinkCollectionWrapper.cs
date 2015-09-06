using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    internal class WorkItemLinkCollectionWrapper : IWorkItemLinkCollection
    {
        private readonly ILogEvents logger;

        private readonly IWorkItemRepository store;

        private readonly WorkItemLinkCollection workItemLinkCollection;

        public WorkItemLinkCollectionWrapper(WorkItemLinkCollection workItemLinkCollection, IWorkItemRepository store, ILogEvents logger)
        {
            this.logger = logger;
            this.workItemLinkCollection = workItemLinkCollection;
            this.store = store;
        }

        public IEnumerator<IWorkItemLink> GetEnumerator()
        {
            foreach (WorkItemLink item in this.workItemLinkCollection.Cast<WorkItemLink>())
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
