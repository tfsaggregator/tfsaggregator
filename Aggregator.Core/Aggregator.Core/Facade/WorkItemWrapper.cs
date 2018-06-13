using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Aggregator.Core.Navigation;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    public class WorkItemWrapper : WorkItemImplementationBase, IWorkItem
    {
        private readonly WorkItem workItem;

        private readonly IRuntimeContext context;

        public WorkItemWrapper(WorkItem workItem, IRuntimeContext context)
            : base(context)
        {
            this.workItem = workItem;
            this.context = context;
        }

        public WorkItemType Type
        {
            get
            {
                return this.workItem.Type;
            }
        }

        public bool ShouldLimit(RateLimiter limiter)
        {
            return limiter?.ShouldLimit(this.workItem) ?? false;
        }

        public string TypeName
        {
            get
            {
                return this.workItem.Type.Name;
            }
        }

        public string History
        {
            get
            {
                return this.workItem.History;
            }

            set
            {
                this.workItem.History = value;
            }
        }

        public int Id
        {
            get
            {
                return this.workItem.Id;
            }
        }

        public object this[string name]
        {
            get
            {
                // Ensure that this uses the FieldCollection and not directly accesses the
                // <code>workItem[name]</code> indexer, that would ignore the double fix.
                return this.Fields[name].Value;
            }

            set
            {
                this.Fields[name].Value = value;
            }
        }

        public IFieldCollection Fields
        {
            get
            {
                return new FieldCollectionWrapper(this.workItem.Fields, this.context);
            }
        }

        public Uri Uri
        {
            get
            {
                return this.workItem.Uri;
            }
        }

        public bool IsValid()
        {
            return this.workItem.IsValid();
        }

        public ArrayList Validate()
        {
            return this.workItem.Validate();
        }

        public void PartialOpen()
        {
            this.workItem.PartialOpen();
        }

        public void Save()
        {
            this.workItem.Save();
        }

        public void RevertChanges()
        {
            this.workItem.Reset();
        }

        public void TryOpen()
        {
            try
            {
                this.workItem.Open();
            }
            catch (Exception e)
            {
                this.Logger.WorkItemWrapperTryOpenException(this, e);
            }
        }

        public bool IsDirty
        {
            get
            {
                return this.workItem.IsDirty;
            }
        }

        public override IWorkItemLinkCollection WorkItemLinksImpl
        {
            get
            {
                return new WorkItemLinkCollectionWrapper(this.workItem.WorkItemLinks, this.context);
            }
        }

        public IWorkItemLinkExposedCollection WorkItemLinks
        {
            get
            {
                return new WorkItemLinkExposedCollectionWrapper(this.workItem.WorkItemLinks, this.context);
            }
        }

        IWorkItemType IWorkItem.Type
        {
            get
            {
                return new WorkItemTypeWrapper(this.Type);
            }
        }

        public DateTime RevisedDate
        {
            get
            {
                return this.workItem.RevisedDate;
            }
        }

        public int Revision
        {
            get
            {
                return this.workItem.Revision;
            }
        }

        public IRevision LastRevision
        {
            get
            {
                return new RevisionWrapper(this.workItem.Revisions[this.workItem.Revisions.Count - 1], this.context);
            }
        }

        public IRevision PreviousRevision
        {
            get
            {
                int targetRevision = this.workItem.Revision - 1;

                if (targetRevision <= 1)
                {
                    targetRevision = 1;
                }

                return new RevisionWrapper(this.workItem.Revisions[targetRevision - 1], this.context);
            }
        }

        public IRevision NextRevision
        {
            get
            {
                int targetRevision = this.workItem.Revision + 1;

                if (targetRevision >= this.workItem.Revisions.Count + 1)
                {
                    targetRevision = this.workItem.Revisions.Count - 1;
                }

                return new RevisionWrapper(this.workItem.Revisions[targetRevision], this.context);
            }
        }

        public IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query)
        {
            return WorkItemLazyVisitor
                .MakeRelativesLazyVisitor(this, query);
        }

        public void TransitionToState(string state, string comment)
        {
            StateWorkFlow.TransitionToState(this, state, comment, this.Logger);
        }

        public void AddWorkItemLink(IWorkItemExposed destination, string linkTypeName)
        {
            IEnumerable<WorkItemLinkType> availableLinkTypes = this.workItem.Store.WorkItemLinkTypes;
            this.AddWorkItemLink(destination, linkTypeName, availableLinkTypes);
        }

        internal void AddWorkItemLink(IWorkItemExposed destination, string linkTypeName, IEnumerable<WorkItemLinkType> availableLinkTypes)
        {
            WorkItemLinkType workItemLinkType = availableLinkTypes
                .FirstOrDefault(
                    t => new string[] { t.ForwardEnd.ImmutableName, t.ForwardEnd.Name, t.ReverseEnd.ImmutableName, t.ReverseEnd.Name }
                        .Contains(linkTypeName, StringComparer.OrdinalIgnoreCase));

            if (workItemLinkType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(linkTypeName));
            }

            WorkItemLinkTypeEnd destLinkType;
#pragma warning disable S3240
            if (
                new string[] { workItemLinkType.ForwardEnd.ImmutableName, workItemLinkType.ForwardEnd.Name }
                    .Contains(linkTypeName, StringComparer.OrdinalIgnoreCase))
            {
                destLinkType = workItemLinkType.ForwardEnd;
            }
            else
            {
                destLinkType = workItemLinkType.ReverseEnd;
            }
#pragma warning restore S3240

            var relationship = new WorkItemLink(destLinkType, this.Id, destination.Id);

            // check it does not exist already
            if (!this.workItem.WorkItemLinks.Contains(relationship))
            {
                this.Logger.AddingWorkItemLink(this.Id, destLinkType, destination.Id);
                this.workItem.WorkItemLinks.Add(relationship);
            }
            else
            {
                this.Logger.WorkItemLinkAlreadyExists(this.Id, destLinkType, destination.Id);
            }
        }

        public void AddHyperlink(string destination)
        {
            this.AddHyperlink(destination, string.Empty);
        }

        public void AddHyperlink(string destination, string comment)
        {
            var link = new Hyperlink(destination);
            link.Comment = comment ?? string.Empty;
            if (!this.workItem.Links.Contains(link))
            {
                this.Logger.AddingHyperlink(this.Id, destination, comment);
                this.workItem.Links.Add(link);
            }
            else
            {
                this.Logger.HyperlinkAlreadyExists(this.Id, destination, comment);
            }
        }

        public void RemoveWorkItemLink(IWorkItemLinkExposed link)
        {
            bool deleted = false;
            foreach (WorkItemLink item in this.workItem.WorkItemLinks)
            {
                if (item.SourceId == this.Id
                    && item.TargetId == link.Target.Id
                    && item.LinkTypeEnd.ImmutableName == link.LinkTypeEndImmutableName)
                {
                    this.Logger.RemovingWorkItemLink(item);
                    this.workItem.WorkItemLinks.Remove(item);
                    deleted = true;
                    break;
                }
            }
            if (!deleted)
            {
                this.Logger.WorkItemLinkNotFound(link);
            }
        }
    }
}
