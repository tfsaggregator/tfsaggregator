using System.Collections.Generic;

namespace Aggregator.Core.Interfaces
{
    /// <summary>
    /// Decouples Core from TFS Client API <see cref="Microsoft.TeamFoundation.WorkItemTracking.Client.FieldCollection"/>
    /// </summary>
    public interface IFieldCollectionWrapper : IEnumerable<IFieldWrapper>
    {
        IFieldWrapper this[string name] { get; set; }
    }
}
