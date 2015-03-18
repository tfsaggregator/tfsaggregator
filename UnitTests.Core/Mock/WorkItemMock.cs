using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core.Mock
{
    using System.Collections;

    using Aggregator.Core;
    using Aggregator.Core.Facade;

    internal class WorkItemMock : IWorkItem
    {
        private FieldCollectionMock fields = new FieldCollectionMock();

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

        public Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemLinkCollection WorkItemLinks { get; set; }

        public IWorkItemExposed Parent { get; set; }
    }
}
