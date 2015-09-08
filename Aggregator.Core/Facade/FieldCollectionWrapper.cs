using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    public class FieldCollectionWrapper : IFieldCollectionWrapper
    {
        private readonly FieldCollection fields;

        public FieldCollectionWrapper(FieldCollection fieldCollection)
        {
            this.fields = fieldCollection;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarQube", "S3237:\"value\" parameters should be used", Justification = "Available for mock testing only")]
        public IFieldWrapper this[string name]
        {
            get
            {
                return new FieldWrapper(this.fields[name]);
            }

            set
            {
                // Do nothing - this is here for unit testing purposes.
                // We don't actually want to add fields in our app code
            }
        }

        public IEnumerator<IFieldWrapper> GetEnumerator()
        {
            return this.fields.Cast<Field>().Select(f => (IFieldWrapper)new FieldWrapper(f)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
