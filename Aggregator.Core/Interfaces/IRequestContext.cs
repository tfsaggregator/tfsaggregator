using System;

#if TFS2015u1
using IVssRequestContext = Microsoft.TeamFoundation.Framework.Server.IVssRequestContext;
#else
using IVssRequestContext = Microsoft.TeamFoundation.Framework.Server.TeamFoundationRequestContext;
#endif

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

        IProjectProperty[] GetProjectProperties(Uri teamProjectUri);

        Microsoft.TeamFoundation.Framework.Client.IdentityDescriptor GetIdentityToImpersonate(Uri projectCollectionUrl);

        INotification Notification { get; }

        IVssRequestContext VssContext { get; }
    }
}
