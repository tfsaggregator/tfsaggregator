using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Facade
{
    using System.Globalization;

    public class FieldWrapper : IFieldWrapper
    {
        private Field tfsField;
        public FieldWrapper(Field field)
        {
            this.tfsField = field;
        }

        public string Name
        {
            get
            {
                return tfsField.Name;
            }
        }

        public string ReferenceName
        {
            get
            {
                return tfsField.ReferenceName;
            }
        }

        public object Value
        {
            get
            {
                return tfsField.Value;
            }
            set
            {
                if (value != null && tfsField.FieldDefinition.SystemType == typeof(double))
                {
                    // Ugly hack to ensure the double comparison goed safely. TFS internally rounds/truncates the values.
                    CultureInfo c = CultureInfo.InvariantCulture;
                    double original = double.Parse(((double)tfsField.Value).ToString(c), c);
                    double proposed = double.Parse(((double)value).ToString(c), c);

                    // Irnore when the same value is assigned.
                    if (original == proposed)
                    {
                        return;
                    }
                }
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
