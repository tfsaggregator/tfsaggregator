using Aggregator.Core.Context;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
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
}