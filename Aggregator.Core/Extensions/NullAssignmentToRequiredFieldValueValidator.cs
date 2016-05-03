using Aggregator.Core.Context;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
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
}