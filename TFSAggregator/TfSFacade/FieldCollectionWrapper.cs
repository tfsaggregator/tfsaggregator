using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSAggregator.TfsFacade
{
    public interface IFieldCollectionWrapper
    {
        IFieldWrapper this[string name] { get; set; }
    }

    public class FieldCollectionWrapper : IFieldCollectionWrapper
    {
        private TFS.FieldCollection fields;
        public FieldCollectionWrapper(TFS.FieldCollection fieldCollection)
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
