using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    internal class WorkItemLinkWrapper : IWorkItemLink
    {
        private readonly ILogEvents logger;
        private readonly WorkItemLink link;
        private readonly IWorkItemRepository store;

        public WorkItemLinkWrapper(WorkItemLink link, IWorkItemRepository store, ILogEvents logger)
        {
            this.logger = logger;
            this.link = link;
            this.store = store;
        }

        public string LinkTypeEndImmutableName
        {
            get
            {
                return this.link.LinkTypeEnd.ImmutableName;
            }
        }

        public int TargetId
        {
            get
            {
                return this.link.TargetId;
            }
        }

        public IWorkItem Target
        {
            get
            {
                return this.store.GetWorkItem(this.TargetId);
            }
        }

        internal WorkItemLink WorkItemLink
        {
            get
            {
                return this.link;
            }
        }
    }
}
