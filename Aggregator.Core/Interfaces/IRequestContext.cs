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

        IProcessTemplateVersionWrapper GetCurrentProjectProcessVersion(Uri projectUri);


        IProcessTemplateVersionWrapper GetCreationProjectProcessVersion(Uri projectUri);
    }
}
