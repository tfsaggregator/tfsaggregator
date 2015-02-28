using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
    public interface IFieldWrapper
    {
        object Value { get; set; }
        FieldStatus Status { get; }
        object OriginalValue { get; }
    }
}
