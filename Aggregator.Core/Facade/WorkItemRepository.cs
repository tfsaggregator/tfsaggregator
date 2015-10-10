using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

        private readonly Dictionary<int, IWorkItem> loadedWorkItems = new Dictionary<int, IWorkItem>();

        private readonly List<IWorkItem> createdWorkItems = new List<IWorkItem>();

        private readonly WorkItemStore workItemStore;

        private readonly TfsTeamProjectCollection tfs;

        public WorkItemRepository(Uri tfsCollectionUri, IdentityDescriptor toImpersonate, ILogEvents logger)
        {
            this.logger = logger;
            this.tfs = new TfsTeamProjectCollection(tfsCollectionUri, toImpersonate);
            this.workItemStore = (WorkItemStore)this.tfs.GetService(typeof(WorkItemStore));
        }

        public IWorkItem GetWorkItem(int workItemId)
        {
            IWorkItem result;
            if (!this.loadedWorkItems.TryGetValue(workItemId, out result))
            {
                result = new WorkItemWrapper(this.workItemStore.GetWorkItem(workItemId), this, this.logger);
                this.loadedWorkItems.Add(workItemId, result);
            }

            return result;
        }

        public ReadOnlyCollection<IWorkItem> LoadedWorkItems
        {
            get
            {
                return new ReadOnlyCollection<IWorkItem>(this.loadedWorkItems.Values.ToList());
            }
        }

        public ReadOnlyCollection<IWorkItem> CreatedWorkItems
        {
            get
            {
                return new ReadOnlyCollection<IWorkItem>(this.createdWorkItems);
            }
        }

        public IWorkItem MakeNewWorkItem(string projectName, string workItemTypeName)
        {
            var targetType = this.workItemStore.Projects[projectName].WorkItemTypes[workItemTypeName];
            var target = new WorkItem(targetType);

            IWorkItem justCreated = new WorkItemWrapper(target, this, this.logger);
            this.createdWorkItems.Add(justCreated);
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
