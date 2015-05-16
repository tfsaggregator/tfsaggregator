namespace Aggregator.Core.Facade
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Framework.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    /// <summary>
    /// Singleton used to access TFS Data.  This keeps us from connecting each and every time we get an update.
    /// Keeps track of all WorkItems pulled in memory that should be saved later.
    /// </summary>
    public class WorkItemRepository : IWorkItemRepository
    {
        ILogEvents logger;
        private readonly string tfsCollectionUrl;

        private readonly IdentityDescriptor toImpersonate;
        private WorkItemStore workItemStore;
        List<IWorkItem> loadedWorkItems = new List<IWorkItem>();

        public WorkItemRepository(string tfsCollectionUrl, IdentityDescriptor toImpersonate, ILogEvents logger)
        {
            this.logger = logger;
            this.tfsCollectionUrl = tfsCollectionUrl;
            this.toImpersonate = toImpersonate;
        }

        private void ConnectToWorkItemStore()
        {
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(this.tfsCollectionUrl), this.toImpersonate);
            this.workItemStore = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
        }

        public IWorkItem GetWorkItem(int workItemId)
        {
            if (this.workItemStore == null)
            {
                this.ConnectToWorkItemStore();
            }
            IWorkItem justLoaded = new WorkItemWrapper(this.workItemStore.GetWorkItem(workItemId), this, this.logger);
            this.loadedWorkItems.Add(justLoaded);
            return justLoaded;
        }

        public ReadOnlyCollection<IWorkItem> LoadedWorkItems
        {
            get { return new ReadOnlyCollection<IWorkItem>(this.loadedWorkItems); }
        }
    }
}
