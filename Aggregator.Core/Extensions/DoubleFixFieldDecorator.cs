using System;
using System.Globalization;

using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
    public class DoubleFixFieldDecorator : IFieldExposed
    {
        private readonly IFieldExposed decoratedField;

        public DoubleFixFieldDecorator(IFieldExposed decoratedField, IRuntimeContext context)
        {
            this.decoratedField = decoratedField;
        }

        public string Name
        {
            get
            {
                return this.decoratedField.Name;
            }
        }

        public string ReferenceName
        {
            get
            {
                return this.decoratedField.ReferenceName;
            }
        }

        public object Value
        {
            get
            {
                return this.decoratedField.Value;
            }

            set
            {
                if (
                    value != null
                    && this.Value != null
                    && this.DataType == typeof(double))
                {
                    CultureInfo invariant = CultureInfo.InvariantCulture;
                    decimal current = decimal.Parse(Convert.ToDouble(this.Value, invariant).ToString(invariant), invariant);
                    decimal proposed = decimal.Parse(Convert.ToDouble(value, invariant).ToString(invariant), invariant);

                    // Ignore when the same value is assigned.
                    if (current == proposed)
                    {
                        return;
                    }
                }

                this.decoratedField.Value = value;
            }
        }

        public FieldStatus Status
        {
            get
            {
                return this.decoratedField.Status;
            }
        }

        public object OriginalValue
        {
            get
            {
                return this.decoratedField.OriginalValue;
            }
        }

        public Type DataType
        {
            get
            {
                return this.decoratedField.DataType;
            }
        }

        public Field TfsField
        {
            get
            {
                return this.decoratedField.TfsField;
            }
        }
    }
}
