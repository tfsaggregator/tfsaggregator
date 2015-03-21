using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
    /// <summary>
    /// Decouples Core from TFS Client API <see cref="Microsoft.TeamFoundation.WorkItemTracking.Client.FieldCollection"/>
    /// </summary>
    public interface IFieldCollectionWrapper
    {
        IFieldWrapper this[string name] { get; set; }
    }
}
