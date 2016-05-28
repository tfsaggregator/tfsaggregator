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

        public override IWorkItemLinkCollection WorkItemLinks
        {
            get
            {
                return new WorkItemLinkCollectionWrapper(this.workItem, this.workItem.WorkItemLinks, this.context);
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
            this.DoAddWorkItemLink(destination, linkTypeName);
        }

        public override IWorkItemLink MakeLink(IWorkItemLinkType workItemLinkType, IWorkItemExposed source, IWorkItemExposed destination)
        {
            var fwd = this.workItem.Store.WorkItemLinkTypes.First(t => t.ForwardEnd.ImmutableName == workItemLinkType.ForwardEndImmutableName);
            return new WorkItemLinkWrapper(new WorkItemLink(fwd.ForwardEnd, source.Id, destination.Id), this.context);
        }

        public void RemoveWorkItemLink(IWorkItemExposed destination, string linkTypeName)
        {
            this.DoRemoveWorkItemLink(destination, linkTypeName);
        }

        public void RemoveWorkItemLinks(string linkTypeName)
        {
            throw new NotImplementedException();
        }

        public void AddHyperlink(string destination)
        {
            this.AddHyperlink(destination, string.Empty);
        }

        public void AddHyperlink(string destination, string comment = "")
        {
            var link = new Hyperlink(destination);
            link.Comment = comment;
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
    }
}
