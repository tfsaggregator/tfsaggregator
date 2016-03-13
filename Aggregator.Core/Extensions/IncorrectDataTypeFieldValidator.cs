using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
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