using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core.Mock
{
    using System.Collections;

    using Aggregator.Core;
    using Aggregator.Core.Facade;
    using System.Collections.Generic;
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
        public bool _SaveCalled { get { return saveCalled > 0; } }
        public int _SaveCount { get { return saveCalled; } }
        public void Save()
        {
            saveCalled++;
        }

        public object this[string name]
        {
            get
            {
                return Fields[name].Value;
            }
            set
            {
                Fields[name].Value = value;
            }
        }

        public void TryOpen()
        {
            
        }

        public string TypeName { get; set; }

        public System.Collections.ArrayList Validate()
        {
            return new ArrayList();
        }

        WorkItemLinkCollectionMock workItemLinks = new WorkItemLinkCollectionMock();
        public IWorkItemLinkCollection WorkItemLinks { get { return workItemLinks; } }

        public IWorkItemExposed Parent
        {
            get
            {
                return WorkItemLazyReference.MakeParentLazyReference(this, store);
            }
        }

        public IEnumerable<IWorkItemExposed> Children
        {
            get
            {
                return WorkItemLazyReference.MakeChildrenLazyReferences(this, store);
            }
        }

        public IWorkItemType Type { get; set; }

        public IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query)
        {
            return WorkItemLazyVisitor
                .MakeRelativesLazyVisitor(this, query, store);
        }

        public void TransitionToState(string state, string comment)
        {
            StateWorkflow.TransitionToState(this, state, comment, store.Logger);
        }
    }
}
