using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    using Aggregator.Core.Interfaces;

    using Microsoft.TeamFoundation.Server.Core;

    /// <summary>
    /// Decouples Core from TFS Server API
    /// </summary>
    public interface IRequestContext
    {
        string CollectionName { get; }
        string GetProjectName(Uri teamProjectUri);

        IProjectPropertyWrapper[] GetProjectProperties(Uri teamProjectUri);

        ProcessTemplateVersion GetCurrentProjectProcessVersion(Uri projectUri);


        ProcessTemplateVersion GetCreationProjectProcessVersion(Uri projectUri);
    }
}
