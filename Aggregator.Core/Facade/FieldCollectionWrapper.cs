using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Facade
{
    public class FieldCollectionWrapper : IFieldCollectionWrapper
    {
        private FieldCollection fields;
        public FieldCollectionWrapper(FieldCollection fieldCollection)
        {
            fields = fieldCollection;
        }

        public IFieldWrapper this[string name]
        {
            get
            {
                return new FieldWrapper(fields[name]);
            }
            set
            {
                //Do nothing - this is here for unit testing purposes.
                //We don't actually want to add fields in our app code
            }
        }
    }
}
