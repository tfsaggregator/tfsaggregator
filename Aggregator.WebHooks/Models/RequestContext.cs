using System;
using System.Linq;

using Aggregator.Core;
using Aggregator.Core.Facade;
using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Server.Core;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Models
{
    public class RequestContext : IRequestContext
    {
        private readonly string teamProjectCollectionUrl;

        private readonly string teamProjectName;

        public RequestContext(string teamProjectCollectionUrl, string teamProjectName)
        {
            this.teamProjectCollectionUrl = teamProjectCollectionUrl;
            this.teamProjectName = teamProjectName;
        }

        public Uri GetProjectCollectionUri()
        {
            return new Uri(this.teamProjectCollectionUrl);
        }

        public string CollectionName
        {
            get
            {
                // HACK but works with VSTS
                return this.GetProjectCollectionUri().Segments[1].Replace("/", string.Empty);
            }
        }

        public INotification Notification
        {
            get
            {
                return new Notification(this.CurrentWorkItemId, this.teamProjectCollectionUrl, this.teamProjectName);
            }
        }

        public int CurrentWorkItemId { get; set; }

        public string GetProjectName(Uri projectUri)
        {
            // no need to compute from URI
            return this.teamProjectName;
        }

        public IProjectProperty[] GetProjectProperties(Uri projectUri)
        {
            // TODO maybe one day we will implement...
            throw new NotImplementedException();
        }

        public IdentityDescriptor GetIdentityToImpersonate()
        {
            // makes no sense impersonation in WebHook, and it is not called anyway
            return null;
        }
    }
}
