namespace Aggregator.Core
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Decouples Core from TFS Client API <see cref="Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemStore"/>
    /// </summary>
    public interface IWorkItemRepository
    {
        IWorkItem GetWorkItem(int workItemId);
        ReadOnlyCollection<IWorkItem> LoadedWorkItems { get; }
        IWorkItem MakeNewWorkItem(string projectName, string workItemTypeName);
    }
}
