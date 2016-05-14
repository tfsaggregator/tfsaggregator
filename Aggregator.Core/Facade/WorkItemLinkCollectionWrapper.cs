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

        private readonly WeakReference<WorkItem> workItem;

        private readonly WorkItemLinkCollection workItemLinkCollection;

        public WorkItemLinkCollectionWrapper(WorkItem workItem, WorkItemLinkCollection workItemLinkCollection, IRuntimeContext context)
        {
            this.logger = context.Logger;
            this.workItem = new WeakReference<WorkItem>(workItem);
            this.workItemLinkCollection = workItemLinkCollection;
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
            WorkItem target;
            if (!this.workItem.TryGetTarget(out target))
            {
                throw new InvalidOperationException("Lost reference to Work Item for link Collection while Adding a new Link");
            }

            // no need to be lousy here
            var linkType = target.Store.WorkItemLinkTypes.FirstOrDefault(t => t.ForwardEnd.ImmutableName == link.LinkTypeEndImmutableName);
            var relationship = new WorkItemLink(linkType.ForwardEnd, link.TargetId);

            // check it does not exist already
            if (!this.workItemLinkCollection.Contains(relationship))
            {
                this.logger.AddingWorkItemLink(target.Id, linkType.ForwardEnd, link.TargetId);
                this.workItemLinkCollection.Add(relationship);
            }
            else
            {
                this.logger.WorkItemLinkAlreadyExists(target.Id, linkType.ForwardEnd, link.TargetId);
            }
        }

        public bool Contains(IWorkItemLink link)
        {
            WorkItemLink item = ((WorkItemLinkWrapper)link).WorkItemLink;
            return this.workItemLinkCollection.Contains(item);
        }

        public IEnumerable<IWorkItemLink> Filter(string linkTypeName)
        {
            // be kind and accept lousy references to a link type
            var workItemLinkType = this.store.WorkItemLinkTypes.FirstOrDefault(
                    t => new string[] { t.ReferenceName, t.ForwardEndImmutableName, t.ForwardEndName, t.ReverseEndImmutableName, t.ReverseEndName }
                        .Contains(linkTypeName, StringComparer.OrdinalIgnoreCase));
            if (workItemLinkType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(linkTypeName));
            }

            foreach (WorkItemLink link in this.workItemLinkCollection.Cast<WorkItemLink>())
            {
                if (link.LinkTypeEnd.ImmutableName == workItemLinkType.ForwardEndImmutableName
                    || link.LinkTypeEnd.ImmutableName == workItemLinkType.ReverseEndImmutableName)
                {
                    yield return new WorkItemLinkWrapper(link, this.context);
                }
            }
        }

        public void Remove(IWorkItemLink link)
        {
            WorkItem target;
            if (!this.workItem.TryGetTarget(out target))
            {
                throw new InvalidOperationException("Lost reference to Work Item for link Collection while Removing Link");
            }

            // no need to be lousy here
            var linkType = target.Store.WorkItemLinkTypes.FirstOrDefault(t => t.ForwardEnd.ImmutableName == link.LinkTypeEndImmutableName);
            var relationship = new WorkItemLink(linkType.ForwardEnd, link.TargetId);

            // check it does not exist already
            if (!this.workItemLinkCollection.Contains(relationship))
            {
                this.logger.WorkItemLinkDoesNotExists(target.Id, linkType.ForwardEnd, link.TargetId);
            }
            else
            {
                this.logger.RemovingWorkItemLink(target.Id, linkType.ForwardEnd, link.TargetId);
                this.workItemLinkCollection.Remove(relationship);
            }
        }
    }
}
