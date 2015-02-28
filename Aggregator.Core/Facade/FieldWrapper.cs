using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Facade
{
    public class FieldWrapper : IFieldWrapper
    {
        private Field tfsField;
        public FieldWrapper(Field field)
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

        public FieldStatus Status
        {
            get { return tfsField.Status; }
        }

        public object OriginalValue
        {
            get { return tfsField.OriginalValue; }
        }
    }
}
