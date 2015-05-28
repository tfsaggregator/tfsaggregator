using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Aggregator.Core.Navigation;

    public class WorkItemWrapper : IWorkItem
    {
        ILogEvents logger;
        private TFS.WorkItem workItem;
        private IWorkItemRepository store;

        public WorkItemWrapper(TFS.WorkItem workItem, IWorkItemRepository store, ILogEvents logger)
        {
            this.logger = logger;
            this.workItem = workItem;
            this.store = store;
        }

        public TFS.WorkItemType Type { get { return this.workItem.Type; } }

        public string TypeName { get { return this.workItem.Type.Name; } }

        public bool HasParent()
        {
            return this.HasRelation(WorkItemLazyReference.ParentRelationship);
        }

        public bool HasChildren()
        {
            return this.HasRelation(WorkItemLazyReference.ChildRelationship);
        }

        public bool HasRelation(string relation)
        {
            if (string.IsNullOrWhiteSpace(relation))
            {
                throw new ArgumentNullException("relation");
            }

            foreach (var link in this.WorkItemLinks)
            {
                if (string.Equals(relation, link.LinkTypeEndImmutableName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

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

        public IWorkItemLinkCollection WorkItemLinks
        {
            get
            {
                return new WorkItemLinkCollectionWrapper(this.workItem.WorkItemLinks, this.store, this.logger);
            }
        }

        /// <summary>
        /// Used to convert a field to a number.  If anything goes wrong then the default value is returned.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="fieldName">The name of the field to be retrieved</param>
        /// <param name="defaultValue">Value to be returned if something goes wrong.</param>
        /// <returns></returns>
        public TType GetField<TType>(string fieldName, TType defaultValue)
        {
            try
            {
                TType convertedValue = (TType)this.workItem[fieldName];
                return convertedValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public IWorkItemExposed Parent
        {
            get
            {
                return WorkItemLazyReference.MakeParentLazyReference(this, this.store);
            }
        }

        public IEnumerable<IWorkItemExposed> Children
        {
            get
            {
                return WorkItemLazyReference.MakeChildrenLazyReferences(this, this.store);
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
            StateWorkflow.TransitionToState(this, state, comment, this.logger);
        }

        public void AddWorkItemLink(IWorkItemExposed destination, string linkTypeName)
        {
            var destLinkType = this.workItem.Store.WorkItemLinkTypes.Where(t => t.ForwardEnd.Name == linkTypeName).FirstOrDefault().ForwardEnd;
            var relationship = new TFS.WorkItemLink(destLinkType, this.Id, destination.Id);
            // check it does not exist already
            if (!this.workItem.WorkItemLinks.Contains(relationship))
            {
                //TODO logger.AddingWorkItemLink(this.Id, destLinkType, destination.Id);
                this.workItem.WorkItemLinks.Add(relationship);
            }
            else
            {
                //TODO logger.WorkItemLinkAlreadyExists(this.Id, destLinkType, destination.Id);
            }//if
        }

        public void AddHyperlink(string destination, string comment = "")
        {
            var link = new TFS.Hyperlink(destination);
            link.Comment = comment;
            if (!this.workItem.Links.Contains(link))
            {
                //TODO logger.AddingHyperlink(this.Id, destination, comment);
                this.workItem.Links.Add(link);
            }
            else
            {
                //TODO logger.HyperlinkAlreadyExists(this.Id, destination, comment);
            }
        }
    }
}
