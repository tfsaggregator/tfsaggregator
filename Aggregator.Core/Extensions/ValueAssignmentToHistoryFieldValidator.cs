using System;
using System.Linq;

using Aggregator.Core.Context;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
    internal class ValueAssignmentToHistoryFieldValidator : BaseFieldValueValidator
    {
        public ValueAssignmentToHistoryFieldValidator(IRuntimeContext context)
            : base(context)
        {
        }

        public override bool ValidateFieldValue(Field field, object value)
        {
            if (field.ReferenceName.Equals("System.History", StringComparison.OrdinalIgnoreCase))
            {
                this.Logger.FieldValidationFailedAssignmentToHistory(field.WorkItem.Id);

                return false;
            }

            return true;
        }
    }
}