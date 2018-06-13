using System.Collections.Generic;

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

        IWorkItem MakeNewWorkItem(IWorkItemExposed inSameProjectAs, string workItemTypeName);

        IEnumerable<string> GetGlobalList(string globalListName);

        void AddItemToGlobalList(string globalListName, string item);

        void RemoveItemFromGlobalList(string globalListName, string item);
    }
}