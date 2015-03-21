using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    /// <summary>
    /// Decouples Core from TFS Server API
    /// </summary>
    public interface INotification
    {
        string ProjectUri { get; }
        int WorkItemId { get; }
    }
}
