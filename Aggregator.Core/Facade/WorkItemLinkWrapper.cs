namespace Aggregator.Core.Facade
{
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    class WorkItemLinkWrapper : IWorkItemLink
    {
        ILogEvents logger;
        private WorkItemLink link;
        private IWorkItemRepository store;

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
            get { return this.store.GetWorkItem(this.TargetId); }
        }

        internal WorkItemLink WorkItemLink { get { return link; } }
    }
}
