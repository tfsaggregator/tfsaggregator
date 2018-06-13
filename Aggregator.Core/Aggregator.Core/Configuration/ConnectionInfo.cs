using System;

using AuthenticationToken = Aggregator.Core.Facade.WorkItemRepository.AuthenticationToken;

namespace Aggregator.Core.Configuration
{
    public class ConnectionInfo
    {
        public Uri ProjectCollectionUri { get; private set; }

        public AuthenticationToken Token { get; private set; }

        public ConnectionInfo(Uri uri, AuthenticationToken token)
        {
            this.ProjectCollectionUri = uri;
            this.Token = token;
        }
    }
}
