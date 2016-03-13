using System.Collections;

using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
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
}