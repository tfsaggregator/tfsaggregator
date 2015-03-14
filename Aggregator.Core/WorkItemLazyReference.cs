using Aggregator.Core.Facade;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
    /// <summary>
    /// Lazy navigation
    /// </summary>
    public class WorkItemLazyReference : IWorkItem
    {
        int targetWorkItemId;
        IWorkItemRepository store;
        IWorkItem targetWorkItem;

        public static readonly string ParentRelationship = "System.LinkTypes.Hierarchy-Reverse";

        public WorkItemLazyReference(IWorkItem sourceItem, string relationship, IWorkItemRepository store)
        {
            this.targetWorkItemId = (from WorkItemLink workItemLink in sourceItem.WorkItemLinks
                                     where workItemLink.LinkTypeEnd.ImmutableName == relationship
                                     select workItemLink.TargetId).FirstOrDefault();
            this.store = store;
            this.targetWorkItem = null;
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

        protected IWorkItem Target
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

        public void PartialOpen()
        {
            this.Target.PartialOpen();
        }

        public void Save()
        {
            this.Target.Save();
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

        public void TryOpen()
        {
            this.Target.TryOpen();
        }

        public string TypeName
        {
            get { return this.Target.TypeName; }
        }

        public System.Collections.ArrayList Validate()
        {
            return this.Target.Validate();
        }

        public Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemLinkCollection WorkItemLinks
        {
            get { return this.Target.WorkItemLinks; }
        }

        public IWorkItem Parent
        {
            get
            {
                return this.Target.Parent;
            }
        }
    }
}
