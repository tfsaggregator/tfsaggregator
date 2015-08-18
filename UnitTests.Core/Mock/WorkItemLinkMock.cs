using Aggregator.Core.Interfaces;

namespace UnitTests.Core.Mock
{
    using Aggregator.Core;

    class WorkItemLinkMock : IWorkItemLink
    {
        private readonly string relationship;
        private readonly int id;
        private readonly IWorkItemRepository store;

        public WorkItemLinkMock(string relationship, int id, IWorkItemRepository store)
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
            get { return store.GetWorkItem(this.id); }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WorkItemLinkMock))
                return false;

            var rhs = obj as WorkItemLinkMock;

            return this.LinkTypeEndImmutableName == rhs.LinkTypeEndImmutableName
                && this.TargetId == rhs.TargetId;
        }
    }
}
