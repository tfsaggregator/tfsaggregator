using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    internal class WorkItemLinkCollectionWrapper : IWorkItemLinkCollection
    {
        private readonly ILogEvents logger;

        private readonly IWorkItemRepository store;

        private readonly IRuntimeContext context;

        private readonly IEnumerable<WorkItemLink> workItemLinkCollection;

        public WorkItemLinkCollectionWrapper(WorkItemLinkCollection workItemLinkCollection, IRuntimeContext context) 
            : this(context)
        {
            this.workItemLinkCollection = workItemLinkCollection.Cast<WorkItemLink>();
        }

        public WorkItemLinkCollectionWrapper(LinkCollection linkCollection, IRuntimeContext context)
            : this(context)
        {
            this.workItemLinkCollection = linkCollection.OfType<WorkItemLink>();
        }

        private WorkItemLinkCollectionWrapper(IRuntimeContext context)
        {
            this.logger = context.Logger;
            this.store = context.WorkItemRepository;
            this.context = context;
        }

        public IEnumerator<IWorkItemLink> GetEnumerator()
        {
            foreach (WorkItemLink item in this.workItemLinkCollection.Cast<WorkItemLink>())
            {
                yield return new WorkItemLinkWrapper(item, this.context);
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
