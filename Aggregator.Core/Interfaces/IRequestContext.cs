namespace Aggregator.Core
{
    using System;

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
