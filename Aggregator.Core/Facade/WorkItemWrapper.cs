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

        public WorkItemWrapper(WorkItem workItem, IWorkItemRepository store, ILogEvents logger)
            : base(store, logger)
        {
            this.workItem = workItem;
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
                return this.Fields[name].Value;
            }

            set
            {
                this.Fields[name].Value = value;
            }
        }

        public IFieldCollectionWrapper Fields
        {
            get
            {
                return new FieldCollectionWrapper(this.workItem.Fields);
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
                return new WorkItemLinkCollectionWrapper(this.workItem.WorkItemLinks, this.Store, this.Logger);
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
                // works even on a new workitem with no revisions...
                return new RevisionWrapper(
                    this.workItem.Revisions[this.workItem.Revisions.Count - 1]);
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
            var destLinkType = this.workItem.Store.WorkItemLinkTypes
                .FirstOrDefault(t => t.ForwardEnd.Name == linkTypeName)
                .ForwardEnd;
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
