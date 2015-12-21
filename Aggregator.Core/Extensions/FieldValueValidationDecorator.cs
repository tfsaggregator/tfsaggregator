using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using Aggregator.Core.Facade;
using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
    public class FieldValueValidationDecorator : IField
    {
        private readonly FieldWrapper decoratedField;

        private readonly ICollection<BaseFieldValueValidator> validators =
            new BaseFieldValueValidator[]
            {
                new IncorrectDataTypeFieldValidator(),
                new NullAssignmentToRequiredFieldValueValidator(),
                new InvalidValueFieldValueValidator(),
            };

        public FieldValueValidationDecorator(FieldWrapper decoratedField)
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
                bool valid = true;

                foreach (var validator in this.validators)
                {
                    valid &= validator.ValidateFieldValue(this.decoratedField.TfsField, value);
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
    }


    internal abstract class BaseFieldValueValidator
    {
        public abstract bool ValidateFieldValue(Field field, object value);
    }

    internal class NullAssignmentToRequiredFieldValueValidator : BaseFieldValueValidator
    {
        public override bool ValidateFieldValue(Field field, object value)
        {
            if (value == null)
            {
                if (field.IsRequired)
                {
                    // Log warning;
                    return false;
                }
            }

            return true;
        }
    }

    internal class InvalidValueFieldValueValidator : BaseFieldValueValidator
    {
        public override bool ValidateFieldValue(Field field, object value)
        {
            if (value != null && field.IsLimitedToAllowedValues)
            {
                if (field.HasAllowedValuesList && !field.FieldDefinition.IsIdentity)
                {
                    if (!((IList)field.FieldDefinition.AllowedValues).Contains(value))
                    {
                        return false;
                    }
                }
                else if (field.HasAllowedValuesList && field.FieldDefinition.IsIdentity)
                {
                    if (!((IList)field.FieldDefinition.IdentityFieldAllowedValues).Contains(value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    internal class IncorrectDataTypeFieldValidator : BaseFieldValueValidator
    {
        public override bool ValidateFieldValue(Field field, object value)
        {
            if (value != null)
            {
                if (value.GetType() != field.FieldDefinition.SystemType)
                {
                    // Log warning to raise issue.
                    return false;
                }
            }

            return true;
        }
    }
}
