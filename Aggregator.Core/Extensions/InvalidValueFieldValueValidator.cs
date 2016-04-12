using System.Collections;

using Aggregator.Core.Context;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
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

                if (!valid)
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