using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Aggregator.Core.Navigation;

    public class WorkItemWrapper : WorkItemImplementationBase, IWorkItem
    {
        private TFS.WorkItem workItem;

        public WorkItemWrapper(TFS.WorkItem workItem, IWorkItemRepository store, ILogEvents logger)
            : base(store, logger)
        {
            this.workItem = workItem;
        }

        public TFS.WorkItemType Type { get { return this.workItem.Type; } }

        public string TypeName { get { return this.workItem.Type.Name; } }

        public string History
        {
            get
            {
                return this.workItem.History;
            }
            set {
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
                return this.workItem[name];
            }
            set
            {
                this.workItem[name] = value;
            }
        }

        public IFieldCollectionWrapper Fields
        {
            get
            {
                return new FieldCollectionWrapper(this.workItem.Fields);
            }
        }

        public bool IsValid() { return this.workItem.IsValid(); }

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
                this.logger.WorkItemWrapperTryOpenException(this, e);
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
                return new WorkItemLinkCollectionWrapper(this.workItem.WorkItemLinks, this.store, this.logger);
            }
        }

        IWorkItemType IWorkItem.Type
        {
            get
            {
                return new WorkItemTypeWrapper(this.Type);
            }
        }

        public IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query)
        {
            return WorkItemLazyVisitor
                .MakeRelativesLazyVisitor(this, query, this.store);
        }

        public void TransitionToState(string state, string comment)
        {
            StateWorkFlow.TransitionToState(this, state, comment, this.logger);
        }

        public void AddWorkItemLink(IWorkItemExposed destination, string linkTypeName)
        {
            var destLinkType = this.workItem.Store.WorkItemLinkTypes
                .FirstOrDefault(t => t.ForwardEnd.Name == linkTypeName)
                .ForwardEnd;
            var relationship = new TFS.WorkItemLink(destLinkType, this.Id, destination.Id);
            // check it does not exist already
            if (!this.workItem.WorkItemLinks.Contains(relationship))
            {
                logger.AddingWorkItemLink(this.Id, destLinkType, destination.Id);
                this.workItem.WorkItemLinks.Add(relationship);
            }
            else
            {
                logger.WorkItemLinkAlreadyExists(this.Id, destLinkType, destination.Id);
            }//if
        }

        public void AddHyperlink(string destination, string comment = "")
        {
            var link = new TFS.Hyperlink(destination);
            link.Comment = comment;
            if (!this.workItem.Links.Contains(link))
            {
                logger.AddingHyperlink(this.Id, destination, comment);
                this.workItem.Links.Add(link);
            }
            else
            {
                logger.HyperlinkAlreadyExists(this.Id, destination, comment);
            }//if
        }
    }
}
