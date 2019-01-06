using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Aggregator.Core.Interfaces
{
    /// <summary>
    /// Decouples Core from TFS Client API <see cref="Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemStore"/>
    /// </summary>
    public interface IWorkItemRepository : IWorkItemRepositoryExposed
    {
        ReadOnlyCollection<IWorkItem> LoadedWorkItems { get; }

        ReadOnlyCollection<IWorkItem> CreatedWorkItems { get; }
    }
}
