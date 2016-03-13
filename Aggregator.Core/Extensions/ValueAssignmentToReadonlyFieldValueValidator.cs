using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
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
}