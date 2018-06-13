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
    internal class WorkItemLinkExposedCollectionWrapper : IWorkItemLinkExposedCollection
    {
        private readonly ILogEvents logger;

        private readonly IWorkItemRepository store;

        private readonly IRuntimeContext context;

        private readonly IEnumerable<WorkItemLink> workItemLinkCollection;

        public WorkItemLinkExposedCollectionWrapper(WorkItemLinkCollection workItemLinkCollection, IRuntimeContext context)
            : this(context)
        {
            this.workItemLinkCollection = workItemLinkCollection.Cast<WorkItemLink>();
        }

        public WorkItemLinkExposedCollectionWrapper(LinkCollection linkCollection, IRuntimeContext context)
            : this(context)
        {
            this.workItemLinkCollection = linkCollection.OfType<WorkItemLink>();
        }

        private WorkItemLinkExposedCollectionWrapper(IRuntimeContext context)
        {
            this.logger = context.Logger;
            this.store = context.WorkItemRepository;
            this.context = context;
        }

        public IEnumerator<IWorkItemLinkExposed> GetEnumerator()
        {
            foreach (WorkItemLink item in this.workItemLinkCollection)
            {
                yield return new WorkItemLinkExposedWrapper(item, this.context);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}