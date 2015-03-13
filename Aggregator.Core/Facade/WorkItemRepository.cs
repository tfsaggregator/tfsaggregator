using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    /// <summary>
    /// Singleton Used to access TFS Data.  This keeps us from connecting each and every time we get an update.
    /// </summary>
    public class WorkItemRepository : IWorkItemRepository
    {
        ILogEvents logger;
        private readonly string tfsCollectionUrl;
        private WorkItemStore workItemStore;

        public WorkItemRepository(string tfsCollectionUrl, ILogEvents logger)
        {
            this.logger = logger;
            this.tfsCollectionUrl = tfsCollectionUrl;
        }

        private void ConnectToWorkItemStore()
        {
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(tfsCollectionUrl));
            workItemStore = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
        }

        public IWorkItem GetWorkItem(int workItemId)
        {
            if (workItemStore == null)
            {
                ConnectToWorkItemStore();
            }
            return new WorkItemWrapper(workItemStore.GetWorkItem(workItemId), this, this.logger);
        }
    }
}
