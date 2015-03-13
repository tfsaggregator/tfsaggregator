using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
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

        public TFS.WorkItemType Type { get { return workItem.Type; } }

        public string TypeName { get { return workItem.Type.Name; } }

        public string History
        {
            get { return workItem.History; }
            set { workItem.History = value; }
        }

        public int Id
        {
            get
            {
                return workItem.Id;
            }
        }

        public object this[string name]
        {
            get
            {
                return workItem[name];
            }
            set
            {
                workItem[name] = value;
            }
        }

        public IFieldCollectionWrapper Fields
        {
            get
            {
                return new FieldCollectionWrapper(workItem.Fields);
            }
        }

        public bool IsValid() { return workItem.IsValid(); }

        public ArrayList Validate()
        {
            return workItem.Validate();
        }

        public void PartialOpen() { workItem.PartialOpen(); }
        public void Save() { workItem.Save(); }

        public void TryOpen()
        {
            try
            {
                workItem.Open();
            }
            catch (Exception e)
            {
                logger.WorkItemWrapperTryOpenException(this, e);
            }
        }

        public TFS.WorkItemLinkCollection WorkItemLinks
        {
            get
            {
                return workItem.WorkItemLinks;
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
                TType convertedValue = (TType)workItem[fieldName];
                return convertedValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public IWorkItem Parent
        {
            get
            {
                return new WorkItemLazyReference(this, WorkItemLazyReference.ParentRelationship, store);
            }
        }
    }
}
