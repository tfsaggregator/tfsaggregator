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

            return this.MakeNewWorkItem(workItemTypeName, inSameProjectAs[CoreFieldReferenceNames.TeamProject] as string);
        }

        public IEnumerable<string> GetGlobalList(string globalListName)
        {
            string boo = @"<gl:GLOBALLISTS xmlns:gl='http://schemas.microsoft.com/VisualStudio/2005/workitemtracking/globallists'>
  <GLOBALLIST name='Builds - Share_Migration_toolkit_for_Sharepoint'>
    <LISTITEM value='ShareMigrate2/ShareMigrate2_20130318.1' />
  </GLOBALLIST>
  <GLOBALLIST name='Builds - MyBuildTests'>
    <LISTITEM value='DumpEnvironment/DumpEnvironment_20150528.1' />
    <LISTITEM value='DumpEnvironment/DumpEnvironment_20150528.2' />
    <LISTITEM value='DumpEnvironment/DumpEnvironment_20150528.3' />
    <LISTITEM value='DumpEnvironment/DumpEnvironment_20150528.4' />
    <LISTITEM value='DumpEnvironment/DumpEnvironment_20150528.5' />
  </GLOBALLIST>
  <GLOBALLIST name='Aggregator - UserParameters'>
    <LISTITEM value= 'myParameter=30' />
  </GLOBALLIST>
</gl:GLOBALLISTS>";

            var sourceGL = new XmlDocument();
            sourceGL.LoadXml(boo);

            return Aggregator.Core.Facade.WorkItemRepository.ParseGlobalList(sourceGL, globalListName);
        }

        public ReadOnlyCollection<IWorkItem> LoadedWorkItems
        {
            get { return new ReadOnlyCollection<IWorkItem>(this.loadedWorkItems); }
        }

        public ReadOnlyCollection<IWorkItem> CreatedWorkItems
        {
            get { return new ReadOnlyCollection<IWorkItem>(this.createdWorkItems); }
        }

        public ReadOnlyCollection<IWorkItemLinkType> WorkItemLinkTypes
        {
            get
            {
                var result = new List<IWorkItemLinkType>();
                result.Add(new WorkItemLinkTypeMock()
                {
                    ReferenceName = "System.LinkTypes.Hierarchy",
                    ForwardEndName = "Child",
                    ForwardEndImmutableName = "System.LinkTypes.Hierarchy-Forward",
                    ReverseEndName = "Parent",
                    ReverseEndImmutableName = "System.LinkTypes.Hierarchy-Reverse"
                });
                result.Add(new WorkItemLinkTypeMock()
                {
                    ReferenceName = "Microsoft.VSTS.Common.TestedBy",
                    ForwardEndName = "Tested By",
                    ForwardEndImmutableName = "Microsoft.VSTS.Common.TestedBy-Forward",
                    ReverseEndName = "Tests",
                    ReverseEndImmutableName = "Microsoft.VSTS.Common.TestedBy-Reverse"
                });
                return new ReadOnlyCollection<IWorkItemLinkType>(result);
            }
        }
    }
}
