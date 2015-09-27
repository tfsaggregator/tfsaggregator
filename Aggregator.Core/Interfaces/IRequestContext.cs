using System;

namespace Aggregator.Core.Interfaces
{
    /// <summary>
    /// Decouples Core from TFS Server API
    /// </summary>
    public interface IRequestContext
    {
        Uri GetProjectCollectionUri();

        string CollectionName { get; }

        string GetProjectName(Uri teamProjectUri);

        IProjectPropertyWrapper[] GetProjectProperties(Uri teamProjectUri);

        IProcessTemplateVersionWrapper GetCurrentProjectProcessVersion(Uri projectUri);

        IProcessTemplateVersionWrapper GetCreationProjectProcessVersion(Uri projectUri);

        Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor GetIdentityToImpersonate();

        INotification Notification { get; }
    }
}
