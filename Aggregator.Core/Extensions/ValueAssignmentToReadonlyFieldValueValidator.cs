using Aggregator.Core.Context;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
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
}