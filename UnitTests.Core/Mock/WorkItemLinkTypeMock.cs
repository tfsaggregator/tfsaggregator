using System;
using Aggregator.Core.Interfaces;

namespace UnitTests.Core.Mock
{
    internal class WorkItemLinkTypeMock : IWorkItemLinkType
    {
        public string ForwardEndImmutableName
        {
            get; set;
        }

        public string ForwardEndName
        {
            get; set;
        }

        public string ReferenceName
        {
            get; set;
        }

        public string ReverseEndImmutableName
        {
            get; set;
        }

        public string ReverseEndName
        {
            get; set;
        }

        public override string ToString()
        {
            return string.Format("LinkType {0}", this.ReferenceName);
        }
    }
}