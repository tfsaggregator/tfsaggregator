namespace Aggregator.Core
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// This interface is visible to scripts.
    /// </summary>
    public interface IWorkItemRepositoryExposed
    {
        IWorkItem GetWorkItem(int workItemId);
        IWorkItem MakeNewWorkItem(string projectName, string workItemTypeName);
    }
    /// <summary>
    /// Decouples Core from TFS Client API <see cref="Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemStore"/>
    /// </summary>
    public interface IWorkItemRepository : IWorkItemRepositoryExposed
    {
        ReadOnlyCollection<IWorkItem> LoadedWorkItems { get; }
    }
}
