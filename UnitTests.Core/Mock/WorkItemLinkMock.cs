using Aggregator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTests.Core.Mock
{
    class WorkItemLinkMock : IWorkItemLink
    {
        private string relationship;
        private int id;
        private WorkItemRepositoryMock store;

        public WorkItemLinkMock(string relationship, int id, WorkItemRepositoryMock store)
        {
            this.relationship = relationship;
            this.id = id;
            this.store = store;
        }

        public int TargetId
        {
            get { return this.id; }
        }

        public string LinkTypeEndImmutableName
        {
            get { return this.relationship; }
        }

        public IWorkItem Target
        {
            //TODO
            get { return store.GetWorkItem(this.id); }
        }
    }
}
