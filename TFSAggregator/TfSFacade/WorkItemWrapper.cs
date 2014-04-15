using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFSAggregator.TfSFacade;
using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;


namespace TFSAggregator.TfsFacade
{
    public class WorkItemWrapper : IWorkItem
    {
        private TFS.WorkItem workItem;
        public WorkItemWrapper(TFS.WorkItem workItem)
        {
            this.workItem = workItem;
        }

        public string TypeName { get {return workItem.Type.Name;}}

        public string History { 
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

        public TFS.FieldCollection Fields { get { return workItem.Fields; } }

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
                MiscHelpers.LogMessage(String.Format("Unable to open work item '{0}'\nException: {1}", workItem.Id.ToString(), e.Message), true);
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
        /// <param name="fieldName">The name of the field to be retrived</param>
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
    }
}
