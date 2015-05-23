namespace UnitTests.Core.Mock
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Aggregator.Core;

    internal class WorkItemRepositoryMock : IWorkItemRepository
    {
        private List<IWorkItem> _workItems = new List<IWorkItem>();
        List<IWorkItem> loadedWorkItems = new List<IWorkItem>();

        public ILogEvents Logger { get; set; }
        
        public IWorkItem GetWorkItem(int workItemId)
        {
            IWorkItem justLoaded = _workItems.SingleOrDefault(wi => wi.Id == workItemId);
            loadedWorkItems.Add(justLoaded);
            return justLoaded;
        }

        internal void SetWorkItems(IEnumerable<IWorkItem> items)
        {
            _workItems = new List<IWorkItem>(items);
        }

        public ReadOnlyCollection<IWorkItem> LoadedWorkItems
        {
            get { return new ReadOnlyCollection<IWorkItem>(loadedWorkItems); }
        }
    }
}
