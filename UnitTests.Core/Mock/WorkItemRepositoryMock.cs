using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Aggregator.Core;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace UnitTests.Core.Mock
{
    internal class WorkItemRepositoryMock : IWorkItemRepository
    {
        private List<IWorkItem> workItems = new List<IWorkItem>();

        private readonly List<IWorkItem> loadedWorkItems = new List<IWorkItem>();

        private int newWorkItemId = -1;

        public ILogEvents Logger { get; set; }

        public IWorkItem GetWorkItem(int workItemId)
        {
            IWorkItem justLoaded = this.workItems.SingleOrDefault(wi => wi.Id == workItemId);
            this.loadedWorkItems.Add(justLoaded);
            return justLoaded;
        }

        internal void SetWorkItems(IEnumerable<IWorkItem> items)
        {
            this.workItems = new List<IWorkItem>(items);

            // reset the flag!
            this.workItems.ForEach(wi => ((WorkItemMock)wi).IsDirty = false);
        }

        public IWorkItem MakeNewWorkItem(string projectName, string workItemTypeName)
        {
            var newWorkItem = new WorkItemMock(this);
            newWorkItem.Id = this.newWorkItemId--;
            newWorkItem.TypeName = workItemTypeName;

            // don't forget to add to collection
            this.loadedWorkItems.Add(newWorkItem);
            return newWorkItem;
        }

        public ReadOnlyCollection<IWorkItem> LoadedWorkItems
        {
            get { return new ReadOnlyCollection<IWorkItem>(this.loadedWorkItems); }
        }
    }
}
