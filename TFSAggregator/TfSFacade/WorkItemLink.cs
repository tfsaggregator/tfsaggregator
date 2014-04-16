using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSAggregator.TfsFacade
{
    public class WorkItemLink
    {
        private TFS.WorkItemLink link;
        public WorkItemLink(TFS.WorkItemLink link)
        {
            this.link = link;
        }
    }
}
