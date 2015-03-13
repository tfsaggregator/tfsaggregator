namespace UnitTests.Core.Mock
{
    using System;
    using System.Collections.Generic;

    using Aggregator.Core;

    internal class FieldCollectionMock : IFieldCollectionWrapper
    {
        Dictionary<string, IFieldWrapper> fields = new Dictionary<string, IFieldWrapper>(); 

        public IFieldWrapper this[string name]
        {
            get
            {
                if (!this.fields.ContainsKey(name))
                {
                    this.fields.Add(name, new FieldMock(name));
                }
                return fields[name];
            }
            set
            {
                fields[name] = value;
            }
        }
    }
}