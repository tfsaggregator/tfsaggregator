using System.Collections.Generic;

namespace Aggregator.Core.Interfaces
{
    /// <summary>
    /// This interface is visible to scripts.
    /// </summary>
    public interface IWorkItemRepositoryExposed
    {
        IWorkItem GetWorkItem(int workItemId);

        IEnumerable<IWorkItem> QueryWorkItems(string wiqlQuery);

        IWorkItem MakeNewWorkItem(string projectName, string workItemTypeName);

        IWorkItem MakeNewWorkItem(IWorkItem inSameProjectAs, string workItemTypeName);

        IEnumerable<string> GetGlobalList(string globalListName);
    }
}