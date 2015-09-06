using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

using IdentityDescriptor = Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor;

namespace Aggregator.Core.Facade
{
    /// <summary>
    /// Singleton used to access TFS Data.  This keeps us from connecting each and every time we get an update.
    /// Keeps track of all WorkItems pulled in memory that should be saved later.
    /// </summary>
    public class WorkItemRepository : IWorkItemRepository, IDisposable
    {
        private readonly ILogEvents logger;

        private readonly string tfsCollectionUrl;

        private readonly IdentityDescriptor toImpersonate;

        private readonly List<IWorkItem> loadedWorkItems = new List<IWorkItem>();

        private WorkItemStore workItemStore;

        private TfsTeamProjectCollection tfs;

        public WorkItemRepository(string tfsCollectionUrl, IdentityDescriptor toImpersonate, ILogEvents logger)
        {
            this.logger = logger;
            this.tfsCollectionUrl = tfsCollectionUrl;
            this.toImpersonate = toImpersonate;
        }

        private void ConnectToWorkItemStore()
        {
            this.tfs = new TfsTeamProjectCollection(new Uri(this.tfsCollectionUrl), this.toImpersonate);
            this.workItemStore = (WorkItemStore)this.tfs.GetService(typeof(WorkItemStore));
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
            get
            {
                return new ReadOnlyCollection<IWorkItem>(this.loadedWorkItems);
            }
        }

        public IWorkItem MakeNewWorkItem(string projectName, string workItemTypeName)
        {
            if (this.workItemStore == null)
            {
                this.ConnectToWorkItemStore();
            }

            var targetType = this.workItemStore.Projects[projectName].WorkItemTypes[workItemTypeName];
            var target = new WorkItem(targetType);

            IWorkItem justCreated = new WorkItemWrapper(target, this, this.logger);
            this.loadedWorkItems.Add(justCreated);
            return justCreated;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.tfs?.Dispose();
            }
        }
    }
}
