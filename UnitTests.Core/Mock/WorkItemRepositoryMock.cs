namespace UnitTests.Core.Mock
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Aggregator.Core;

    internal class WorkItemRepositoryMock : IWorkItemRepository
    {
        private List<IWorkItem> _workItems = new List<IWorkItem>();
        List<IWorkItem> loadedWorkItems = new List<IWorkItem>();

        public ILogEvents Logger { get; set; }

        private int newWorkItemId = -1;
        
        public IWorkItem GetWorkItem(int workItemId)
        {
            IWorkItem justLoaded = _workItems.SingleOrDefault(wi => wi.Id == workItemId);
            loadedWorkItems.Add(justLoaded);
            return justLoaded;
        }

        internal void SetWorkItems(IEnumerable<IWorkItem> items)
        {
            _workItems = new List<IWorkItem>(items);
            // reset the flag!
            _workItems.ForEach(wi => (wi as WorkItemMock).IsDirty = false);
        }

        public IWorkItem MakeNewWorkItem(string projectName, string workItemTypeName)
        {
            var newWorkItem = new WorkItemMock(this);
            newWorkItem.Id = newWorkItemId--;
            newWorkItem.TypeName = workItemTypeName;
            // don't forget to add to collection
            loadedWorkItems.Add(newWorkItem);
            return newWorkItem;
        }

        public ReadOnlyCollection<IWorkItem> LoadedWorkItems
        {
            get { return new ReadOnlyCollection<IWorkItem>(loadedWorkItems); }
        }
    }
}
