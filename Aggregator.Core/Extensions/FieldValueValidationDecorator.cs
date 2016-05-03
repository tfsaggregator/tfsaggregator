using System;
using System.Collections.Generic;

using Aggregator.Core.Context;
using Aggregator.Core.Facade;
using Aggregator.Core.Interfaces;

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
                new ValueAssignmentToReadonlyFieldValueValidator(context),
                new ValueAssignmentToHistoryFieldValidator(context)
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
}
