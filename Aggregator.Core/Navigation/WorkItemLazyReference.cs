using Aggregator.Core.Facade;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core.Navigation
{
    /// <summary>
    /// Lazy navigation
    /// </summary>
    public class WorkItemLazyReference : IWorkItemExposed
    {
        int targetWorkItemId;
        IWorkItemRepository store;
        IWorkItem targetWorkItem;

        public static readonly string ParentRelationship = "System.LinkTypes.Hierarchy-Reverse";
        public static readonly string ChildRelationship = "System.LinkTypes.Hierarchy-Reverse";

        public static WorkItemLazyReference MakeParentLazyReference(IWorkItem sourceItem, IWorkItemRepository store)
        {
            int targetWorkItemId = (from IWorkItemLink workItemLink in sourceItem.WorkItemLinks
                                    where workItemLink.LinkTypeEndImmutableName == ParentRelationship
                                    select workItemLink.TargetId).FirstOrDefault();
            return new WorkItemLazyReference(targetWorkItemId, store);
        }

        public static IEnumerable<WorkItemLazyReference> MakeChildrenLazyReferences(IWorkItem sourceItem, IWorkItemRepository store)
        {
            return (from IWorkItemLink workItemLink in sourceItem.WorkItemLinks
                   where workItemLink.LinkTypeEndImmutableName == ChildRelationship
                   select new WorkItemLazyReference( workItemLink.TargetId, store));
        }

        /// <summary>
        /// This constructor is designed for Mocking
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="relationship"></param>
        /// <param name="store"></param>
        public WorkItemLazyReference(int targetWorkItemId, IWorkItemRepository store)
        {
            this.targetWorkItemId = targetWorkItemId;
            this.store = store;
            this.targetWorkItem = null;
        }

        protected IWorkItemExposed Target
        {
            get
            {
                if (targetWorkItem == null)
                {
                    targetWorkItem = store.GetWorkItem(targetWorkItemId);
                }
                return targetWorkItem;
            }
        }

        public IFieldCollectionWrapper Fields
        {
            get { return this.Target.Fields; }
        }

        public TType GetField<TType>(string fieldName, TType defaultValue)
        {
            return this.Target.GetField<TType>(fieldName, defaultValue);
        }

        public string History
        {
            get
            {
                return this.Target.History;
            }
            set
            {
                this.Target.History = value;
            }
        }

        public int Id
        {
            get { return targetWorkItemId; }
        }

        public bool IsValid()
        {
            return this.Target.IsValid();
        }

        public object this[string name]
        {
            get
            {
                return this.Target[name];
            }
            set
            {
                this.Target[name] = value;
            }
        }

        public string TypeName
        {
            get { return this.Target.TypeName; }
        }

        public IWorkItemExposed Parent
        {
            get
            {
                return this.Target.Parent;
            }
        }


        public IEnumerable<IWorkItemExposed> Children
        {
            get { return this.Target.Children; }
        }

        public IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query)
        {
            return this.Target.GetRelatives(query);
        }

        public FluentQuery WhereTypeIs(string workItemType)
        {
            return new FluentQuery(this).WhereTypeIs(workItemType);
        }

        public FluentQuery AtMost(int levels)
        {
            return new FluentQuery(this).AtMost(levels);
        }

        public FluentQuery FollowingLinks(string linkType)
        {
            return new FluentQuery(this).FollowingLinks(linkType);
        }


        public void TransitionToState(string state, string comment)
        {
            this.Target.TransitionToState(state, comment);
        }
    }
}
