using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
    using System.Collections;

    /// <summary>
    /// Decouples Core from TFS Client API <see cref="Microsoft.TeamFoundation.WorkItemTracking.Client.FieldCollection"/>
    /// </summary>
    public interface IFieldCollectionWrapper : IEnumerable<IFieldWrapper>
    {
        IFieldWrapper this[string name] { get; set; }
    }
}
