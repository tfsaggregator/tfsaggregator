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
    public interface IRequestContext
    {
        string CollectionName { get; }
        string GetProjectName(string teamProjectUri);
    }
}
