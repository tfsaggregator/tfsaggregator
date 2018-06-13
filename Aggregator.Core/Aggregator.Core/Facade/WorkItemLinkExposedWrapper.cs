using System;
using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    internal class WorkItemLinkExposedWrapper : IWorkItemLinkExposed
    {
        private readonly WorkItemLink item;
        private readonly IRuntimeContext context;

        public WorkItemLinkExposedWrapper(WorkItemLink item, IRuntimeContext context)
        {
            this.item = item;
            this.context = context;
        }

        public string LinkTypeEndImmutableName => this.item.LinkTypeEnd.ImmutableName;

        public int TargetId => this.item.TargetId;

        public IWorkItemExposed Target => this.context.WorkItemRepository.GetWorkItem(this.item.TargetId);
    }
}