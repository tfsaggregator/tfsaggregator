using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Facade
{
    using System.Globalization;

    using Microsoft.TeamFoundation.WorkItemTracking.Client;

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
                if (value != null && this.tfsField.FieldDefinition.SystemType == typeof(double))
                {
                    // Ugly hack to ensure the double comparison goes safely. TFS internally rounds/truncates the values.
                    CultureInfo c = CultureInfo.InvariantCulture;
                    double original = double.Parse(((double)this.tfsField.Value).ToString(c), c);
                    double proposed = double.Parse(((double)value).ToString(c), c);

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
