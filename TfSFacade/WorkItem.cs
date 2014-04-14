using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;


namespace TFSAggregator.TfsFacade
{
    public class WorkItem
    {
        private TFS.WorkItem workItem;
        public WorkItem(TFS.WorkItem workItem)
        {
            this.workItem = workItem;
        }


    }
}
