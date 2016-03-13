using System;
using System.Collections;
using System.Collections.Generic;

using Aggregator.Core.Facade;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
    public class FieldValueValidationDecorator : IFieldExposed
    {
        private readonly IFieldExposed decoratedField;

        private readonly ICollection<BaseFieldValueValidator> validators;

        public FieldValueValidationDecorator(IFieldExposed decoratedField, ILogEvents logger)
        {
            this.decoratedField = decoratedField;

            this.validators = new BaseFieldValueValidator[]
            {
                new IncorrectDataTypeFieldValidator(logger),
                new NullAssignmentToRequiredFieldValueValidator(logger),
                new InvalidValueFieldValueValidator(logger),
                new ValueAssignmentToReadonlyFieldValueValidator(logger)
            };
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

        public Field TfsField
        {
            get
            {
                return this.decoratedField.TfsField;
            }
        }
    }


    internal abstract class BaseFieldValueValidator
    {
        protected ILogEvents Logger { get; private set; }

        internal BaseFieldValueValidator(ILogEvents logger)
        {
            this.Logger = logger;
        }

        public abstract bool ValidateFieldValue(Field field, object value);
    }

    internal class NullAssignmentToRequiredFieldValueValidator : BaseFieldValueValidator
    {
        internal NullAssignmentToRequiredFieldValueValidator(ILogEvents logger)
            : base(logger)
        {
        }

        public override bool ValidateFieldValue(Field field, object value)
        {
            if (value == null && field.IsRequired)
            {
                this.Logger.FieldValidationFailedFieldRequired(field.WorkItem.Id, field.ReferenceName);

                return false;
            }

            return true;
        }
    }

    internal class ValueAssignmentToReadonlyFieldValueValidator : BaseFieldValueValidator
    {
        internal ValueAssignmentToReadonlyFieldValueValidator(ILogEvents logger)
            : base(logger)
        {
        }

        public override bool ValidateFieldValue(Field field, object value)
        {
            if (value != null && !field.IsEditable)
            {
                this.Logger.FieldValidationFailedFieldNotEditable(field.WorkItem.Id, field.ReferenceName, value);

                return false;
            }

            return true;
        }
    }

    internal class InvalidValueFieldValueValidator : BaseFieldValueValidator
    {
        internal InvalidValueFieldValueValidator(ILogEvents logger)
            : base(logger)
        {
        }

        public override bool ValidateFieldValue(Field field, object value)
        {
            if (value != null && field.IsLimitedToAllowedValues)
            {
                bool valid = true;
                bool hasAllowedvalues = field.HasAllowedValuesList;

                if (hasAllowedvalues)
                {
                    valid &= ((IList)field.FieldDefinition.AllowedValues).Contains(value);
                }

                if (valid)
                {
                    this.Logger.FieldValidationFailedValueNotAllowed(
                            field.WorkItem.Id,
                            field.ReferenceName,
                            value);

                    return false;
                }
            }

            return true;
        }
    }

    internal class IncorrectDataTypeFieldValidator : BaseFieldValueValidator
    {
        internal IncorrectDataTypeFieldValidator(ILogEvents logger)
            : base(logger)
        {
        }

        public override bool ValidateFieldValue(Field field, object value)
        {
            if (value != null && value.GetType() != field.FieldDefinition.SystemType)
            {
                this.Logger.FieldValidationFailedInvalidDataType(
                    field.WorkItem.Id,
                    field.ReferenceName,
                    field.FieldDefinition.SystemType,
                    value.GetType(),
                    value);

                return false;
            }

            return true;
        }
    }
}
