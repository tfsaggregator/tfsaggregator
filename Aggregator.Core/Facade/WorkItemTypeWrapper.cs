using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
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
