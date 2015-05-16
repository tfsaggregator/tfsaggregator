namespace UnitTests.Core.Mock
{
    using System.Collections;
    using System.Collections.Generic;

    using Aggregator.Core;

    internal class FieldCollectionMock : IFieldCollectionWrapper
    {
        Dictionary<string, IFieldWrapper> fields = new Dictionary<string, IFieldWrapper>();
        private WorkItemMock workItemMock;

        public FieldCollectionMock(WorkItemMock workItemMock)
        {
            this.workItemMock = workItemMock;
        } 

        public IFieldWrapper this[string name]
        {
            get
            {
                if (!this.fields.ContainsKey(name))
                {
                    this.fields.Add(name, new FieldMock(this.workItemMock, name));
                }
                return fields[name];
            }
            set
            {
                fields[name] = value;
            }
        }

        public IEnumerator<IFieldWrapper> GetEnumerator()
        {
            return this.fields.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}