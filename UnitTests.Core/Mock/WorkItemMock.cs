namespace UnitTests.Core.Mock
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Aggregator.Core;
    using Aggregator.Core.Navigation;

    internal class WorkItemMock : IWorkItem
    {
        private FieldCollectionMock fields;
        private WorkItemRepositoryMock store;

        public WorkItemMock(WorkItemRepositoryMock store)
        {
            this.store = store;
            this.fields = new FieldCollectionMock(this);
        }

        public IFieldCollectionWrapper Fields { get
        {
            return fields;
        } }

        public TType GetField<TType>(string fieldName, TType defaultValue)
        {
            throw new NotImplementedException();
        }

        public string History{ get; set; }

        public int Id{ get; set; }

        public bool IsValid()
        {
            return true;
        }

        public bool IsDirty { get; set; }

        public void PartialOpen()
        {
            
        }

        int saveCalled = 0;
        public bool _SaveCalled { get { return this.saveCalled > 0; } }
        public int _SaveCount { get { return this.saveCalled; } }
        public void Save()
        {
            this.saveCalled++;
        }

        public object this[string name]
        {
            get
            {
                return Fields[name].Value;
            }
            set
            {
                this.Fields[name].Value = value;
            }
        }

        public void TryOpen()
        {
            
        }

        public string TypeName { get; set; }

        public bool HasParent()
        {
            return this.HasRelation("Parent");
        }

        public bool HasChildren()
        {
            return this.HasRelation("Child");
        }

        public bool HasRelation(string relation)
        {
            if (string.IsNullOrWhiteSpace(relation))
            {
                throw new ArgumentNullException("relation");
            }

            foreach (var link in this.WorkItemLinks)
            {
                if (string.Equals(relation, link.LinkTypeEndImmutableName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public ArrayList Validate()
        {
            return new ArrayList();
        }

        WorkItemLinkCollectionMock workItemLinks = new WorkItemLinkCollectionMock();

        public IWorkItemLinkCollection WorkItemLinks
        {
            get { return this.workItemLinks; }
        }

        public IWorkItemExposed Parent
        {
            get
            {
                return WorkItemLazyReference.MakeParentLazyReference(this, this.store);
            }
        }

        public IEnumerable<IWorkItemExposed> Children
        {
            get
            {
                return WorkItemLazyReference.MakeChildrenLazyReferences(this, this.store);
            }
        }

        public IWorkItemType Type { get; set; }

        public IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query)
        {
            return WorkItemLazyVisitor
                .MakeRelativesLazyVisitor(this, query, this.store);
        }

        public void TransitionToState(string state, string comment)
        {
            StateWorkflow.TransitionToState(this, state, comment, this.store.Logger);
        }

        public void AddWorkItemLink(IWorkItemExposed destination, string linkTypeName)
        {
            throw new NotImplementedException();
        }
    }
}
