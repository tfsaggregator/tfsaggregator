using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core.Facade
{
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
                return link.LinkTypeEnd.ImmutableName;
            }
        }

        public int TargetId
        {
            get
            {
                return link.TargetId;
            }
        }

        public IWorkItem Target
        {
            get { return store.GetWorkItem(this.TargetId); }
        }
    }
}
