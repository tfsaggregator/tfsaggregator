using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;

using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace UnitTests.Core.Mock
{
    internal class WorkItemRepositoryMock : IWorkItemRepository
    {
        private List<IWorkItem> workItems = new List<IWorkItem>();

        private readonly List<IWorkItem> loadedWorkItems = new List<IWorkItem>();

        private readonly List<IWorkItem> createdWorkItems = new List<IWorkItem>();

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
            var newWorkItem = new WorkItemMock(this, new RuntimeContextMock())
            {
                Id = 0, TypeName = workItemTypeName
            };

            // don't forget to add to collection
            this.createdWorkItems.Add(newWorkItem);
            return newWorkItem;
        }

        public IWorkItem MakeNewWorkItem(IWorkItem inSameProjectAs, string workItemTypeName)
        {
            if (inSameProjectAs == null)
            {
                throw new ArgumentNullException(nameof(inSameProjectAs));
            }

            return this.MakeNewWorkItem(inSameProjectAs[CoreFieldReferenceNames.TeamProject] as string, workItemTypeName);
        }

        public IWorkItem MakeNewWorkItem(IWorkItemExposed inSameProjectAs, string workItemTypeName)
        {
            return this.MakeNewWorkItem((IWorkItem)inSameProjectAs, workItemTypeName);
        }

        XmlDocument globalListsDoc = null;

        public WorkItemRepositoryMock()
        {
        }

        public WorkItemRepositoryMock(string globalLists)
        {
            this.globalListsDoc = new XmlDocument();
            this.globalListsDoc.LoadXml(globalLists);
        }


        public IEnumerable<string> GetGlobalList(string globalListName)
        {
            return Aggregator.Core.Facade.WorkItemRepository.ParseGlobalList(this.globalListsDoc, globalListName);
        }

        public void AddItemToGlobalList(string globalListName, string item)
        {
            Aggregator.Core.Facade.WorkItemRepository.EditGlobalList(this.globalListsDoc, globalListName, item, Aggregator.Core.Facade.WorkItemRepository.EditAction.Add);
        }

        public void RemoveItemFromGlobalList(string globalListName, string item)
        {
            Aggregator.Core.Facade.WorkItemRepository.EditGlobalList(this.globalListsDoc, globalListName, item, Aggregator.Core.Facade.WorkItemRepository.EditAction.Remove);
        }

        public ReadOnlyCollection<IWorkItem> LoadedWorkItems
        {
            get { return new ReadOnlyCollection<IWorkItem>(this.loadedWorkItems); }
        }

        public ReadOnlyCollection<IWorkItem> CreatedWorkItems
        {
            get { return new ReadOnlyCollection<IWorkItem>(this.createdWorkItems); }
        }
    }
}
