using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core.Mock
{
    using Aggregator.Core;
    using System.Collections.ObjectModel;

    internal class WorkItemRepositoryMock : IWorkItemRepository
    {
        private List<IWorkItem> _workItems = new List<IWorkItem>();
        List<IWorkItem> loadedWorkItems = new List<IWorkItem>();
        
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
