using System;
using Aggregator.Core.Interfaces;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Server;

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
                return new Notification(this.CurrentWorkItemId, this.CurrentChangeType, this.teamProjectCollectionUrl, this.teamProjectName);
            }
        }

        public int CurrentWorkItemId { get; set; }

        public ChangeTypes CurrentChangeType { get; set; }

        public IVssRequestContext VssContext
        {
            get
            {
                throw new NotImplementedException("Cannot emulate IVssRequestContext in remote web service");
            }
        }

        public string GetProjectName(Uri teamProjectUri)
        {
            // no need to compute from URI
            return this.teamProjectName;
        }

        public IProjectProperty[] GetProjectProperties(Uri teamProjectUri)
        {
            // TODO maybe one day we will implement...
            throw new NotImplementedException();
        }

        public IdentityDescriptor GetIdentityToImpersonate(Uri projectCollectionUrl)
        {
            // makes no sense impersonation in WebHook, and it is not called anyway
            return null;
        }
    }
}
