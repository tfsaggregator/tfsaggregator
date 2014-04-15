using System;
using Microsoft.TeamFoundation.Client;
using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;
using TFSAggregator.TfsFacade;
using TFSAggregator.TfSFacade;

namespace TFSAggregator.TfsSpecific
{
    /// <summary>
    /// Singleton Used to access TFS Data.  This keeps us from connecting each and every time we get an update.
    /// </summary>
    public class WorkItemRepository : IWorkItemRepository
    {
        private readonly string tfsServerUrl;
        private TFS.WorkItemStore workItemStore;

        public WorkItemRepository(string tfsServerUrl)
        {
            this.tfsServerUrl = tfsServerUrl;
        }

        private void ConnectToWorkItemStore()
        {
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(tfsServerUrl));
            workItemStore = (TFS.WorkItemStore)tfs.GetService(typeof(TFS.WorkItemStore));
        }

        public IWorkItem GetWorkItem(int workItemId)
        {
            if (workItemStore == null)
            {
                ConnectToWorkItemStore();
            }
            return new WorkItemWrapper(workItemStore.GetWorkItem(workItemId));
        }
    }
}