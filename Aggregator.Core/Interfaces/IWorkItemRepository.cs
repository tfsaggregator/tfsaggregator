using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Aggregator.Core.Interfaces
{
    /// <summary>
    /// This interface is visible to scripts.
    /// </summary>
    public interface IWorkItemRepositoryExposed
    {
        IWorkItem GetWorkItem(int workItemId);

        IWorkItem MakeNewWorkItem(string projectName, string workItemTypeName);

        IWorkItem MakeNewWorkItem(IWorkItem inSameProjectAs, string workItemTypeName);
    }

    /// <summary>
    /// Decouples Core from TFS Client API <see cref="Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemStore"/>
    /// </summary>
    public interface IWorkItemRepository : IWorkItemRepositoryExposed
    {
        ReadOnlyCollection<IWorkItem> LoadedWorkItems { get; }

        ReadOnlyCollection<IWorkItem> CreatedWorkItems { get; }
    }
}
