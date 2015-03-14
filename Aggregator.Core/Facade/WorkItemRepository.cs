using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.ObjectModel;

namespace Aggregator.Core.Facade
{
    /// <summary>
    /// Singleton used to access TFS Data.  This keeps us from connecting each and every time we get an update.
    /// Keeps track of all WorkItems pulled in memory that should be saved later.
    /// </summary>
    public class WorkItemRepository : IWorkItemRepository
    {
        ILogEvents logger;
        private readonly string tfsCollectionUrl;
        private WorkItemStore workItemStore;
        List<IWorkItem> loadedWorkItems = new List<IWorkItem>();

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
            IWorkItem justLoaded = new WorkItemWrapper(workItemStore.GetWorkItem(workItemId), this, this.logger);
            loadedWorkItems.Add(justLoaded);
            return justLoaded;
        }

        public ReadOnlyCollection<IWorkItem> LoadedWorkItems
        {
            get { return new ReadOnlyCollection<IWorkItem>(loadedWorkItems); }
        }
    }
}
