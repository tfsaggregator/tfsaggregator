using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;

using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

using IdentityDescriptor = Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor;

namespace Aggregator.Core.Facade
{
    /// <summary>
    /// Singleton used to access TFS Data.  This keeps us from connecting each and every time we get an update.
    /// Keeps track of all WorkItems pulled in memory that should be saved later.
    /// </summary>
    public partial class WorkItemRepository : IWorkItemRepository, IDisposable
    {
        private readonly ILogEvents logger;

        private readonly IRuntimeContext context;

        private readonly Dictionary<int, IWorkItem> loadedWorkItems = new Dictionary<int, IWorkItem>();

        private readonly List<IWorkItem> createdWorkItems = new List<IWorkItem>();

        private readonly WorkItemStore workItemStore;

        private readonly TfsTeamProjectCollection tfs;

        public WorkItemRepository(IRuntimeContext context)
        {
            this.logger = context.Logger;
            this.context = context;
            var ci = context.GetConnectionInfo();
            this.logger.Connecting(ci);
            this.tfs = ci.Token.GetCollection(ci.ProjectCollectionUri);
            this.tfs.Authenticate();
            this.workItemStore = this.tfs.GetService<WorkItemStore>();
        }

        public IWorkItem GetWorkItem(int workItemId)
        {
            IWorkItem result;
            if (!this.loadedWorkItems.TryGetValue(workItemId, out result))
            {
                result = new WorkItemWrapper(this.workItemStore.GetWorkItem(workItemId), this.context);
                this.loadedWorkItems.Add(workItemId, result);
            }

            return result;
        }

        public ReadOnlyCollection<IWorkItem> LoadedWorkItems
        {
            get
            {
                return new ReadOnlyCollection<IWorkItem>(this.loadedWorkItems.Values.ToList());
            }
        }

        public ReadOnlyCollection<IWorkItem> CreatedWorkItems
        {
            get
            {
                return new ReadOnlyCollection<IWorkItem>(this.createdWorkItems);
            }
        }

        public IWorkItem MakeNewWorkItem(string projectName, string workItemTypeName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw new ArgumentNullException(nameof(projectName));
            }

            if (string.IsNullOrWhiteSpace(workItemTypeName))
            {
                throw new ArgumentNullException(nameof(workItemTypeName));
            }

            var targetType = this.workItemStore.Projects[projectName].WorkItemTypes[workItemTypeName];
            var target = new WorkItem(targetType);

            IWorkItem justCreated = new WorkItemWrapper(target, this.context);
            this.createdWorkItems.Add(justCreated);
            return justCreated;
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
            // TODO this.logger.ReadingGlobalList(this.workItemStore.TeamProjectCollection.Name, globalListName);

            // get Global Lists from TFS collection
            var sourceGL = this.workItemStore.ExportGlobalLists();
            return ParseGlobalList(sourceGL, globalListName);
        }

        // HACK public to allow Unit Testing
        public static IEnumerable<string> ParseGlobalList(XmlDocument sourceGL, string globalListName)
        {
            var ns = new XmlNamespaceManager(sourceGL.NameTable);
            ns.AddNamespace("gl", "http://schemas.microsoft.com/VisualStudio/2005/workitemtracking/globallists");

            string xpath = string.Format("/gl:GLOBALLISTS/GLOBALLIST[@name='{0}']/LISTITEM/@value", globalListName);
            var nodes = sourceGL.SelectNodes(xpath, ns);

            foreach (XmlAttribute node in nodes.Cast<XmlAttribute>())
            {
                yield return node.Value;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.tfs?.Dispose();
            }
        }
    }
}
