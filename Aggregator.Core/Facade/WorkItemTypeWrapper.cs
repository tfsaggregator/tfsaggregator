namespace Aggregator.Core.Facade
{
    using System.Xml;

    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    public class WorkItemTypeWrapper : IWorkItemType
    {
        private WorkItemType type;

        public WorkItemTypeWrapper(WorkItemType type)
        {
            this.type = type;
        }

        public string Name
        {
            get
            {
                return this.type.Name;
            }
        }

        public XmlDocument Export(bool includeGlobalListsFlag)
        {
            return this.type.Export(includeGlobalListsFlag);
        }
    }
}
