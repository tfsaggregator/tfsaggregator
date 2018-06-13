using Aggregator.Core;
using Aggregator.Core.Interfaces;

namespace UnitTests.Core.Mock
{
    internal class WorkItemLinkMock : IWorkItemLink
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
            get { return this.store.GetWorkItem(this.id); }
        }

        public bool IsNew
        {
            get { return false; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((WorkItemLinkMock)obj);
        }

        protected bool Equals(WorkItemLinkMock other)
        {
            return string.Equals(this.relationship, other.relationship) && this.id == other.id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.relationship?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.id;
                return hashCode;
            }
        }
    }
}
