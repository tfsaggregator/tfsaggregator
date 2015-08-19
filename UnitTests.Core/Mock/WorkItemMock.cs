using System;
using System.Collections;
using System.Collections.Generic;

using Aggregator.Core;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Navigation;

namespace UnitTests.Core.Mock
{
    internal class WorkItemMock : WorkItemImplementationBase, IWorkItem
    {
        private readonly FieldCollectionMock fields;

        public WorkItemMock(WorkItemRepositoryMock store)
            : base(store, store.Logger)
        {
            this.fields = new FieldCollectionMock(this);
            this.IsDirty = false;
        }

        public IFieldCollectionWrapper Fields
        {
            get
            {
                return this.fields;
            }
        }

        public string History { get; set; }

        public int Id { get; set; }

        public bool IsValid()
        {
            return true;
        }

        public bool IsDirty { get; set; }

        public void PartialOpen()
        {
            // No functionality needed in mock.
        }

        private int saveCalled = 0;

        public bool _SaveCalled
        {
            get
            {
                return this.saveCalled > 0;
            }
        }

        public int _SaveCount
        {
            get
            {
                return this.saveCalled;
            }
        }

        public void Save()
        {
            this.saveCalled++;
        }

        public object this[string name]
        {
            get
            {
                return this.Fields[name].Value;
            }

            set
            {
                this.Fields[name].Value = value;
            }
        }

        public void TryOpen()
        {
            // No functionality needed in mock.
        }

        public string TypeName { get; set; }

        public ArrayList Validate()
        {
            return new ArrayList();
        }

        private readonly WorkItemLinkCollectionMock workItemLinks = new WorkItemLinkCollectionMock();

        public override IWorkItemLinkCollection WorkItemLinks
        {
            get { return this.workItemLinks; }
        }

        public IWorkItemType Type { get; set; }

        public IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query)
        {
            return WorkItemLazyVisitor
                .MakeRelativesLazyVisitor(this, query);
        }

        public void TransitionToState(string state, string comment)
        {
            // HACK
            StateWorkFlow.TransitionToState(this, state, comment, this.logger);
        }

        public void AddWorkItemLink(IWorkItemExposed destination, string linkTypeName)
        {
            // HACK: should use the code in wrapper...
            var relationship = new WorkItemLinkMock(linkTypeName, destination.Id, this.store);

            // check it does not exist already
            if (!this.workItemLinks.Contains(relationship))
            {
                this.workItemLinks.Add(relationship);
                this.IsDirty = true;
            }
        }

        public void AddHyperlink(string destination, string comment = "")
        {
            throw new NotImplementedException();
        }
    }
}
