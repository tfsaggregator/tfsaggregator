using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core.Mock
{
    using Aggregator.Core;

    internal class WorkItemRepositoryMock : IWorkItemRepository
    {
        private List<IWorkItem> _workItems = new List<IWorkItem>();
        
        public IWorkItem GetWorkItem(int workItemId)
        {
            return _workItems.SingleOrDefault(wi => wi.Id == workItemId);
        }

        internal void SetWorkItems(IEnumerable<IWorkItem> items)
        {
            _workItems = new List<IWorkItem>(items);
        }
    }
}
