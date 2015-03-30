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
        private FieldCollectionMock fields = new FieldCollectionMock();
        private WorkItemRepositoryMock store;

        public WorkItemMock(WorkItemRepositoryMock store)
        {
            this.store = store;
        }

        public IFieldCollectionWrapper Fields { get
        {
            return fields;
        } }

        public TType GetField<TType>(string fieldName, TType defaultValue)
        {
            throw new NotImplementedException();
        }

        public string History
        {
            get; set; }

        public int Id
        {
            get; set; }

        public bool IsValid()
        {
            return true;
        }

        public void PartialOpen()
        {
            
        }

        bool saveCalled = false;
        public bool _SaveCalled { get { return saveCalled; } }
        public void Save()
        {
            saveCalled = true;
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

        public IEnumerable<IWorkItemExposed> GetRelatives(string workItemType = "*", int levels = 1, string linkType = "*")
        {
            return WorkItemLazyVisitor
                .MakeRelativesLazyVisitor(this, workItemType, levels, linkType, store);
        }

        public void TransitionToState(string state, string comment)
        {
            throw new NotImplementedException();
        }
    }
}
