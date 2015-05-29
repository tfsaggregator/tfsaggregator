namespace UnitTests.Core.Mock
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Aggregator.Core;
    using Aggregator.Core.Navigation;

    internal class WorkItemMock : WorkItemImplementationBase, IWorkItem
    {
        private FieldCollectionMock fields;

        public WorkItemMock(WorkItemRepositoryMock store)
            : base(store, store.Logger)
        {
            this.fields = new FieldCollectionMock(this);
        }

        public IFieldCollectionWrapper Fields { get
        {
            return fields;
        } }

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

        public ArrayList Validate()
        {
            return new ArrayList();
        }

        WorkItemLinkCollectionMock workItemLinks = new WorkItemLinkCollectionMock();

        public override IWorkItemLinkCollection WorkItemLinks
        {
            get { return this.workItemLinks; }
        }

        public IWorkItemType Type { get; set; }

        public IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query)
        {
            return WorkItemLazyVisitor
                .MakeRelativesLazyVisitor(this, query, this.store);
        }

        public void TransitionToState(string state, string comment)
        {
            //HACK
            StateWorkflow.TransitionToState(this, state, comment, this.logger);
        }

        public void AddWorkItemLink(IWorkItemExposed destination, string linkTypeName)
        {
            workItemLinks.Add(new WorkItemLinkMock(linkTypeName, destination.Id, store));
        }

        public void AddHyperlink(string destination, string comment = "")
        {
            throw new NotImplementedException();
        }
    }
}
