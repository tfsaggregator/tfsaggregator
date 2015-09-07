using System.Globalization;

using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    public class FieldWrapper : IFieldWrapper
    {
        private readonly Field tfsField;

        public FieldWrapper(Field field)
        {
            this.tfsField = field;
        }

        public string Name
        {
            get
            {
                return this.tfsField.Name;
            }
        }

        public string ReferenceName
        {
            get
            {
                return this.tfsField.ReferenceName;
            }
        }

        public object Value
        {
            get
            {
                return this.tfsField.Value;
            }

            set
            {
                if (
                    value != null
                    && this.tfsField.Value != null
                    && this.tfsField.FieldDefinition.SystemType == typeof(double))
                {
                    // Ugly hack to ensure the double comparison goes safely. TFS internally rounds/truncates the values.
                    CultureInfo invariant = CultureInfo.InvariantCulture;
                    decimal original = decimal.Parse(((double)this.tfsField.Value).ToString(invariant), invariant);
                    decimal proposed = decimal.Parse(((double)value).ToString(invariant), invariant);

                    // Ignore when the same value is assigned.
                    if (original == proposed)
                    {
                        return;
                    }
                }

                this.tfsField.Value = value;
            }
        }

        public FieldStatus Status
        {
            get { return this.tfsField.Status; }
        }

        public object OriginalValue
        {
            get { return this.tfsField.OriginalValue; }
        }
    }
}
