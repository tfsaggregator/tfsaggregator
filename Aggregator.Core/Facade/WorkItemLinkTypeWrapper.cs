using System;
using Aggregator.Core.Interfaces;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    internal class WorkItemLinkTypeWrapper : IWorkItemLinkType
    {
        private WorkItemLinkType linkType;

        public WorkItemLinkTypeWrapper(WorkItemLinkType linkType)
        {
            this.linkType = linkType;
        }

        public string ForwardEndImmutableName
        {
            get
            {
                return this.linkType.ForwardEnd.ImmutableName;
            }
        }

        public string ForwardEndName
        {
            get
            {
                return this.linkType.ForwardEnd.Name;
            }
        }

        public string ReferenceName
        {
            get
            {
                return this.linkType.ReferenceName;
            }
        }

        public string ReverseEndImmutableName
        {
            get
            {
                return this.linkType.ReverseEnd.ImmutableName;
            }
        }

        public string ReverseEndName
        {
            get
            {
                return this.linkType.ReverseEnd.Name;
            }
        }
    }
}