using System.Xml;

using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    public class WorkItemTypeWrapper : IWorkItemType
    {
        private readonly WorkItemType type;

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
