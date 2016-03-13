using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
    internal abstract class BaseFieldValueValidator
    {
        protected ILogEvents Logger { get; private set; }

        internal BaseFieldValueValidator(ILogEvents logger)
        {
            this.Logger = logger;
        }

        public abstract bool ValidateFieldValue(Field field, object value);
    }
}