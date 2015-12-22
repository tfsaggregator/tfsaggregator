using System.Collections;
using System.Collections.Generic;

using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;

namespace UnitTests.Core.Mock
{
    internal class FieldCollectionMock : IFieldCollection
    {
        private readonly Dictionary<string, IField> fields = new Dictionary<string, IField>();

        private readonly WorkItemMock workItemMock;

        public FieldCollectionMock(WorkItemMock workItemMock)
        {
            this.workItemMock = workItemMock;
        }

        public IField this[string name]
        {
            get
            {
                if (!this.fields.ContainsKey(name))
                {
                    IFieldExposed field = new FieldMock(this.workItemMock, name);
                    this.fields.Add(name, new DoubleFixFieldDecorator(field, null));
                }

                return this.fields[name];
            }

            set
            {
                this.fields[name] = value;
            }
        }

        public IEnumerator<IField> GetEnumerator()
        {
            return this.fields.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}