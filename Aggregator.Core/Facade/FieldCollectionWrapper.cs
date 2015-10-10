using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    public class FieldCollectionWrapper : IFieldCollection
    {
        private readonly Microsoft.TeamFoundation.WorkItemTracking.Client.FieldCollection fields;

        public FieldCollectionWrapper(Microsoft.TeamFoundation.WorkItemTracking.Client.FieldCollection fieldCollection)
        {
            this.fields = fieldCollection;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarQube", "S3237:\"value\" parameters should be used", Justification = "Available for mock testing only")]
        public IField this[string name]
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

        public IEnumerator<IField> GetEnumerator()
        {
            return this.fields.Cast<Field>().Select(f => (IField)new FieldWrapper(f)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
