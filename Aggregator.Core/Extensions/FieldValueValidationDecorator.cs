using System;
using System.Collections;
using System.Collections.Generic;

using Aggregator.Core.Context;
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

        public FieldValueValidationDecorator(IFieldExposed decoratedField, IRuntimeContext context)
        {
            this.decoratedField = decoratedField;

            this.validators = new BaseFieldValueValidator[]
            {
                new IncorrectDataTypeFieldValidator(context),
                new NullAssignmentToRequiredFieldValueValidator(context),
                new InvalidValueFieldValueValidator(context),
                new ValueAssignmentToReadonlyFieldValueValidator(context)
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

        protected IRuntimeContext Context { get; private set; }

        internal BaseFieldValueValidator(IRuntimeContext context)
        {
            this.Logger = context.Logger;
            this.Context = context;
        }

        public abstract bool ValidateFieldValue(Field field, object value);
    }

    internal class NullAssignmentToRequiredFieldValueValidator : BaseFieldValueValidator
    {
        internal NullAssignmentToRequiredFieldValueValidator(IRuntimeContext context)
            : base(context)
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
        internal ValueAssignmentToReadonlyFieldValueValidator(IRuntimeContext context)
            : base(context)
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
        internal InvalidValueFieldValueValidator(IRuntimeContext context)
            : base(context)
        {
        }

        public override bool ValidateFieldValue(Field field, object value)
        {
            if (value != null && field.IsLimitedToAllowedValues)
            {
                bool valid = true;
                bool hasAllowedvalues = field.HasAllowedValuesList;

#if TFS2015 || TFS2015u1
                bool isIdentity = field.FieldDefinition.IsIdentity;
#else
                bool isIdentity = false;
#endif

                if (hasAllowedvalues && !isIdentity)
                {
                    valid &= ((IList)field.FieldDefinition.AllowedValues).Contains(value);
                }
#if TFS2015 || TFS2015u1
                else if (hasAllowedvalues && isIdentity)
                {
                    valid &= ((IList)field.FieldDefinition.IdentityFieldAllowedValues).Contains(value);
                }
#endif

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
        internal IncorrectDataTypeFieldValidator(IRuntimeContext context)
            : base(context)
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
