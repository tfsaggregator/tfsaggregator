using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSAggregator.TfsFacade
{
    public interface IFieldWrapper
    {
        object Value { get; set; }
        TFS.FieldStatus Status { get; }
        object OriginalValue { get; }
    }

    public class FieldWrapper : IFieldWrapper
    {
        private TFS.Field tfsField;
        public FieldWrapper(TFS.Field field)
        {
            this.tfsField = field;
        }

        public object Value
        {
            get
            {
                return tfsField.Value;
            }
            set
            {
                tfsField.Value = value;
            }
        }

        public TFS.FieldStatus Status
        {
            get { return tfsField.Status; }
        }

        public object OriginalValue
        {
            get { return tfsField.OriginalValue; }
        }
    }
}
